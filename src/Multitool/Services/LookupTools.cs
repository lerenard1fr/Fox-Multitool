using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Multitool.Services;

public sealed record WalletInfo(bool Ok, decimal Btc, long TxCount);

/// <summary>Outils de lookup web/OSINT : tout est passif ou sur vos propres cibles.</summary>
public static class LookupTools
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(15) };

    static LookupTools()
    {
        Http.DefaultRequestHeaders.UserAgent.ParseAdd("FoxMultitool/1.0");
    }

    // ---------- URL Discovery Crawler ----------
    public static async Task<List<string>> CrawlAsync(string startUrl, int depth, bool sameDomain, int maxPages = 60)
    {
        var found = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!Uri.TryCreate(startUrl.Trim(), UriKind.Absolute, out var root)
            || (root.Scheme != Uri.UriSchemeHttp && root.Scheme != Uri.UriSchemeHttps))
            return found;
        var queue = new Queue<(Uri Uri, int Depth)>();
        queue.Enqueue((root, 0));
        seen.Add(Norm(root));
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(3));
        while (queue.Count > 0 && found.Count < maxPages && !cts.IsCancellationRequested)
        {
            var (uri, d) = queue.Dequeue();
            string html;
            try { html = await Http.GetStringAsync(uri, cts.Token); }
            catch { continue; }
            foreach (Match m in Regex.Matches(html, """(?:href|src)\s*=\s*["']?([^"'#\s>]+)""", RegexOptions.IgnoreCase))
            {
                if (!Uri.TryCreate(uri, m.Groups[1].Value.Trim(), out var abs)) continue;
                if (abs.Scheme != Uri.UriSchemeHttp && abs.Scheme != Uri.UriSchemeHttps) continue;
                if (sameDomain && !abs.Host.Equals(root.Host, StringComparison.OrdinalIgnoreCase)) continue;
                string key = Norm(abs);
                if (seen.Add(key))
                {
                    found.Add(abs.ToString());
                    if (d < depth && found.Count < maxPages) queue.Enqueue((abs, d + 1));
                }
            }
        }
        return found;
    }

    private static string Norm(Uri u) => u.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path,
        UriFormat.Unescaped).TrimEnd('/').ToLowerInvariant();

    // ---------- Host Discovery (ping sweep local) ----------
    public static async Task<List<(string Ip, string Name)>> HostDiscoveryAsync(string cidr)
    {
        var result = new List<(string Ip, string Name)>();
        if (!TryParseCidr(cidr.Trim(), out uint net, out int prefix) || prefix is < 16 or > 30)
            return result;
        uint count = 1u << (32 - prefix);
        if (count > 1024) return result;
        var sem = new SemaphoreSlim(64);
        var tasks = new List<Task>();
        for (uint i = 1; i < count - 1; i++)
        {
            uint addr = net + i;
            string ip = $"{(addr >> 24) & 0xFF}.{(addr >> 16) & 0xFF}.{(addr >> 8) & 0xFF}.{addr & 0xFF}";
            tasks.Add(Task.Run(async () =>
            {
                await sem.WaitAsync();
                try
                {
                    using var ping = new System.Net.NetworkInformation.Ping();
                    var reply = await ping.SendPingAsync(ip, 1200);
                    if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                    {
                        string name = "";
                        try { name = (await Dns.GetHostEntryAsync(ip)).HostName; } catch { }
                        lock (result) result.Add((ip, name));
                    }
                }
                catch { }
                finally { sem.Release(); }
            }));
        }
        await Task.WhenAll(tasks);
        result.Sort((a, b) => string.Compare(a.Ip, b.Ip, StringComparison.Ordinal));
        return result;
    }

    private static bool TryParseCidr(string cidr, out uint net, out int prefix)
    {
        net = 0; prefix = 0;
        var parts = cidr.Split('/');
        if (parts.Length != 2 || !IPAddress.TryParse(parts[0], out var ip)
            || !int.TryParse(parts[1], out prefix)) return false;
        var b = ip.GetAddressBytes();
        if (b.Length != 4) return false;
        net = ((uint)b[0] << 24) | ((uint)b[1] << 16) | ((uint)b[2] << 8) | b[3];
        uint mask = prefix == 0 ? 0 : 0xFFFFFFFF << (32 - prefix);
        net &= mask;
        return true;
    }

    // ---------- Dorking (requetes pretes, ouvertes dans le navigateur) ----------
    public static readonly (string Name, string Template)[] Dorks =
    [
        ("site:", "site:{domain}"),
        ("admin pages", "site:{domain} inurl:admin"),
        ("login pages", "site:{domain} inurl:login"),
        ("PDF files", "site:{domain} filetype:pdf"),
        ("SQL dumps", "site:{domain} filetype:sql"),
        ("config exposed", "site:{domain} inurl:config"),
        ("directory listing", "site:{domain} intitle:\"index of\""),
        ("logs", "site:{domain} filetype:log"),
    ];

    public static readonly (string Name, string Url)[] Engines =
    [
        ("Google", "https://www.google.com/search?q="),
        ("Bing", "https://www.bing.com/search?q="),
        ("DuckDuckGo", "https://duckduckgo.com/?q="),
    ];

    public static string DorkUrl(string engineUrl, string query)
        => engineUrl + Uri.EscapeDataString(query);

    // ---------- Email : format + MX + Gravatar ----------
    public static async Task<(bool FormatOk, bool DomainOk, string Mx, bool HasGravatar, string GravatarUrl)> EmailCheckAsync(string email)
    {
        email = email.Trim().ToLowerInvariant();
        bool formatOk;
        string domain = "";
        try { var m = new System.Net.Mail.MailAddress(email); domain = m.Host; formatOk = m.Address == email; }
        catch { return (false, false, "", false, ""); }
        bool domainOk;
        try { await Dns.GetHostEntryAsync(domain); domainOk = true; }
        catch { domainOk = false; }
        string mx = await DnsTools.LookupAsync(domain, "MX");
        byte[] hash = MD5.HashData(Encoding.UTF8.GetBytes(email));
        string gravUrl = $"https://www.gravatar.com/avatar/{Convert.ToHexString(hash).ToLowerInvariant()}?d=404&s=80";
        bool hasGravatar;
        try
        {
            using var resp = await Http.SendAsync(new HttpRequestMessage(HttpMethod.Head, gravUrl));
            hasGravatar = resp.IsSuccessStatusCode;
        }
        catch { hasGravatar = false; }
        return (formatOk, domainOk, mx, hasGravatar, gravUrl);
    }

    // ---------- Wallet : BTC via blockstream.info (sans cle) ----------
    public static async Task<WalletInfo> WalletBtcAsync(string address)
    {
        address = address.Trim();
        if (!Regex.IsMatch(address, @"^(bc1|[13])[a-zA-HJ-NP-Z0-9]{25,62}$"))
            return new WalletInfo(false, 0, 0);
        try
        {
            string json = await Http.GetStringAsync($"https://blockstream.info/api/address/{address}");
            using var doc = JsonDocument.Parse(json);
            var stats = doc.RootElement.GetProperty("chain_stats");
            long funded = stats.GetProperty("funded_txo_sum").GetInt64();
            long spent = stats.GetProperty("spent_txo_sum").GetInt64();
            long txs = stats.GetProperty("tx_count").GetInt64();
            return new WalletInfo(true, (funded - spent) / 100_000_000m, txs);
        }
        catch { return new WalletInfo(false, 0, 0); }
    }

    // ---------- Instagram : ouverture profil (login requis pour le reste) ----------
    public static string? InstagramUrl(string username)
    {
        username = username.Trim().TrimStart('@');
        if (string.IsNullOrWhiteSpace(username) || !Regex.IsMatch(username, @"^[A-Za-z0-9._]{1,30}$"))
            return null;
        return $"https://www.instagram.com/{username}/";
    }

    // ---------- Website Cloner (miroir leger, meme hote) ----------
    public static async Task<(string Folder, int Files)> CloneAsync(string startUrl)
    {
        if (!Uri.TryCreate(startUrl.Trim(), UriKind.Absolute, out var root)
            || (root.Scheme != Uri.UriSchemeHttp && root.Scheme != Uri.UriSchemeHttps))
            return ("", 0);
        string dir = Path.Combine(AppPaths.DataDir,
            $"clone-{AppPaths.SafeFileName(root.Host)}-{DateTime.Now:yyyyMMdd-HHmmss}");
        Directory.CreateDirectory(dir);
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        int count = 0;

        async Task<string?> SaveAsset(Uri u)
        {
            if (count >= 100) return null;
            string key = Norm(u);
            if (map.TryGetValue(key, out var existing)) return existing;
            byte[] data;
            try { data = await Http.GetByteArrayAsync(u); }
            catch { return null; }
            if (data.Length > 10_000_000) return null;
            string ext = Path.GetExtension(u.AbsolutePath);
            if (ext.Length is 0 or > 6 || !Regex.IsMatch(ext, @"^\.[A-Za-z0-9]+$")) ext = ".bin";
            string file = $"asset-{count}{ext}";
            await File.WriteAllBytesAsync(Path.Combine(dir, file), data);
            map[key] = file;
            count++;
            return file;
        }

        string html;
        try { html = await Http.GetStringAsync(root); }
        catch { return ("", 0); }
        await File.WriteAllTextAsync(Path.Combine(dir, "index.html"), html);
        count++;
        var assets = new HashSet<string>();
        foreach (Match m in Regex.Matches(html,
                     """(?:src|href)\s*=\s*["']?([^"'#\s>]+\.(?:css|js|png|jpe?g|gif|svg|ico|webp|woff2?))""",
                     RegexOptions.IgnoreCase))
        {
            if (Uri.TryCreate(root, m.Groups[1].Value, out var abs)
                && abs.Host.Equals(root.Host, StringComparison.OrdinalIgnoreCase))
                assets.Add(abs.ToString());
        }
        foreach (var a in assets)
        {
            if (!Uri.TryCreate(a, UriKind.Absolute, out var u)) continue;
            string? local = await SaveAsset(u);
            if (local is not null) html = html.Replace(a, local);
        }
        await File.WriteAllTextAsync(Path.Combine(dir, "index.html"), html);
        return (dir, count);
    }
}
