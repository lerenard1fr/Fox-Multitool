using System.Diagnostics;
using System.Text;

namespace Multitool.Services;

public static class DnsTools
{
    public static async Task<string> LookupAsync(string domain, string recordType)
    {
        domain = domain.Trim();
        if (string.IsNullOrWhiteSpace(domain))
            return Lang.T("svc_ns_empty");
        recordType = recordType.Trim().ToUpperInvariant();

        // Outil connu Windows : nslookup
        try
        {
            return await RunAsync("nslookup", $"-type={recordType} {domain}", 15000);
        }
        catch (Exception ex)
        {
            return $"{Lang.T("svc_ns_err")}{ex.Message}";
        }
    }

    public static async Task<string> ReverseAsync(string ip)
    {
        ip = ip.Trim();
        if (string.IsNullOrWhiteSpace(ip))
            return Lang.T("svc_ns_ipempty");
        try
        {
            return await RunAsync("nslookup", ip, 15000);
        }
        catch (Exception ex)
        {
            return $"{Lang.T("svc_ns_err")}{ex.Message}";
        }
    }

    private static async Task<string> RunAsync(string file, string args, int timeoutMs)
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
        if (p is null) return Lang.T("svc_ns_run");
        var outTask = p.StandardOutput.ReadToEndAsync();
        var errTask = p.StandardError.ReadToEndAsync();
        if (await Task.WhenAny(p.WaitForExitAsync(), Task.Delay(timeoutMs)) is not Task t || !p.HasExited)
        {
            try { p.Kill(); } catch { }
            return Lang.T("svc_ns_timeout");
        }
        string stdout = await outTask;
        string stderr = await errTask;
        return string.IsNullOrWhiteSpace(stderr) ? stdout.Trim() : $"{stdout.Trim()}\n{stderr.Trim()}".Trim();
    }
}
