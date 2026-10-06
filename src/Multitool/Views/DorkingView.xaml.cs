using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Multitool.Services;

namespace Multitool.Views;

public partial class DorkingView : UserControl
{
    public DorkingView()
    {
        InitializeComponent();
        foreach (var (name, _) in LookupTools.Engines)
            EngineBox.Items.Add(name);
        EngineBox.SelectedIndex = 0;
        foreach (var (name, template) in LookupTools.Dorks)
        {
            var b = new Button
            {
                Content = name, Tag = template,
                Style = (Style)FindResource("SecondaryButton"),
                Margin = new Thickness(0, 0, 8, 8),
            };
            b.Click += (s, _) => Open((string)((Button)s!).Tag!);
            DorkPanel.Children.Add(b);
        }
        RefreshTexts();
        Lang.Changed += RefreshTexts;
    }

    private void RefreshTexts()
    {
        TxtSub.Text = Lang.T("v_drk_sub");
        GrpMain.Header = Lang.T("v_drk_grp");
        BtnGo.Content = Lang.T("v_osi_search");
    }

    private void Open(string template)
    {
        string domain = InDomain.Text.Trim();
        if (string.IsNullOrWhiteSpace(domain)) return;
        string engine = LookupTools.Engines[EngineBox.SelectedIndex < 0 ? 0 : EngineBox.SelectedIndex].Url;
        try { Process.Start(new ProcessStartInfo(LookupTools.DorkUrl(engine, template.Replace("{domain}", domain))) { UseShellExecute = true }); }
        catch { }
    }

    private void BtnGo_Click(object sender, RoutedEventArgs e) => Open(InCustom.Text);
}
