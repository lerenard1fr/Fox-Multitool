using System.Windows;
using System.Windows.Controls;
using Multitool.Services;

namespace Multitool.Views;

public partial class NetworkView : UserControl
{
    public NetworkView() => InitializeComponent();

    private async void BtnIps_Click(object sender, RoutedEventArgs e)
    {
        OutIps.Text = $"Locale: {NetworkTools.LocalIp()}\nPublique: chargement...";
        OutIps.Text = $"Locale: {NetworkTools.LocalIp()}\nPublique: {await NetworkTools.PublicIpAsync()}";
    }

    private async void BtnPing_Click(object sender, RoutedEventArgs e)
        => OutPing.Text = await NetworkTools.PingAsync(InPing.Text);

    private async void BtnDns_Click(object sender, RoutedEventArgs e)
        => OutDns.Text = await NetworkTools.DnsLookupAsync(InDns.Text);

    private async void BtnScan_Click(object sender, RoutedEventArgs e)
    {
        OutScan.Text = "Scan en cours...";
        OutScan.Text = await NetworkTools.ScanPortsAsync(InScan.Text, NetworkTools.CommonPorts);
    }
}
