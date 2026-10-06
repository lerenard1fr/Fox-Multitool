using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Multitool.Services;

namespace Multitool.Views;

public partial class InstagramView : UserControl
{
    public InstagramView()
    {
        InitializeComponent();
        RefreshTexts();
        Lang.Changed += RefreshTexts;
    }

    private void RefreshTexts()
    {
        TxtSub.Text = Lang.T("v_ig_sub");
        GrpMain.Header = Lang.T("v_ig_grp");
        BtnOpen.Content = Lang.T("v_ig_open");
        TxtNote.Text = Lang.T("v_ig_note");
    }

    private void BtnOpen_Click(object sender, RoutedEventArgs e)
    {
        string? url = LookupTools.InstagramUrl(InUser.Text);
        if (url is null)
        {
            TxtNote.Text = Lang.T("v_ig_invalid");
            return;
        }
        TxtNote.Text = Lang.T("v_ig_note");
        try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
        catch (Exception ex) { TxtNote.Text = $"{Lang.T("svc_error")}{ex.Message}"; }
    }
}
