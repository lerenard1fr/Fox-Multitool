using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Multitool.Services;

public static class NetworkTools
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(10) };

    public static string LocalIp()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        var ip = host.AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
        return ip?.ToString() ?? "Introuvable";
    }

    public static async Task<string> PublicIpAsync()
    {
        try { return (await Http.GetStringAsync("https://api.ipify.org")).Trim(); }
        catch (Exception ex) { return $"Erreur: {ex.Message}"; }
    }

    public static async Task<string> PingAsync(string host)
    {
        try
        {
            using var p = new Ping();
            var r = await p.SendPingAsync(host, 2000);
            return r.Status == IPStatus.Success
                ? $"OK - {r.Address} en {r.RoundtripTime} ms (TTL={r.Options?.Ttl})"
                : $"Échec: {r.Status}";
        }
        catch (Exception ex) { return $"Erreur: {ex.Message}"; }
    }

    public static async Task<string> DnsLookupAsync(string host)
    {
        try
        {
            var e = await Dns.GetHostEntryAsync(host);
            return string.Join("\n", e.AddressList.Select(a => a.ToString()));
        }
        catch (Exception ex) { return $"Erreur: {ex.Message}"; }
    }

    public static async Task<string> ScanPortsAsync(string host, int[] ports, int timeoutMs = 800)
    {
        var open = new List<string>();
        var tasks = ports.Select(async port =>
        {
            try
            {
                using var c = new TcpClient();
                var t = c.ConnectAsync(host, port);
                if (await Task.WhenAny(t, Task.Delay(timeoutMs)) == t && c.Connected)
                    lock (open) open.Add($"{port} ouvert");
            }
            catch { /* fermé */ }
        });
        await Task.WhenAll(tasks);
        return open.Count == 0 ? "Aucun port ouvert (parmi ceux testés)." : string.Join("\n", open.OrderBy(s => s));
    }

    public static int[] CommonPorts => [21, 22, 23, 25, 53, 80, 110, 143, 443, 445, 3306, 3389, 8080];
}
