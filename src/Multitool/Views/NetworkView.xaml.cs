using System.Windows;
using System.Windows.Controls;
using Multitool.Services;

namespace Multitool.Views;

public partial class NetworkView : UserControl
{
    public NetworkView()
    {
        InitializeComponent();
        RefreshTexts();
        Lang.Changed += RefreshTexts;
    }

    private void RefreshTexts()
    {
        TxtTitle.Text = Lang.T("v_net_title");
        GrpIps.Header = Lang.T("v_net_ips");
        BtnIps.Content = Lang.T("v_net_show_ips");
        BtnDns.Content = Lang.T("v_net_resolve");
        GrpScan.Header = Lang.T("v_net_scan_ports");
        BtnScan.Content = Lang.T("v_net_scan");
    }

    private async void BtnIps_Click(object sender, RoutedEventArgs e)
    {
        OutIps.Text = $"{Lang.T("v_net_local")}: {NetworkTools.LocalIp()}\n{Lang.T("v_net_public")}: {Lang.T("v_net_loading")}";
        OutIps.Text = $"{Lang.T("v_net_local")}: {NetworkTools.LocalIp()}\n{Lang.T("v_net_public")}: {await NetworkTools.PublicIpAsync()}";
    }

    private async void BtnPing_Click(object sender, RoutedEventArgs e)
        => OutPing.Text = await NetworkTools.PingAsync(InPing.Text);

    private async void BtnDns_Click(object sender, RoutedEventArgs e)
        => OutDns.Text = await NetworkTools.DnsLookupAsync(InDns.Text);

    private async void BtnScan_Click(object sender, RoutedEventArgs e)
    {
        OutScan.Text = Lang.T("v_msg_scanning");
        OutScan.Text = await NetworkTools.ScanPortsAsync(InScan.Text, NetworkTools.CommonPorts);
    }
}
