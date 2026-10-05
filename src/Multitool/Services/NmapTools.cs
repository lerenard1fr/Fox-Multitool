using System.Diagnostics;
using System.IO;
using System.Text;

namespace Multitool.Services;

public static class NmapTools
{
    public const string DownloadUrl = "https://nmap.org/download.html";

    public static string? FindNmap()
    {
        // 1. Portable : tools/nmap/nmap.exe a cote de l'exe (prioritaire)
        if (File.Exists(AppPaths.PortableNmap)) return AppPaths.PortableNmap;
        string[] candidates =
        [
            "nmap",
            @"C:\Program Files (x86)\Nmap\nmap.exe",
            @"C:\Program Files\Nmap\nmap.exe",
        ];
        foreach (var c in candidates)
        {
            try
            {
                if (c.Contains('\\'))
                {
                    if (File.Exists(c)) return c;
                }
                else
                {
                    // Test via where
                    var psi = new ProcessStartInfo("where", c)
                    {
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    };
                    using var p = Process.Start(psi);
                    if (p is null) continue;
                    string outp = p.StandardOutput.ReadToEnd();
                    p.WaitForExit(3000);
                    if (p.ExitCode == 0 && !string.IsNullOrWhiteSpace(outp))
                        return outp.Split('\n', StringSplitOptions.RemoveEmptyEntries)[0].Trim();
                }
            }
            catch { }
        }
        return null;
    }

    public static async Task<string> VersionAsync()
    {
        var nmap = FindNmap();
        if (nmap is null)
            return string.Format(Lang.T("svc_nonmap_ver"), DownloadUrl);
        return await RunAsync($"{nmap}", "--version", 10000);
    }

    public static Task<string> QuickScanAsync(string target) => RunNmapAsync(target, "-F");
    public static Task<string> ServiceScanAsync(string target) => RunNmapAsync(target, "-sV");
    public static Task<string> OsScanAsync(string target) => RunNmapAsync(target, "-O");
    public static Task<string> FullScanAsync(string target) => RunNmapAsync(target, "-A");
    public static Task<string> PingSweepAsync(string target) => RunNmapAsync(target, "-sn");
    public static Task<string> PortsScanAsync(string target, string ports) => RunNmapAsync(target, $"-p {ports} -sV");

    public static async Task<string> RunNmapAsync(string target, string args)
    {
        target = target.Trim();
        if (string.IsNullOrWhiteSpace(target))
            return Lang.T("svc_empty_target");
        // Garde-fou basique anti-injection
        if (target.Any(c => c is '&' or '|' or ';' or '$' or '`'))
            return Lang.T("svc_bad_target");
        var nmap = FindNmap();
        if (nmap is null)
            return string.Format(Lang.T("svc_nonmap"), DownloadUrl);
        return await RunAsync(nmap, $"{args} {target}", 120000);
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
            if (p is null) return Lang.T("svc_nmap_run");
            var outTask = p.StandardOutput.ReadToEndAsync();
            var errTask = p.StandardError.ReadToEndAsync();
            using var cts = new CancellationTokenSource(timeoutMs);
            try
            {
                await p.WaitForExitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                try { p.Kill(); } catch { }
                return Lang.T("svc_timeout");
            }
            string stdout = await outTask;
            string stderr = await errTask;
            if (!string.IsNullOrWhiteSpace(stderr) && string.IsNullOrWhiteSpace(stdout))
                return $"[stderr]\n{stderr.Trim()}";
            return string.IsNullOrWhiteSpace(stderr)
                ? stdout.Trim()
                : $"{stdout.Trim()}\n\n[stderr]\n{stderr.Trim()}".Trim();
        }
        catch (Exception ex)
        {
            return $"{Lang.T("svc_run_err")}{ex.Message}";
        }
    }
}
