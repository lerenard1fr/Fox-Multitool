using System.Windows;
using System.Windows.Controls;
using Multitool.Services;

namespace Multitool.Views;

public partial class SystemView : UserControl
{
    public SystemView() => InitializeComponent();
    private void BtnInfo_Click(object sender, RoutedEventArgs e) => OutInfo.Text = SystemTools.SystemInfo();
    private void BtnProc_Click(object sender, RoutedEventArgs e) => OutProc.Text = SystemTools.TopProcesses();
    private void BtnClean_Click(object sender, RoutedEventArgs e) => OutClean.Text = SystemTools.CleanTemp();
}
