using System.Windows;
using System.Windows.Controls;
using Multitool.Services;

namespace Multitool.Views;

public partial class WalletView : UserControl
{
    public WalletView()
    {
        InitializeComponent();
        RefreshTexts();
        Lang.Changed += RefreshTexts;
    }

    private void RefreshTexts()
    {
        TxtSub.Text = Lang.T("v_wlt_sub");
        GrpMain.Header = Lang.T("v_wlt_grp");
        BtnCheck.Content = Lang.T("v_nmap_check");
    }

    private async void BtnCheck_Click(object sender, RoutedEventArgs e)
    {
        OutWallet.Text = Lang.T("v_msg_searching");
        var r = await LookupTools.WalletBtcAsync(InAddr.Text);
        OutWallet.Text = r.Ok
            ? $"{Lang.T("v_wlt_balance")}: {r.Btc} BTC\n{Lang.T("v_wlt_txs")}: {r.TxCount}"
            : Lang.T("v_wlt_invalid");
    }
}
