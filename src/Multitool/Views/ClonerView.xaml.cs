using System.Windows;
using System.Windows.Controls;
using Multitool.Services;

namespace Multitool.Views;

public partial class ClonerView : UserControl
{
    public ClonerView()
    {
        InitializeComponent();
        RefreshTexts();
        Lang.Changed += RefreshTexts;
    }

    private void RefreshTexts()
    {
        TxtSub.Text = Lang.T("v_cln_sub");
        GrpMain.Header = Lang.T("v_cln_grp");
        BtnClone.Content = Lang.T("v_cln_clone");
    }

    private async void BtnClone_Click(object sender, RoutedEventArgs e)
    {
        OutClone.Text = Lang.T("v_msg_scanning");
        var (folder, files) = await LookupTools.CloneAsync(InUrl.Text);
        OutClone.Text = string.IsNullOrEmpty(folder)
            ? Lang.T("v_cln_failed")
            : $"{Lang.T("v_cln_saved")}{folder}\n{string.Format(Lang.T("v_cln_files"), files)}";
    }
}
