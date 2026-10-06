using System.Windows;
using System.Windows.Controls;
using Multitool.Services;

namespace Multitool.Views;

public partial class EmailView : UserControl
{
    public EmailView()
    {
        InitializeComponent();
        RefreshTexts();
        Lang.Changed += RefreshTexts;
    }

    private void RefreshTexts()
    {
        TxtSub.Text = Lang.T("v_eml_sub");
        GrpMain.Header = Lang.T("v_eml_grp");
        BtnCheck.Content = Lang.T("v_nmap_check");
    }

    private async void BtnCheck_Click(object sender, RoutedEventArgs e)
    {
        OutEmail.Text = Lang.T("v_msg_searching");
        var (formatOk, domainOk, mx, hasGravatar, gravUrl) = await LookupTools.EmailCheckAsync(InEmail.Text);
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"{Lang.T("v_eml_format")}: {(formatOk ? "OK" : Lang.T("v_eml_invalid"))}");
        sb.AppendLine($"{Lang.T("v_eml_domain")}: {(domainOk ? "OK" : Lang.T("v_eml_invalid"))}");
        sb.AppendLine($"--- MX ---\n{mx}");
        sb.AppendLine($"--- Gravatar ---\n{(hasGravatar ? gravUrl : Lang.T("v_eml_nogravatar"))}");
        OutEmail.Text = sb.ToString().Trim();
    }
}
