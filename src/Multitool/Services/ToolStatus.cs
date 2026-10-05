using System.Diagnostics;

namespace Multitool.Services;

public record ToolState(string Name, bool Installed, string Detail);

public static class ToolStatus
{
    public static bool IsOnPath(string exe)
    {
        try
        {
            var psi = new ProcessStartInfo("where", exe)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            using var p = Process.Start(psi);
            if (p is null) return false;
            p.WaitForExit(3000);
            return p.ExitCode == 0;
        }
        catch { return false; }
    }

    public static List<ToolState> CheckAll()
    {
        string? nmap = NmapTools.FindNmap();
        return
        [
            new("nmap", nmap is not null, nmap ?? "non installé"),
            new("nslookup", IsOnPath("nslookup.exe"), "DNS Windows"),
            new("tracert", IsOnPath("tracert.exe"), "route"),
            new("arp", IsOnPath("arp.exe"), "table ARP"),
            new("ipconfig", IsOnPath("ipconfig.exe"), "config IP"),
            new("netstat", IsOnPath("netstat.exe"), "connexions"),
            new("winget", IsOnPath("winget.exe"), "installeur"),
        ];
    }
}
