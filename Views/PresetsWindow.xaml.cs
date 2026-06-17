using System;
using System.Windows;
using System.Windows.Input;
using Klik.Helpers;
using Klik.ViewModels;

namespace Klik.Views;

public partial class PresetsWindow : Window
{
    public PresetsWindow(PresetsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        // Apply title bar theme based on current system theme
        ThemeHelper.UpdateTitleBarTheme(this, !ThemeHelper.IsSystemLightTheme());
    }

    private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            element.Focus();
        }
    }
}
