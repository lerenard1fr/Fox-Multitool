using System.IO;

namespace Multitool.Services;

/// <summary>
/// Dossier portable : tout vit a cote de l'exe. Le dossier se deplace/copie tel quel.
/// FoxMultitool/
///   FoxMultitool.exe
///   settings.json
///   tools/   (ex. tools/nmap/nmap.exe detecte en priorite)
///   logs/    (journal scans + erreurs)
///   data/    (exports : resultats nmap, cibles...)
/// Si le dossier de l'exe n'est pas inscriptible (ex. Program Files via MSI),
/// repli sur %LocalAppData%/FoxMultitool.
/// </summary>
public static class AppPaths
{
    private static readonly string ResolvedBase = ResolveBase();

    public static string BaseDir => ResolvedBase;
    public static string SettingsFile => Path.Combine(BaseDir, "settings.json");
    public static string ToolsDir => Path.Combine(BaseDir, "tools");
    public static string PortableNmap => Path.Combine(ToolsDir, "nmap", "nmap.exe");
    public static string LogsDir => Path.Combine(BaseDir, "logs");
    public static string DataDir => Path.Combine(BaseDir, "data");

    private static string ResolveBase()
    {
        string exeDir = AppContext.BaseDirectory;
        if (IsWritable(exeDir)) return exeDir;
        string fallback = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FoxMultitool");
        try { Directory.CreateDirectory(fallback); } catch { }
        return fallback;
    }

    private static bool IsWritable(string dir)
    {
        try
        {
            Directory.CreateDirectory(dir);
            string probe = Path.Combine(dir, ".writetest");
            File.WriteAllText(probe, "ok");
            File.Delete(probe);
            return true;
        }
        catch { return false; }
    }

    public static void EnsureAll()
    {
        try
        {
            Directory.CreateDirectory(ToolsDir);
            Directory.CreateDirectory(LogsDir);
            Directory.CreateDirectory(DataDir);
        }
        catch { }
    }

    public static string SafeFileName(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name.Trim().Trim('.');
    }
}
