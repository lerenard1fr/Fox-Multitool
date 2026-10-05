using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Multitool.Services;

namespace Multitool.Views;

public partial class SecurityView : UserControl
{
    private readonly Dictionary<string, (Ellipse Dot, TextBlock Status, Button Install, Button Launch)> _rows = new();

    public SecurityView() : this(null) { }

    public SecurityView(string? initialFilter)
    {
        InitializeComponent();
        InSearch.Text = string.IsNullOrWhiteSpace(initialFilter) ? Lang.T("v_sec_search") : initialFilter;
        RefreshTexts();
        Lang.Changed += () => { RefreshTexts(); BuildCards(NormalizedFilter()); RefreshRows(); };
        BuildCards(NormalizedFilter());
        Loaded += async (_, _) => await DetectAsync();
    }

    private string NormalizedFilter()
        => Lang.IsPlaceholder(InSearch.Text, "v_sec_search") ? "" : InSearch.Text;

    private void RefreshTexts()
    {
        TxtTitle.Text = Lang.T("v_sec_title");
        TxtSub.Text = Lang.T("v_sec_sub");
        BtnDetect.Content = Lang.T("v_sec_detect");
        if (Lang.IsPlaceholder(InSearch.Text, "v_sec_search") || string.IsNullOrEmpty(InSearch.Text))
            InSearch.Text = Lang.T("v_sec_search");
    }

    private void BuildCards(string filter)
    {
        CardsHost.Children.Clear();
        _rows.Clear();
        string q = filter.Trim().ToLowerInvariant();
        foreach (var t in SecurityTools.Catalog)
        {
            if (q.Length > 0
                && !t.Name.ToLowerInvariant().Contains(q)
                && !t.Description.ToLowerInvariant().Contains(q)
                && !t.Category.ToLowerInvariant().Contains(q))
                continue;

            var card = new Border
            {
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 10, 12, 10),
                Margin = new Thickness(0, 0, 0, 10),
                Background = (Brush)FindResource("BgCard"),
                BorderBrush = (Brush)FindResource("BorderCol"),
                BorderThickness = new Thickness(1),
            };
            var stack = new StackPanel();
            var head = new StackPanel { Orientation = Orientation.Horizontal };
            var dot = new Ellipse { Width = 12, Height = 12, Fill = Brushes.Gray, Margin = new Thickness(0, 0, 8, 0) };
            head.Children.Add(dot);
            head.Children.Add(new TextBlock { Text = t.Name, FontWeight = FontWeights.Bold, FontSize = 14 });
            head.Children.Add(new TextBlock
            {
                Text = "  [" + t.Category + "]",
                FontSize = 11,
                Foreground = (Brush)FindResource("FgMuted"),
                VerticalAlignment = VerticalAlignment.Center,
            });
            stack.Children.Add(head);
            stack.Children.Add(new TextBlock
            {
                Text = t.Description, TextWrapping = TextWrapping.Wrap,
                Foreground = (Brush)FindResource("FgSub"), Margin = new Thickness(20, 2, 0, 0),
            });
            if (!string.IsNullOrWhiteSpace(t.Notes))
                stack.Children.Add(new TextBlock
                {
                    Text = "ⓘ " + t.Notes, TextWrapping = TextWrapping.Wrap,
                    Foreground = (Brush)FindResource("FgMuted"),
                    FontSize = 11, Margin = new Thickness(20, 2, 0, 0),
                });
            var status = new TextBlock
            {
                Text = Lang.T("v_sec_nocheck"), FontSize = 11,
                Foreground = (Brush)FindResource("FgMuted"), Margin = new Thickness(20, 4, 0, 0),
            };
            stack.Children.Add(status);

            var buttons = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(20, 8, 0, 0) };
            var btnInstall = new Button
            {
                Content = t.InstallKind == SecInstallKind.Winget ? Lang.T("v_sec_install_w")
                    : t.InstallKind == SecInstallKind.Page ? Lang.T("v_sec_page") : t.InstallRef,
                Style = (Style)FindResource("ActionButton"),
                Margin = new Thickness(0, 0, 8, 0), Tag = t,
            };
            btnInstall.Click += BtnInstall_Click;
            var btnLaunch = new Button
            {
                Content = Lang.T("v_sec_launch"), Style = (Style)FindResource("SecondaryButton"), Tag = t,
            };
            btnLaunch.Click += BtnLaunch_Click;
            buttons.Children.Add(btnInstall);
            buttons.Children.Add(btnLaunch);
            stack.Children.Add(buttons);

            card.Child = stack;
            CardsHost.Children.Add(card);
            _rows[t.Id] = (dot, status, btnInstall, btnLaunch);
        }
        RefreshRows();
    }

    private void RefreshRows()
    {
        foreach (var t in SecurityTools.Catalog)
        {
            if (!_rows.TryGetValue(t.Id, out var r)) continue;
            r.Dot.Fill = t.Installed ? Brushes.LimeGreen : Brushes.Red;
            r.Status.Text = t.Installed ? $"{Lang.T("v_sec_found")}{t.FoundPath}" : Lang.T("v_sec_notfound");
            r.Status.Foreground = (Brush)FindResource(t.Installed ? "GreenFg" : "RedFg");
            r.Launch.IsEnabled = t.Installed && t.LaunchKind != SecLaunchKind.None;
            r.Install.IsEnabled = !t.Installed && t.InstallKind != SecInstallKind.Bundled;
        }
    }

    private async Task DetectAsync()
    {
        await Task.Run(() => SecurityTools.DetectAll());
        RefreshRows();
    }

    private async void BtnDetect_Click(object sender, RoutedEventArgs e) => await DetectAsync();

    private void InSearch_Changed(object sender, TextChangedEventArgs e) => BuildCards(NormalizedFilter());

    private async void BtnInstall_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is not SecTool t) return;
        if (!_rows.TryGetValue(t.Id, out var r)) return;
        r.Install.IsEnabled = false;
        r.Status.Text = Lang.T("v_sec_working");
        r.Status.Text = await SecurityTools.InstallAsync(t);
        await DetectAsync();
        r.Install.IsEnabled = !t.Installed && t.InstallKind != SecInstallKind.Bundled;
    }

    private void BtnLaunch_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is not SecTool t) return;
        if (!_rows.TryGetValue(t.Id, out var r)) return;
        r.Status.Text = SecurityTools.Launch(t);
    }
}
