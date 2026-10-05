using System.Windows;
using System.Windows.Controls;
using Multitool.Services;

namespace Multitool.Views;

public partial class NetworkExtraView : UserControl
{
    public NetworkExtraView()
    {
        InitializeComponent();
        RefreshTexts();
        Lang.Changed += RefreshTexts;
    }

    private void RefreshTexts()
    {
        TxtTitle.Text = Lang.T("v_next_title");
        TxtSub.Text = Lang.T("v_next_sub");
        BtnWifi.Content = Lang.T("v_next_wifi");
    }

    private async void BtnTracert_Click(object sender, RoutedEventArgs e)
    {
        OutTracert.Text = Lang.T("v_next_tracing");
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
