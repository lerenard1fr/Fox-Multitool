using System.Windows;
using System.Windows.Controls;
using Multitool.Views;

namespace Multitool;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        MainContent.Content = new NetworkView();
        Highlight(BtnNetwork);
    }

    private void Nav_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        Highlight(btn);

        MainContent.Content = btn.Name switch
        {
            nameof(BtnNetwork) => new NetworkView(),
            nameof(BtnNmap) => new NmapView(),
            nameof(BtnSystem) => new SystemView(),
            nameof(BtnOsint) => new OsintView(),
            nameof(BtnGenerator) => new GeneratorView(),
            _ => MainContent.Content
        };
    }

    private void Highlight(Button active)
    {
        foreach (var b in new[] { BtnNetwork, BtnNmap, BtnSystem, BtnOsint, BtnGenerator })
            b.Background = System.Windows.Media.Brushes.Transparent;
        active.Background = new System.Windows.Media.SolidColorBrush(
            System.Windows.Media.Color.FromRgb(0x21, 0x26, 0x2D));
    }
}
