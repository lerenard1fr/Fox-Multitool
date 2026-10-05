using System.Diagnostics;
using System.IO;

namespace Multitool.Services;

public static class SystemTools
{
    public static string SystemInfo()
    {
        return $"Machine: {Environment.MachineName}\n" +
               $"OS: {Environment.OSVersion}\n" +
               $"64-bit OS: {Environment.Is64BitOperatingSystem}\n" +
               $"User: {Environment.UserName}\n" +
               $"CPU: {Environment.ProcessorCount} {Lang.T("sys_cores")}\n" +
               $"Uptime: {TimeSpan.FromMilliseconds(Environment.TickCount64):d\\j\\ hh\\:mm\\:ss}\n" +
               $"{Lang.T("sys_folder")}: {Environment.CurrentDirectory}";
    }

    public static string TopProcesses(int count = 15)
    {
        try
        {
            var list = Process.GetProcesses()
                .OrderByDescending(p => { try { return p.WorkingSet64; } catch { return 0; } })
                .Take(count)
                .Select(p =>
                {
                    try { return $"{p.ProcessName,-25} PID {p.Id,-7} {(p.WorkingSet64 / 1024 / 1024)} Mo"; }
                    catch { return $"{p.ProcessName} ({Lang.T("sys_denied")})"; }
                });
            return string.Join("\n", list);
        }
        catch (Exception ex) { return $"{Lang.T("svc_error")}{ex.Message}"; }
    }

    public static string CleanTemp()
    {
        int deleted = 0, failed = 0;
        string tmp = Path.GetTempPath();
        try
        {
            foreach (var f in Directory.EnumerateFiles(tmp, "*", SearchOption.TopDirectoryOnly))
            {
                try { File.Delete(f); deleted++; }
                catch { failed++; }
            }
            return $"Temp: {tmp}\n{Lang.T("sys_deleted")}: {deleted}\n{Lang.T("sys_failed")}: {failed}";
        }
        catch (Exception ex) { return $"{Lang.T("svc_error")}{ex.Message}"; }
    }
}
