using System.Diagnostics;
using System.IO;

namespace Multitool.Services;

public enum SecInstallKind { Winget, Page, Bundled }
public enum SecLaunchKind { Gui, ConsoleHelp, Browser, None }

public sealed class SecTool
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Category { get; init; } = "";
    public string Description { get; init; } = "";
    public string[] ExeNames { get; init; } = [];
    public string[] ExtraPaths { get; init; } = [];
    public SecInstallKind InstallKind { get; init; }
    public string InstallRef { get; init; } = ""; // winget id ou URL ou note
    public SecLaunchKind LaunchKind { get; init; } = SecLaunchKind.ConsoleHelp;
    public string LaunchRef { get; init; } = ""; // URL si Browser
    public string Notes { get; init; } = "";
    public string? FoundPath { get; set; }
    public bool Installed => FoundPath is not null;
}

/// <summary>
/// Catalogue d'outils de securite connus : detection locale (rouge/vert),
/// installation guidee (winget ou page officielle verifiee) et lancement.
/// Usage defensif : audit de vos propres systemes uniquement.
/// </summary>
public static class SecurityTools
{
    public static readonly List<SecTool> Catalog =
    [
        new() { Id = "wireshark", Name = "Wireshark", Category = "Sniffer",
            Description = "Analyseur de paquets reseau. Capturez et inspectez votre propre trafic.",
            ExeNames = ["wireshark.exe", "tshark.exe"],
            ExtraPaths = [@"C:\Program Files\Wireshark\Wireshark.exe"],
            InstallKind = SecInstallKind.Winget, InstallRef = "WiresharkFoundation.Wireshark",
            LaunchKind = SecLaunchKind.Gui },
        new() { Id = "burp", Name = "Burp Suite Community", Category = "Web",
            Description = "Proxy d'audit d'applications web. Testez vos propres sites.",
            ExeNames = ["BurpSuiteCommunity.exe", "BurpSuitePro.exe"],
            ExtraPaths = [@"C:\Program Files\BurpSuiteCommunity\BurpSuiteCommunity.exe"],
            InstallKind = SecInstallKind.Winget, InstallRef = "PortSwigger.BurpSuite.Community",
            LaunchKind = SecLaunchKind.Gui },
        new() { Id = "nessus", Name = "Nessus", Category = "Vuln Scanner",
            Description = "Scanner de vulnerabilites. Auditez vos propres machines (interface web locale).",
            ExeNames = ["nessuscli.exe"],
            ExtraPaths = [@"C:\Program Files\Tenable\Nessus\nessuscli.exe"],
            InstallKind = SecInstallKind.Winget, InstallRef = "Tenable.Nessus",
            LaunchKind = SecLaunchKind.Browser, LaunchRef = "https://localhost:8834" },
        new() { Id = "metasploit", Name = "Metasploit", Category = "Exploitation",
            Description = "Framework de test d'intrusion. Labo local uniquement (Metasploitable).",
            ExeNames = ["msfconsole.bat", "msfconsole"],
            ExtraPaths = [@"C:\metasploit-framework\bin\msfconsole.bat"],
            InstallKind = SecInstallKind.Page, InstallRef = "https://www.metasploit.com/download" },
        new() { Id = "nikto", Name = "Nikto", Category = "Web",
            Description = "Scanner de serveur web (failles connues). Vos serveurs uniquement.",
            ExeNames = ["nikto.pl", "nikto"],
            InstallKind = SecInstallKind.Page, InstallRef = "https://github.com/sullo/nikto",
            Notes = "Necessite Perl (ou Docker)." },
        new() { Id = "john", Name = "John the Ripper", Category = "Password",
            Description = "Audit de robustesse de mots de passe. Vos propres hashes uniquement.",
            ExeNames = ["john.exe", "john"],
            InstallKind = SecInstallKind.Page, InstallRef = "https://www.openwall.com/john/" },
        new() { Id = "hashcat", Name = "Hashcat", Category = "Password",
            Description = "Recuperation de mots de passe (CPU/GPU). Vos propres hashes uniquement.",
            ExeNames = ["hashcat.exe"],
            InstallKind = SecInstallKind.Page, InstallRef = "https://hashcat.net/hashcat/" },
        new() { Id = "hydra", Name = "Hydra", Category = "Password",
            Description = "Test de robustesse d'authentification multi-protocoles. Vos services uniquement.",
            ExeNames = ["hydra.exe"],
            InstallKind = SecInstallKind.Page, InstallRef = "https://github.com/vanhauser-thc/thc-hydra",
            Notes = "Windows via Cygwin ; image Docker disponible." },
        new() { Id = "sqlmap", Name = "sqlmap", Category = "Web",
            Description = "Detection d'injection SQL. Vos applications uniquement.",
            ExeNames = ["sqlmap", "sqlmap.py"],
            InstallKind = SecInstallKind.Page, InstallRef = "https://github.com/sqlmapproject/sqlmap",
            Notes = "Necessite Python." },
        new() { Id = "gobuster", Name = "Gobuster", Category = "Web",
            Description = "Enumeration de dossiers, DNS et vhosts (Go, rapide). Vos cibles uniquement.",
            ExeNames = ["gobuster.exe"],
            InstallKind = SecInstallKind.Page, InstallRef = "https://github.com/OJ/gobuster",
            Notes = "Binaires Windows precompiles dans Releases." },
        new() { Id = "ffuf", Name = "ffuf", Category = "Web",
            Description = "Fuzzer web rapide. Vos applications uniquement.",
            ExeNames = ["ffuf.exe"],
            InstallKind = SecInstallKind.Winget, InstallRef = "ffuf.ffuf" },
        new() { Id = "ncat", Name = "Netcat (ncat)", Category = "Network",
            Description = "Couteau suisse TCP/UDP. Inclus avec Nmap.",
            ExeNames = ["ncat.exe", "nc.exe"],
            InstallKind = SecInstallKind.Bundled, InstallRef = "Inclus avec Nmap" },
        new() { Id = "aircrack", Name = "Aircrack-ng", Category = "WiFi",
            Description = "Audit de securite WiFi. Votre propre reseau uniquement.",
            ExeNames = ["aircrack-ng.exe", "airodump-ng.exe"],
            InstallKind = SecInstallKind.Page, InstallRef = "https://www.aircrack-ng.org/",
            Notes = "Necessite un adaptateur WiFi compatible (mode monitor, surtout Linux).",
            LaunchKind = SecLaunchKind.Gui },
        new() { Id = "amass", Name = "Amass (OWASP)", Category = "OSINT",
            Description = "Cartographie de surface d'attaque et enumeration de sous-domaines.",
            ExeNames = ["amass.exe"],
            InstallKind = SecInstallKind.Winget, InstallRef = "OWASP.Amass" },
        new() { Id = "theharvester", Name = "theHarvester", Category = "OSINT",
            Description = "Collecte d'emails, sous-domaines et noms (sources publiques).",
            ExeNames = ["theHarvester.py", "theharvester"],
            InstallKind = SecInstallKind.Page, InstallRef = "https://github.com/laramies/theHarvester",
            Notes = "Necessite Python 3.14 + uv." },
        new() { Id = "maltego", Name = "Maltego", Category = "OSINT",
            Description = "Analyse graphique de liens (OSINT visuel).",
            ExeNames = ["Maltego.exe"],
            ExtraPaths = [@"C:\Program Files\Maltego\Maltego.exe"],
            InstallKind = SecInstallKind.Winget, InstallRef = "Maltego.Maltego",
            LaunchKind = SecLaunchKind.Gui },
        new() { Id = "sherlock", Name = "Sherlock", Category = "OSINT",
            Description = "Recherche d'un pseudo sur les reseaux sociaux.",
            ExeNames = ["sherlock", "sherlock.exe"],
            InstallKind = SecInstallKind.Page, InstallRef = "https://github.com/sherlock-project/sherlock",
            Notes = "pipx install sherlock-project." },
        new() { Id = "bettercap", Name = "Bettercap", Category = "Network",
            Description = "Reconnaissance reseau et MITM (labo local uniquement).",
            ExeNames = ["bettercap.exe"],
            InstallKind = SecInstallKind.Page, InstallRef = "https://github.com/bettercap/bettercap",
            Notes = "Binaires Windows precompiles dans Releases." },
        new() { Id = "responder", Name = "Responder", Category = "Network",
            Description = "Empoisonnement LLMNR/NBT-NS (labo isole uniquement).",
            ExeNames = ["Responder.py"],
            InstallKind = SecInstallKind.Page, InstallRef = "https://github.com/lgandx/Responder",
            Notes = "Linux + root uniquement — ne tourne pas sur Windows.",
            LaunchKind = SecLaunchKind.None },
    ];

