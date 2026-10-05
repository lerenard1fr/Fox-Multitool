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
/// </summary>
public static class AppPaths
{
    public static string BaseDir => AppContext.BaseDirectory;
    public static string SettingsFile => Path.Combine(BaseDir, "settings.json");
    public static string ToolsDir => Path.Combine(BaseDir, "tools");
    public static string PortableNmap => Path.Combine(ToolsDir, "nmap", "nmap.exe");
    public static string LogsDir => Path.Combine(BaseDir, "logs");
    public static string DataDir => Path.Combine(BaseDir, "data");

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
