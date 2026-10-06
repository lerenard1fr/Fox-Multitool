using System.IO;
using System.Windows;
using System.Windows.Controls;
using Multitool.Services;

namespace Multitool.Views;

public partial class CrawlerView : UserControl
{
    public CrawlerView()
    {
        InitializeComponent();
        RefreshTexts();
        Lang.Changed += RefreshTexts;
    }

    private void RefreshTexts()
    {
        TxtSub.Text = Lang.T("v_crw_sub");
        GrpMain.Header = Lang.T("v_crw_grp");
        ChkSame.Content = Lang.T("v_crw_same");
        BtnCrawl.Content = Lang.T("v_crw_crawl");
        BtnExport.Content = Lang.T("v_com_export");
    }

    private async void BtnCrawl_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(InDepth.Text, out int depth) || depth is < 0 or > 3) depth = 1;
        OutUrls.Text = Lang.T("v_msg_scanning");
        var urls = await LookupTools.CrawlAsync(InUrl.Text, depth, ChkSame.IsChecked == true);
        OutUrls.Text = urls.Count == 0 ? Lang.T("v_crw_none") : string.Join("\n", urls);
        TxtExport.Text = string.Format(Lang.T("v_crw_found"), urls.Count);
    }

    private void BtnExport_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(OutUrls.Text))
            {
                TxtExport.Text = Lang.T("v_nmap_export_empty");
                return;
            }
            Directory.CreateDirectory(AppPaths.DataDir);
            string file = Path.Combine(AppPaths.DataDir, $"crawl-{DateTime.Now:yyyyMMdd-HHmmss}.txt");
            File.WriteAllText(file, $"FoxMultitool - crawl {InUrl.Text} - {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n\n{OutUrls.Text}");
            TxtExport.Text = $"{Lang.T("v_nmap_exported")}{file}";
        }
        catch (Exception ex) { TxtExport.Text = $"{Lang.T("svc_error")}{ex.Message}"; }
    }
}