    public static void DetectAll()
    {
        foreach (var t in Catalog)
            t.FoundPath = FindExe(t);
    }

    private static string? FindExe(SecTool t)
    {
        foreach (var name in t.ExeNames)
        {
            try
            {
                var psi = new ProcessStartInfo("where", name)
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                using var p = Process.Start(psi);
                if (p is null) continue;
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit(3000);
                if (p.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
                    return output.Split('\n', StringSplitOptions.RemoveEmptyEntries)[0].Trim();
            }
            catch { }
        }
        foreach (var path in t.ExtraPaths)
        {
            try { if (File.Exists(path)) return path; } catch { }
        }
        // ncat est fourni avec nmap : cherche a cote du binaire nmap detecte.
        if (t.Id == "ncat")
        {
            try
            {
                var nmap = NmapTools.FindNmap();
                if (nmap is not null)
                {
                    string? dir = Path.GetDirectoryName(nmap.Trim('"'));
                    if (dir is not null)
                    {
                        string candidate = Path.Combine(dir, "ncat.exe");
                        if (File.Exists(candidate)) return candidate;
                    }
                }
            }
            catch { }
        }
        return null;
    }

    public static async Task<string> InstallAsync(SecTool t)
    {
        return t.InstallKind switch
        {
            SecInstallKind.Winget => await ToolInstaller.InstallViaWingetAsync(t.InstallRef),
            SecInstallKind.Page => OpenPage(t.InstallRef),
            _ => t.InstallRef,
        };
    }

    private static string OpenPage(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            return $"{Lang.T("v_sec_opened")}{url}";
        }
        catch (Exception ex) { return $"{Lang.T("svc_error")}{ex.Message}"; }
    }

