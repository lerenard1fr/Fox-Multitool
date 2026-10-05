using System.Windows;
using System.Windows.Controls;
using Multitool.Services;

namespace Multitool.Views;

public partial class OsintView : UserControl
{
    public OsintView()
    {
        InitializeComponent();
        RefreshTexts();
        Lang.Changed += RefreshTexts;
    }

    private void RefreshTexts() => BtnIp.Content = Lang.T("v_osi_search");

    private async void BtnIp_Click(object sender, RoutedEventArgs e)
    {
        OutIp.Text = Lang.T("v_msg_searching");
        OutIp.Text = await OsintTools.IpLookupAsync(InIp.Text);
    }
}
