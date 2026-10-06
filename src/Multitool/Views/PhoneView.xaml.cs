using System.Windows;
using System.Windows.Controls;
using Multitool.Services;

namespace Multitool.Views;

public partial class PhoneView : UserControl
{
    public PhoneView()
    {
        InitializeComponent();
        RefreshTexts();
        Lang.Changed += RefreshTexts;
    }

    private void RefreshTexts()
    {
        TxtSub.Text = Lang.T("v_phn_sub");
        GrpMain.Header = Lang.T("v_phn_grp");
        BtnParse.Content = Lang.T("v_nmap_check");
    }

    private void BtnParse_Click(object sender, RoutedEventArgs e)
    {
        var r = PhoneTools.Parse(InPhone.Text, string.IsNullOrWhiteSpace(InRegion.Text) ? "FR" : InRegion.Text.Trim().ToUpperInvariant());
        if (!r.Ok)
        {
            OutPhone.Text = Lang.T("v_phn_invalid");
            return;
        }
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"{Lang.T("v_phn_valid")}: {(r.Valid ? "OK" : Lang.T("v_phn_invalid"))}");
        sb.AppendLine($"{Lang.T("v_phn_region")}: {r.Region}");
        sb.AppendLine($"{Lang.T("v_phn_type")}: {r.Type}");
        sb.AppendLine($"{Lang.T("v_phn_carrier")}: {(string.IsNullOrWhiteSpace(r.Carrier) ? "-" : r.Carrier)}");
        sb.AppendLine($"{Lang.T("v_phn_geo")}: {(string.IsNullOrWhiteSpace(r.Geo) ? "-" : r.Geo)}");
        sb.AppendLine($"International: {r.Intl}");
        sb.AppendLine($"E164: {r.E164}");
        OutPhone.Text = sb.ToString().Trim();
    }
}
