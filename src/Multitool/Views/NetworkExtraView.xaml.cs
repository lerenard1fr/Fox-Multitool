using System.Windows;
using System.Windows.Controls;
using Multitool.Services;

namespace Multitool.Views;

public partial class NetworkExtraView : UserControl
{
    public NetworkExtraView() => InitializeComponent();

    private async void BtnTracert_Click(object sender, RoutedEventArgs e)
    {
        OutTracert.Text = "Traceroute en cours... (peut durer ~1 min)";
        OutTracert.Text = await NetExtraTools.TracertAsync(InTracert.Text);
    }

    private async void BtnArp_Click(object sender, RoutedEventArgs e)
    {
        OutExtra.Text = "arp -a ...";
        OutExtra.Text = await NetExtraTools.ArpAsync();
    }

    private async void BtnIpConfig_Click(object sender, RoutedEventArgs e)
    {
        OutExtra.Text = "ipconfig /all ...";
        OutExtra.Text = await NetExtraTools.IpConfigAsync();
    }

    private async void BtnNetstat_Click(object sender, RoutedEventArgs e)
    {
        OutExtra.Text = "netstat -ano ...";
        OutExtra.Text = await NetExtraTools.NetstatAsync();
    }

    private async void BtnWifi_Click(object sender, RoutedEventArgs e)
    {
        OutExtra.Text = "netsh wlan show profiles ...";
        OutExtra.Text = await NetExtraTools.WifiProfilesAsync();
    }
}