    public static string Launch(SecTool t)
    {
        if (!t.Installed || t.FoundPath is null) return Lang.T("v_sec_notfound");
        try
        {
            switch (t.LaunchKind)
            {
                case SecLaunchKind.Gui:
                    Process.Start(new ProcessStartInfo(t.FoundPath) { UseShellExecute = true });
                    return $"{Lang.T("v_sec_launched")}{t.FoundPath}";
                case SecLaunchKind.Browser:
                    Process.Start(new ProcessStartInfo(t.LaunchRef) { UseShellExecute = true });
                    return $"{Lang.T("v_sec_opened")}{t.LaunchRef}";
                case SecLaunchKind.None:
                    return t.Notes;
                default:
                {
                    string target = t.FoundPath.Trim('"');
                    string file, args;
                    if (target.EndsWith(".py", StringComparison.OrdinalIgnoreCase))
                        (file, args) = ("python", $"\"{target}\" --help");
                    else if (target.EndsWith(".pl", StringComparison.OrdinalIgnoreCase))
                        (file, args) = ("cmd", $"/k \"{target}\"");
                    else if (target.EndsWith(".bat", StringComparison.OrdinalIgnoreCase))
                        (file, args) = ("cmd", $"/k \"{target}\" --help");
                    else
                        (file, args) = ("cmd", $"/k \"{target}\" --help");
                    Process.Start(new ProcessStartInfo(file, args) { UseShellExecute = false, CreateNoWindow = false });
                    return $"{Lang.T("v_sec_launched")}{t.Name} --help";
                }
            }
        }
        catch (Exception ex) { return $"{Lang.T("svc_launch_err")}{ex.Message}"; }
    }
}
