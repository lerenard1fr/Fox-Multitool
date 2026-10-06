using System.IO;
using System.Windows;
using System.Windows.Controls;
using Multitool.Services;

namespace Multitool.Views;

public partial class HostDiscoveryView : UserControl
{
    public HostDiscoveryView()
    {
        InitializeComponent();
        RefreshTexts();
        Lang.Changed += RefreshTexts;
    }

    private void RefreshTexts()
    {
        TxtSub.Text = Lang.T("v_hst_sub");
        GrpMain.Header = Lang.T("v_hst_grp");
        BtnDiscover.Content = Lang.T("v_hst_discover");
        BtnExport.Content = Lang.T("v_com_export");
    }

    private async void BtnDiscover_Click(object sender, RoutedEventArgs e)
    {
        OutHosts.Text = Lang.T("v_msg_scanning");
        var hosts = await LookupTools.HostDiscoveryAsync(InCidr.Text);
        OutHosts.Text = hosts.Count == 0
            ? Lang.T("v_hst_none")
            : string.Join("\n", hosts.Select(h => string.IsNullOrWhiteSpace(h.Name) ? h.Ip : $"{h.Ip}  {h.Name}"));
        TxtExport.Text = string.Format(Lang.T("v_hst_found"), hosts.Count);
    }

    private void BtnExport_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(OutHosts.Text))
            {
                TxtExport.Text = Lang.T("v_nmap_export_empty");
                return;
            }
            Directory.CreateDirectory(AppPaths.DataDir);
            string file = Path.Combine(AppPaths.DataDir, $"hosts-{DateTime.Now:yyyyMMdd-HHmmss}.txt");
            File.WriteAllText(file, $"FoxMultitool - host discovery {InCidr.Text} - {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n\n{OutHosts.Text}");
            TxtExport.Text = $"{Lang.T("v_nmap_exported")}{file}";
        }
        catch (Exception ex) { TxtExport.Text = $"{Lang.T("svc_error")}{ex.Message}"; }
    }
}
