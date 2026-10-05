using System.Windows;
using System.Windows.Controls;
using Multitool.Services;

namespace Multitool.Views;

public partial class SystemView : UserControl
{
    public SystemView()
    {
        InitializeComponent();
        RefreshTexts();
        Lang.Changed += RefreshTexts;
    }

    private void RefreshTexts()
    {
        TxtTitle.Text = Lang.T("v_sys_title");
        GrpInfo.Header = Lang.T("v_sys_info");
        BtnInfo.Content = Lang.T("v_sys_show");
        GrpProc.Header = Lang.T("v_sys_proc");
        BtnProc.Content = Lang.T("v_sys_list");
        GrpClean.Header = Lang.T("v_sys_clean_grp");
        BtnClean.Content = Lang.T("v_sys_clean");
    }

    private void BtnInfo_Click(object sender, RoutedEventArgs e) => OutInfo.Text = SystemTools.SystemInfo();
    private void BtnProc_Click(object sender, RoutedEventArgs e) => OutProc.Text = SystemTools.TopProcesses();
    private void BtnClean_Click(object sender, RoutedEventArgs e) => OutClean.Text = SystemTools.CleanTemp();
}
