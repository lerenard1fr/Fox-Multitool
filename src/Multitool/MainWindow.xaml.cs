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
        MainContent.Content = new NetworkView();
        Highlight(BtnNetwork);
        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        TxtStatus.Text = "Vérification outils...";
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
            ? $"● nmap OK ({nmap!.Detail})  •  {count}/{tools.Count} outils dispo"
            : $"● nmap MANQUANT — va dans Réseau > Nmap Scan pour l'installer  •  {count}/{tools.Count} outils dispo";
        TxtStatus.Foreground = ok ? new SolidColorBrush(Color.FromRgb(0x3F, 0xB9, 0x50)) : new SolidColorBrush(Color.FromRgb(0xF8, 0x51, 0x49));
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
            _ => MainContent.Content
        };
    }

    private void Highlight(Button active)
    {
        foreach (var b in new[] { BtnNetwork, BtnNmapScan, BtnDns, BtnNetExtra, BtnSystem, BtnOsint, BtnGenerator })
        {
            b.Background = Brushes.Transparent;
            b.Foreground = new SolidColorBrush(Color.FromRgb(0xC9, 0xD1, 0xD9));
            b.FontWeight = FontWeights.Normal;
        }
        active.Background = new SolidColorBrush(Color.FromRgb(0x21, 0x26, 0x2D));
        active.Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0x7A, 0x00));
        active.FontWeight = FontWeights.SemiBold;
    }
}
