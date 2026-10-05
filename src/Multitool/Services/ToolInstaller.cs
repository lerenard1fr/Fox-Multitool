using System.Diagnostics;
using System.IO;
using System.Net.Http;

namespace Multitool.Services;

public static class ToolInstaller
{
    public const string NmapSetupUrl = "https://nmap.org/dist/nmap-7.991-setup.exe";
    public const string NmapPageUrl = "https://nmap.org/download.html";

    public static async Task<string> InstallNmapViaWingetAsync(int timeoutMs = 300000)
    {
        try
        {
            var psi = new ProcessStartInfo("winget",
                "install -e --id Insecure.Nmap --accept-source-agreements --accept-package-agreements --silent")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            using var p = Process.Start(psi);
            if (p is null) return "Impossible de lancer winget.";
            var outTask = p.StandardOutput.ReadToEndAsync();
            var errTask = p.StandardError.ReadToEndAsync();
            using var cts = new CancellationTokenSource(timeoutMs);
            try { await p.WaitForExitAsync(cts.Token); }
            catch (OperationCanceledException)
            {
                try { p.Kill(); } catch { }
                return "Timeout winget (>5 min). Réessaie ou utilise le setup.exe.";
            }
            string stdout = await outTask;
            string stderr = await errTask;
            return p.ExitCode == 0
                ? $"Nmap installé via winget.\n{stdout.Trim()}\nRelance la vérification Nmap."
                : $"winget exit {p.ExitCode}\n{stdout.Trim()}\n{stderr.Trim()}".Trim();
        }
        catch (Exception ex)
        {
            return $"Erreur winget: {ex.Message}";
        }
    }

    public static async Task<string> DownloadNmapSetupAsync(IProgress<double>? progress = null, CancellationToken ct = default)
    {
        try
        {
            string dest = Path.Combine(Path.GetTempPath(), "nmap-7.991-setup.exe");
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
            return $"Erreur téléchargement: {ex.Message}";
        }
    }

    public static string LaunchInstaller(string setupPath)
    {
        try
        {
            // Lance avec UAC (l'installeur Nmap/Npcap demande admin)
            Process.Start(new ProcessStartInfo(setupPath) { UseShellExecute = true });
            return $"Installeur lancé : {setupPath}\nSuis l'assistant, puis clique Vérifier Nmap.";
        }
        catch (Exception ex)
        {
            return $"Erreur lancement: {ex.Message}";
        }
    }

    public static void OpenNmapPage()
    {
        Process.Start(new ProcessStartInfo(NmapPageUrl) { UseShellExecute = true });
    }
}
