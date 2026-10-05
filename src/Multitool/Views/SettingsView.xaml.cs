using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Multitool.Services;

namespace Multitool.Views;

public partial class SettingsView : UserControl
{
    private bool _ready;
    private bool _updating;

    public SettingsView()
    {
        InitializeComponent();
        RefreshTexts();
        string t = App.Settings.Theme;
        RadioSystem.IsChecked = t == "System";
        RadioDark.IsChecked = t == "Dark";
        RadioLight.IsChecked = t == "Light";
        RestoreFullList();
        // Filtrage uniquement sur frappe clavier, jamais sur selection.
        LangCombo.ApplyTemplate();
        if (LangCombo.Template.FindName("PART_EditableTextBox", LangCombo) is TextBox tb)
        {
            tb.PreviewTextInput += (_, _) => Dispatcher.BeginInvoke(() => FilterLang(tb.Text));
            tb.PreviewKeyDown += (_, e) =>
            {
                if (e.Key is Key.Back or Key.Delete)
                    Dispatcher.BeginInvoke(() => FilterLang(tb.Text));
            };
        }
        _ready = true;
    }

    private void RestoreFullList()
    {
        _updating = true;
        LangCombo.Items.Clear();
        foreach (var (code, label) in Lang.Languages)
            LangCombo.Items.Add(new ComboBoxItem { Tag = code, Content = label });
        foreach (ComboBoxItem item in LangCombo.Items)
            if ((string)item.Tag == App.Settings.Lang) { LangCombo.SelectedItem = item; break; }
        LangCombo.SelectedItem ??= LangCombo.Items[0];
        _updating = false;
    }

    private void FilterLang(string query)
    {
        if (!_ready || _updating) return;
        query = query.Trim().ToLowerInvariant();
        _updating = true;
        LangCombo.Items.Clear();
        foreach (var (code, label) in Lang.Languages)
            if (string.IsNullOrEmpty(query) || label.ToLowerInvariant().Contains(query) || code.ToLowerInvariant().Contains(query))
                LangCombo.Items.Add(new ComboBoxItem { Tag = code, Content = label });
        _updating = false;
        if (LangCombo.Items.Count > 0)
            LangCombo.IsDropDownOpen = true;
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

    private void LangCombo_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (!_ready || _updating) return;
        if (LangCombo.SelectedItem is ComboBoxItem item && item.Tag is string lang)
        {
            SetLang(lang);
            LangCombo.IsDropDownOpen = false;
            RestoreFullList(); // liste complete pour la prochaine ouverture
        }
    }

    private void SetLang(string lang)
    {
        App.Settings.Lang = lang;
        App.Settings.Save();
        Lang.Set(lang);
        RefreshTexts();
    }
}
