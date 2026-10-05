using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Multitool.Services;

namespace Multitool.Views;

public partial class NmapScanView : UserControl
{
    public NmapScanView()
    {
        InitializeComponent();
        Loaded += async (_, _) => await RefreshStatusAsync();
    }

    private async Task RefreshStatusAsync()
    {
        string v = await NmapTools.VersionAsync();
        bool ok = NmapTools.FindNmap() is not null;
        ApplyStatus(ok, v);
    }

    private void ApplyStatus(bool ok, string detail)
    {
        DotStatus.Fill = ok ? Brushes.LimeGreen : Brushes.Red;
        TxtNmapStatus.Text = ok ? "● INSTALLÉ — prêt à scanner" : "● NON INSTALLÉ";
        TxtNmapStatus.Foreground = ok
            ? new SolidColorBrush(Color.FromRgb(0x3F, 0xB9, 0x50))
            : new SolidColorBrush(Color.FromRgb(0xF8, 0x51, 0x49));
        TxtNmapDetail.Text = detail.Split('\n')[0];
        StatusCard.BorderBrush = (Brush)TxtNmapStatus.Foreground;
        StatusCard.BorderThickness = new Thickness(1);
    }

    private async void BtnCheck_Click(object sender, RoutedEventArgs e) => await RefreshStatusAsync();

    private async void BtnWinget_Click(object sender, RoutedEventArgs e)
    {
        BtnWinget.IsEnabled = false;
        OutInstall.Text = "Installation via winget... (peut durer plusieurs minutes)";
        OutInstall.Text = await ToolInstaller.InstallNmapViaWingetAsync();
        await RefreshStatusAsync();
        BtnWinget.IsEnabled = true;
    }

    private async void BtnDownload_Click(object sender, RoutedEventArgs e)
    {
        BtnDownload.IsEnabled = false;
        DlProgress.Value = 0;
        OutInstall.Text = $"Téléchargement {ToolInstaller.NmapSetupUrl} ...";
        var prog = new Progress<double>(v => DlProgress.Value = v);
        string result = await ToolInstaller.DownloadNmapSetupAsync(prog);
        OutInstall.Text = File.Exists(result) ? ToolInstaller.LaunchInstaller(result) : result;
        if (File.Exists(result)) DlProgress.Value = 100;
        BtnDownload.IsEnabled = true;
    }

    private void BtnOpenPage_Click(object sender, RoutedEventArgs e) => ToolInstaller.OpenNmapPage();

    private async void BtnQuick_Click(object sender, RoutedEventArgs e) => await Run(() => NmapTools.QuickScanAsync(InTarget.Text));
    private async void BtnService_Click(object sender, RoutedEventArgs e) => await Run(() => NmapTools.ServiceScanAsync(InTarget.Text));
    private async void BtnOs_Click(object sender, RoutedEventArgs e) => await Run(() => NmapTools.OsScanAsync(InTarget.Text));
    private async void BtnFull_Click(object sender, RoutedEventArgs e) => await Run(() => NmapTools.FullScanAsync(InTarget.Text));
    private async void BtnPingSweep_Click(object sender, RoutedEventArgs e) => await Run(() => NmapTools.PingSweepAsync(InTarget.Text));
    private async void BtnPorts_Click(object sender, RoutedEventArgs e) => await Run(() => NmapTools.PortsScanAsync(InTarget.Text, InPorts.Text));

    private async Task Run(Func<Task<string>> fn)
    {
        OutNmap.Text = "Scan en cours... (peut durer 1-2 min)";
        OutNmap.Text = await fn();
    }
}
