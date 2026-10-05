using System.Windows;
using Microsoft.Win32;

namespace Multitool.Services;

public static class ThemeManager
{
    public const string DarkPath = "Themes/Dark.xaml";
    public const string LightPath = "Themes/Light.xaml";

    public static bool IsSystemLight()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            if (key?.GetValue("AppsUseLightTheme") is int v)
                return v == 1;
        }
        catch { }
        return false; // default dark
    }

    public static string Resolve(string theme) => theme switch
    {
        "Dark" => "Dark",
        "Light" => "Light",
        _ => IsSystemLight() ? "Light" : "Dark",
    };

    public static void Apply(string theme)
    {
        string mode = Resolve(theme);
        string path = mode == "Light" ? LightPath : DarkPath;
        try
        {
            var dict = new ResourceDictionary { Source = new Uri(path, UriKind.Relative) };
            var app = Application.Current.Resources;
            // Remove old theme dictionaries
            for (int i = app.MergedDictionaries.Count - 1; i >= 0; i--)
            {
                var src = app.MergedDictionaries[i].Source?.OriginalString ?? "";
                if (src.Contains("Themes/Dark.xaml") || src.Contains("Themes/Light.xaml"))
                    app.MergedDictionaries.RemoveAt(i);
            }
            app.MergedDictionaries.Add(dict);
        }
        catch { }
    }
}
