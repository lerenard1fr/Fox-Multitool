using System.Windows;
using System.Windows.Controls;
using Multitool.Services;

namespace Multitool.Views;

public partial class GeneratorView : UserControl
{
    public GeneratorView() => InitializeComponent();

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
