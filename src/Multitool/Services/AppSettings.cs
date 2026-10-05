using System.IO;
using System.Text.Json;

namespace Multitool.Services;

public sealed class AppSettings
{
    public string Theme { get; set; } = "System"; // System | Dark | Light
    public string Lang { get; set; } = "en";      // en | fr

    // Portable : settings.json a cote de l'exe (voir AppPaths), le dossier se deplace tel quel.
    private static string FilePath => AppPaths.SettingsFile;

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch { }
        return new AppSettings();
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { }
    }
}
