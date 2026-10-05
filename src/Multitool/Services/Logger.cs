using System.IO;

namespace Multitool.Services;

/// <summary>Journal dans logs/ a cote de l'exe (portable).</summary>
public static class Logger
{
    private static readonly object Lock = new();

    public static void Info(string message) => Write("INFO", message);
    public static void Scan(string tool, string target, string resultFile)
        => Write("SCAN", $"{tool} cible={target} export={resultFile}");

    public static void Error(string message) => Write("ERROR", message);

    private static void Write(string level, string message)
    {
        try
        {
            Directory.CreateDirectory(AppPaths.LogsDir);
            string file = Path.Combine(AppPaths.LogsDir, $"app-{DateTime.Now:yyyy-MM-dd}.log");
            lock (Lock)
                File.AppendAllText(file, $"[{DateTime.Now:HH:mm:ss}] [{level}] {message}\n");
        }
        catch { }
    }
}
