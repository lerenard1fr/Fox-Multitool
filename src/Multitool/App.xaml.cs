using System.Windows;
using Multitool.Services;

namespace Multitool;

public partial class App : Application
{
    public static AppSettings Settings { get; private set; } = new();

    protected override void OnStartup(StartupEventArgs e)
    {
        AppPaths.EnsureAll(); // tools/ logs/ data/ a cote de l'exe
        Settings = AppSettings.Load();
        Lang.Set(Settings.Lang);
        ThemeManager.Apply(Settings.Theme);
        Logger.Info("Demarrage FoxMultitool");
        base.OnStartup(e);
    }
}
