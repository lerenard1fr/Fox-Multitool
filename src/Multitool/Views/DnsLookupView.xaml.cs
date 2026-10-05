using System.Windows;
using System.Windows.Controls;
using Multitool.Services;

namespace Multitool.Views;

public partial class DnsLookupView : UserControl
{
    public DnsLookupView()
    {
        InitializeComponent();
        RefreshTexts();
        Lang.Changed += RefreshTexts;
    }

    private void RefreshTexts() => TxtSub.Text = Lang.T("v_dns_sub");

    private async void BtnNs_Click(object sender, RoutedEventArgs e) => await Dns("NS");
    private async void BtnMx_Click(object sender, RoutedEventArgs e) => await Dns("MX");
    private async void BtnTxt_Click(object sender, RoutedEventArgs e) => await Dns("TXT");
    private async void BtnA_Click(object sender, RoutedEventArgs e) => await Dns("A");
    private async void BtnAaaa_Click(object sender, RoutedEventArgs e) => await Dns("AAAA");
    private async void BtnSoa_Click(object sender, RoutedEventArgs e) => await Dns("SOA");

    private async void BtnPtr_Click(object sender, RoutedEventArgs e)
    {
        OutDns.Text = Lang.T("v_msg_ptr");
        OutDns.Text = await DnsTools.ReverseAsync(InPtr.Text);
    }

    private async Task Dns(string type)
    {
        OutDns.Text = string.Format(Lang.T("v_msg_lookup"), type);
        OutDns.Text = await DnsTools.LookupAsync(InDns.Text, type);
    }
}
