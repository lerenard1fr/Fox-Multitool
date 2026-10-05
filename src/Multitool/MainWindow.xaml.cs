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
        ExpInfo.Header = Lang.T("menu_info");
        ExpVuln.Header = Lang.T("menu_vuln");
        ExpWeb.Header = Lang.T("menu_web");
        ExpPass.Header = Lang.T("menu_pass");
        ExpWifi.Header = Lang.T("menu_wifi");
        ExpSniff.Header = Lang.T("menu_sniff");
        ExpExploit.Header = Lang.T("menu_exploit");
        ExpSocial.Header = Lang.T("menu_social");
        ExpSystem.Header = Lang.T("menu_system");
        ExpGen.Header = Lang.T("menu_generators");
        BtnSettings.Content = "⚙️  " + Lang.T("menu_options");
        BtnSecHub.Content = "🧰  " + Lang.T("nav_hub");
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
            nameof(BtnSecHub) => new SecurityView(),
            nameof(BtnSettings) => new SettingsView(),
            nameof(BtnSecAmass) => new SecurityView("amass"),
            nameof(BtnSecHarvester) => new SecurityView("harvester"),
            nameof(BtnSecNessus) => new SecurityView("nessus"),
            nameof(BtnSecNikto) => new SecurityView("nikto"),
            nameof(BtnSecBurp) => new SecurityView("burp"),
            nameof(BtnSecSqlmap) => new SecurityView("sqlmap"),
            nameof(BtnSecGobuster) => new SecurityView("gobuster"),
            nameof(BtnSecFfuf) => new SecurityView("ffuf"),
            nameof(BtnSecJohn) => new SecurityView("john"),
            nameof(BtnSecHashcat) => new SecurityView("hashcat"),
            nameof(BtnSecHydra) => new SecurityView("hydra"),
            nameof(BtnSecAircrack) => new SecurityView("aircrack"),
            nameof(BtnSecWireshark) => new SecurityView("wireshark"),
            nameof(BtnSecBettercap) => new SecurityView("bettercap"),
            nameof(BtnSecResponder) => new SecurityView("responder"),
            nameof(BtnSecNcat) => new SecurityView("ncat"),
            nameof(BtnSecMetasploit) => new SecurityView("metasploit"),
            nameof(BtnSecMaltego) => new SecurityView("maltego"),
            nameof(BtnSecSherlock) => new SecurityView("sherlock"),
            _ => MainContent.Content
        };
    }

    private void Highlight(Button active)
    {
        foreach (var b in new Button[] { BtnNetwork, BtnNmapScan, BtnDns, BtnNetExtra, BtnSystem, BtnOsint,
                     BtnGenerator, BtnSecHub, BtnSettings, BtnSecAmass, BtnSecHarvester, BtnSecNessus,
                     BtnSecNikto, BtnSecBurp, BtnSecSqlmap, BtnSecGobuster, BtnSecFfuf, BtnSecJohn,
                     BtnSecHashcat, BtnSecHydra, BtnSecAircrack, BtnSecWireshark, BtnSecBettercap,
                     BtnSecResponder, BtnSecNcat, BtnSecMetasploit, BtnSecMaltego, BtnSecSherlock })
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
