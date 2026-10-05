using System.Windows;
using System.Windows.Controls;
using Multitool.Services;

namespace Multitool.Views;

public partial class GeneratorView : UserControl
{
    public GeneratorView()
    {
        InitializeComponent();
        RefreshTexts();
        Lang.Changed += RefreshTexts;
    }

    private void RefreshTexts()
    {
        TxtTitle.Text = Lang.T("v_gen_title");
        GrpPass.Header = Lang.T("v_gen_pass");
        ChkUpper.Content = Lang.T("v_gen_upper");
        ChkDigits.Content = Lang.T("v_gen_digits");
        ChkSym.Content = Lang.T("v_gen_symbols");
        BtnPass.Content = Lang.T("v_gen_generate");
        BtnGuid.Content = Lang.T("v_gen_new_guid");
        BtnEnc.Content = Lang.T("v_gen_encode");
        BtnDec.Content = Lang.T("v_gen_decode");
    }

    private void BtnPass_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(InLen.Text, out int len) || len is < 4 or > 128) len = 20;
        OutPass.Text = GeneratorTools.NewPassword(len, ChkUpper.IsChecked == true, ChkDigits.IsChecked == true, ChkSym.IsChecked == true);
    }

    private void BtnGuid_Click(object sender, RoutedEventArgs e) => OutGuid.Text = Guid.NewGuid().ToString();
    private void BtnEnc_Click(object sender, RoutedEventArgs e) => OutB64.Text = GeneratorTools.ToBase64(InB64.Text);
    private void BtnDec_Click(object sender, RoutedEventArgs e) => OutB64.Text = GeneratorTools.FromBase64(InB64.Text);
    private void BtnSha_Click(object sender, RoutedEventArgs e) => OutHash.Text = GeneratorTools.Sha256(InHash.Text);
    private void BtnMd5_Click(object sender, RoutedEventArgs e) => OutHash.Text = GeneratorTools.Md5(InHash.Text);
}
