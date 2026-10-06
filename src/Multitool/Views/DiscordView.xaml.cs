using System.Windows;
using System.Windows.Controls;
using Multitool.Services;

namespace Multitool.Views;

public partial class DiscordView : UserControl
{
    public DiscordView()
    {
        InitializeComponent();
        RefreshTexts();
        Lang.Changed += RefreshTexts;
    }

    private void RefreshTexts()
    {
        TxtWarn.Text = Lang.T("v_dis_warn");
        GrpSend.Header = Lang.T("v_dis_send_grp");
        BtnSend.Content = Lang.T("v_dis_send");
        GrpLookup.Header = Lang.T("v_dis_lookup_grp");
        BtnId.Content = Lang.T("v_dis_decode");
        BtnInvite.Content = Lang.T("v_dis_check");
        BtnHookInfo.Content = Lang.T("v_dis_hookinfo");
    }

    private async void BtnSend_Click(object sender, RoutedEventArgs e)
    {
        OutSend.Text = Lang.T("v_sec_working");
        OutSend.Text = await DiscordTools.SendWebhookAsync(InHook.Text, InMsg.Text, InUser.Text);
    }

    private void BtnId_Click(object sender, RoutedEventArgs e)
        => OutLookup.Text = DiscordTools.SnowflakeInfo(InId.Text);

    private async void BtnInvite_Click(object sender, RoutedEventArgs e)
    {
        OutLookup.Text = Lang.T("v_sec_working");
        OutLookup.Text = await DiscordTools.InviteInfoAsync(InInvite.Text);
    }

    private async void BtnHookInfo_Click(object sender, RoutedEventArgs e)
    {
        OutLookup.Text = Lang.T("v_sec_working");
        OutLookup.Text = await DiscordTools.WebhookInfoAsync(InHook.Text);
    }
}
