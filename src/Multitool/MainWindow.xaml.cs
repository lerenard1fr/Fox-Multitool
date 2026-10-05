using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Multitool.Services;
using Multitool.Views;

namespace Multitool;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        RefreshTexts();
        Lang.Changed += RefreshTexts;
        MainContent.Content = new NetworkView();
        Highlight(BtnNetwork);
        Loaded += MainWindow_Loaded;
    }

    private void RefreshTexts()
    {
        ExpNetwork.Header = Lang.T("menu_network");
        ExpSystem.Header = Lang.T("menu_system");
        ExpOsint.Header = Lang.T("menu_osint");
        ExpGen.Header = Lang.T("menu_generators");
        BtnSettings.Content = "⚙️  " + Lang.T("menu_options");
        BtnNetwork.Content = Lang.T("nav_base");
        BtnNmapScan.Content = Lang.T("nav_nmap");
        BtnDns.Content = Lang.T("nav_dns");
        BtnNetExtra.Content = Lang.T("nav_extra");
        BtnSystem.Content = Lang.T("nav_sys");
        BtnOsint.Content = Lang.T("nav_osint");
        BtnGenerator.Content = Lang.T("nav_gen");
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        TxtStatus.Text = Lang.T("status_checking");
        await Task.Run(() =>
        {
            var tools = ToolStatus.CheckAll();
            Dispatcher.Invoke(() => ApplyStatus(tools));
        });
    }

    private void ApplyStatus(List<ToolState> tools)
    {
        var nmap = tools.FirstOrDefault(t => t.Name == "nmap");
        bool ok = nmap?.Installed == true;
        DotNmap.Fill = ok ? Brushes.LimeGreen : Brushes.Red;
        int count = tools.Count(t => t.Installed);
        TxtStatus.Text = ok
            ? string.Format(Lang.T("status_ok"), nmap!.Detail, count, tools.Count)
            : string.Format(Lang.T("status_missing"), "", count, tools.Count);
        TxtStatus.Foreground = ok
            ? (Brush)FindResource("GreenFg")
            : (Brush)FindResource("RedFg");
        // Refresh if language changed later
        Lang.Changed += () => ApplyStatus(tools);
    }

    private void Nav_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        Highlight(btn);

        MainContent.Content = btn.Name switch
        {
            nameof(BtnNetwork) => new NetworkView(),
            nameof(BtnNmapScan) => new NmapScanView(),
            nameof(BtnDns) => new DnsLookupView(),
            nameof(BtnNetExtra) => new NetworkExtraView(),
            nameof(BtnSystem) => new SystemView(),
            nameof(BtnOsint) => new OsintView(),
            nameof(BtnGenerator) => new GeneratorView(),
            nameof(BtnSettings) => new SettingsView(),
            _ => MainContent.Content
        };
    }

    private void Highlight(Button active)
    {
        foreach (var b in new[] { BtnNetwork, BtnNmapScan, BtnDns, BtnNetExtra, BtnSystem, BtnOsint, BtnGenerator, BtnSettings })
        {
            b.Background = Brushes.Transparent;
            b.Foreground = (Brush)FindResource("FgSub");
            b.FontWeight = FontWeights.Normal;
        }
        active.Background = (Brush)FindResource("BgHover");
        active.Foreground = (Brush)FindResource("Accent");
        active.FontWeight = FontWeights.SemiBold;
    }
}
