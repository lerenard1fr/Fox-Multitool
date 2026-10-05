using System.Windows;
using System.Windows.Controls;
using Multitool.Services;

namespace Multitool.Views;

public partial class SettingsView : UserControl
{
    private bool _ready;

    public SettingsView()
    {
        InitializeComponent();
        RefreshTexts();
        string t = App.Settings.Theme;
        RadioSystem.IsChecked = t == "System";
        RadioDark.IsChecked = t == "Dark";
        RadioLight.IsChecked = t == "Light";
        _ready = true;
    }

    private void RefreshTexts()
    {
        TxtTitle.Text = Lang.T("settings_title");
        GrpTheme.Header = Lang.T("settings_theme");
        RadioSystem.Content = Lang.T("settings_theme_system");
        RadioDark.Content = Lang.T("settings_theme_dark");
        RadioLight.Content = Lang.T("settings_theme_light");
        GrpLang.Header = Lang.T("settings_lang");
        TxtSaved.Text = Lang.T("settings_saved");
        TxtThemeNote.Text = Lang.T("settings_theme_note");
    }

    private void Theme_Checked(object sender, RoutedEventArgs e)
    {
        if (!_ready) return;
        string theme = RadioDark.IsChecked == true ? "Dark" : RadioLight.IsChecked == true ? "Light" : "System";
        App.Settings.Theme = theme;
        App.Settings.Save();
        ThemeManager.Apply(theme);
    }

    private void BtnEn_Click(object sender, RoutedEventArgs e) => SetLang("en");
    private void BtnFr_Click(object sender, RoutedEventArgs e) => SetLang("fr");

    private void SetLang(string lang)
    {
        App.Settings.Lang = lang;
        App.Settings.Save();
        Lang.Set(lang);
        RefreshTexts();
    }
}
