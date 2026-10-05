using System.Diagnostics;
using System.Text;

namespace Multitool.Services;

public static class NetExtraTools
{
    public static Task<string> TracertAsync(string host)
        => RunAsync("tracert", $"-d {Sanitize(host)}", 120000);

    public static Task<string> ArpAsync() => RunAsync("arp", "-a", 15000);
    public static Task<string> IpConfigAsync() => RunAsync("ipconfig", "/all", 15000);
    public static Task<string> NetstatAsync() => RunAsync("netstat", "-ano", 20000);
    public static Task<string> WifiProfilesAsync() => RunAsync("netsh", "wlan show profiles", 15000);

    private static string Sanitize(string host)
    {
        host = host.Trim();
        if (string.IsNullOrWhiteSpace(host)) return "127.0.0.1";
        if (host.Any(c => c is '&' or '|' or ';' or '$' or '`' or '\n' or '\r'))
            return "127.0.0.1";
        return host;
    }

    private static async Task<string> RunAsync(string file, string args, int timeoutMs)
    {
        try
        {
            var psi = new ProcessStartInfo(file, args)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };
            using var p = Process.Start(psi);
            if (p is null) return $"{string.Format(Lang.T("svc_tool_err"), file)}{Lang.T("svc_unavailable")}";
            var outTask = p.StandardOutput.ReadToEndAsync();
            var errTask = p.StandardError.ReadToEndAsync();
            using var cts = new CancellationTokenSource(timeoutMs);
            try { await p.WaitForExitAsync(cts.Token); }
            catch (OperationCanceledException)
            {
                try { p.Kill(); } catch { }
                return "Timeout.";
            }
            string stdout = await outTask;
            string stderr = await errTask;
            return string.IsNullOrWhiteSpace(stderr) ? stdout.Trim() : $"{stdout.Trim()}\n{stderr.Trim()}".Trim();
        }
        catch (Exception ex)
        {
            return $"{string.Format(Lang.T("svc_tool_err"), file)}{ex.Message}";
        }
    }
}
