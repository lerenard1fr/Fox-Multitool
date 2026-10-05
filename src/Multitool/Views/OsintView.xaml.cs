using System.Windows;
using System.Windows.Controls;
using Multitool.Services;

namespace Multitool.Views;

public partial class OsintView : UserControl
{
    public OsintView() => InitializeComponent();
    private async void BtnIp_Click(object sender, RoutedEventArgs e)
    {
        OutIp.Text = "Recherche...";
        OutIp.Text = await OsintTools.IpLookupAsync(InIp.Text);
    }
}
