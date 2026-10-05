using System.Windows;
using Multitool.Services;

namespace Multitool;

public partial class App : Application
{
    public static AppSettings Settings { get; private set; } = new();

    protected override void OnStartup(StartupEventArgs e)
    {
        Settings = AppSettings.Load();
        Lang.Set(Settings.Lang);
        ThemeManager.Apply(Settings.Theme);
        base.OnStartup(e);
    }
}
