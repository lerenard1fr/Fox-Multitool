using System.Diagnostics;
using System.IO;
using System.Net.Http;

namespace Multitool.Services;

public static class ToolInstaller
{
    public const string NmapSetupUrl = "https://nmap.org/dist/nmap-7.991-setup.exe";
    public const string NmapPageUrl = "https://nmap.org/download.html";

    public static Task<string> InstallNmapViaWingetAsync(int timeoutMs = 300000)
        => InstallViaWingetAsync("Insecure.Nmap", timeoutMs);

    public static async Task<string> InstallViaWingetAsync(string packageId, int timeoutMs = 300000)
    {
        if (string.IsNullOrWhiteSpace(packageId) || packageId.Any(c => c is '&' or '|' or ';' or '$' or '`'))
            return "Identifiant de paquet invalide.";
        try
        {
            var psi = new ProcessStartInfo("winget",
                $"install -e --id {packageId} --accept-source-agreements --accept-package-agreements --silent")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            using var p = Process.Start(psi);
            if (p is null) return Lang.T("svc_winget_run");
            var outTask = p.StandardOutput.ReadToEndAsync();
            var errTask = p.StandardError.ReadToEndAsync();
            using var cts = new CancellationTokenSource(timeoutMs);
            try { await p.WaitForExitAsync(cts.Token); }
            catch (OperationCanceledException)
            {
                try { p.Kill(); } catch { }
                return Lang.T("svc_winget_timeout");
            }
            string stdout = await outTask;
            string stderr = await errTask;
            return p.ExitCode == 0
                ? string.Format(Lang.T("svc_winget_ok"), packageId, stdout.Trim())
                : $"winget exit {p.ExitCode}\n{stdout.Trim()}\n{stderr.Trim()}".Trim();
        }
        catch (Exception ex)
        {
            return $"{Lang.T("svc_error")}{ex.Message}";
        }
    }

    public static async Task<string> DownloadNmapSetupAsync(IProgress<double>? progress = null, CancellationToken ct = default)
    {
        try
        {
            // Portable : le setup est telecharge dans tools/ a cote de l'exe, pas dans %TEMP%.
            Directory.CreateDirectory(AppPaths.ToolsDir);
            string dest = Path.Combine(AppPaths.ToolsDir, "nmap-7.991-setup.exe");
            using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
            using var resp = await http.GetAsync(NmapSetupUrl, HttpCompletionOption.ResponseHeadersRead, ct);
            resp.EnsureSuccessStatusCode();
            long? total = resp.Content.Headers.ContentLength;
            await using var net = await resp.Content.ReadAsStreamAsync(ct);
            await using var fs = File.Create(dest);
            var buf = new byte[81920];
            long read = 0;
            int n;
            while ((n = await net.ReadAsync(buf, ct)) > 0)
            {
                await fs.WriteAsync(buf.AsMemory(0, n), ct);
                read += n;
                if (total.HasValue && progress is not null)
                    progress.Report((double)read / total.Value * 100);
            }
            return dest;
        }
        catch (Exception ex)
        {
            return $"{Lang.T("svc_dl_err")}{ex.Message}";
        }
    }

    public static string LaunchInstaller(string setupPath)
    {
        try
        {
            // Lance avec UAC (l'installeur Nmap/Npcap demande admin)
            Process.Start(new ProcessStartInfo(setupPath) { UseShellExecute = true });
            return $"{Lang.T("v_nmap_launched")}{setupPath}\n{Lang.T("v_nmap_follow")}";
        }
        catch (Exception ex)
        {
            return $"{Lang.T("svc_launch_err")}{ex.Message}";
        }
    }

    public static void OpenNmapPage()
    {
        Process.Start(new ProcessStartInfo(NmapPageUrl) { UseShellExecute = true });
    }
}
