using System.IO;
using System.Windows;
using System.Windows.Controls;
using Multitool.Services;

namespace Multitool.Views;

public partial class MetadataView : UserControl
{
    public MetadataView()
    {
        InitializeComponent();
        RefreshTexts();
        Lang.Changed += RefreshTexts;
    }

    private void RefreshTexts()
    {
        TxtSub.Text = Lang.T("v_mtd_sub");
        GrpMain.Header = Lang.T("v_mtd_grp");
        BtnScan.Content = Lang.T("v_mtd_scan");
        BtnClean.Content = Lang.T("v_mtd_clean");
    }

    private void BtnPick_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new Microsoft.Win32.OpenFileDialog
        {
            Filter = $"{Lang.T("v_mtd_filter")}|*.jpg;*.jpeg;*.png;*.tif;*.tiff;*.webp;*.mp4;*.mp3;*.wav|*.*|*.*",
        };
        if (dlg.ShowDialog() == true) InFile.Text = dlg.FileName;
    }

    private void BtnScan_Click(object sender, RoutedEventArgs e)
    {
        if (!File.Exists(InFile.Text))
        {
            OutMeta.Text = Lang.T("v_mtd_nofile");
            return;
        }
        var (tags, text) = MetaTools.Scan(InFile.Text);
        OutMeta.Text = text;
        TxtClean.Text = string.Format(Lang.T("v_mtd_tags"), tags);
    }

    private void BtnClean_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!File.Exists(InFile.Text))
            {
                TxtClean.Text = Lang.T("v_mtd_nofile");
                return;
            }
            string dest = MetaTools.Clean(InFile.Text);
            var (tags, _) = MetaTools.Scan(dest);
            TxtClean.Text = $"{Lang.T("v_mtd_saved")}{dest} ({string.Format(Lang.T("v_mtd_tags"), tags)})";
            Logger.Info($"Metadata cleaned: {InFile.Text} -> {dest}");
        }
        catch (Exception ex) { TxtClean.Text = $"{Lang.T("svc_error")}{ex.Message}"; }
    }
}
