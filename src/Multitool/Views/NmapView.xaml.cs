using System.IO;
using System.Windows;
using System.Windows.Controls;
using Multitool.Services;

namespace Multitool.Views;

public partial class NmapView : UserControl
{
    public NmapView()
    {
        InitializeComponent();
        Loaded += async (_, _) => TxtNmapStatus.Text = await NmapTools.VersionAsync();
    }

    private async void BtnCheck_Click(object sender, RoutedEventArgs e)
        => TxtNmapStatus.Text = await NmapTools.VersionAsync();

    private async void BtnWinget_Click(object sender, RoutedEventArgs e)
    {
        BtnWinget.IsEnabled = false;
        OutInstall.Text = "Installation via winget... (peut durer plusieurs minutes)";
        OutInstall.Text = await ToolInstaller.InstallNmapViaWingetAsync();
        TxtNmapStatus.Text = await NmapTools.VersionAsync();
        BtnWinget.IsEnabled = true;
    }

    private async void BtnDownload_Click(object sender, RoutedEventArgs e)
    {
        BtnDownload.IsEnabled = false;
        DlProgress.Value = 0;
        OutInstall.Text = $"Téléchargement {ToolInstaller.NmapSetupUrl} ...";
        var prog = new Progress<double>(v => DlProgress.Value = v);
        string result = await ToolInstaller.DownloadNmapSetupAsync(prog);
        if (File.Exists(result))
        {
            DlProgress.Value = 100;
            OutInstall.Text = ToolInstaller.LaunchInstaller(result);
        }
        else
        {
            // result contient le message d'erreur
            OutInstall.Text = result;
        }
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

    private async void BtnNs_Click(object sender, RoutedEventArgs e) => await Dns("NS");
    private async void BtnMx_Click(object sender, RoutedEventArgs e) => await Dns("MX");
    private async void BtnTxt_Click(object sender, RoutedEventArgs e) => await Dns("TXT");
    private async void BtnA_Click(object sender, RoutedEventArgs e) => await Dns("A");

    private async void BtnPtr_Click(object sender, RoutedEventArgs e)
    {
        OutDns.Text = "Recherche PTR...";
        OutDns.Text = await DnsTools.ReverseAsync(InDns.Text);
    }

    private async Task Dns(string type)
    {
        OutDns.Text = $"Recherche {type}...";
        OutDns.Text = await DnsTools.LookupAsync(InDns.Text, type);
    }
}
