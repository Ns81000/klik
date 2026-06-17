using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Win32;

namespace Klik.Helpers;

public static class ThemeHelper
{
    private static ResourceDictionary? _currentThemeDictionary;

    [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
    private static extern void DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int pvAttribute, int cbAttribute);

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    public static void Initialize()
    {
        ApplyTheme();
        SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
    }

    private static void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category == UserPreferenceCategory.General)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(ApplyTheme));
        }
    }

    public static void ApplyTheme()
    {
        bool isLight = IsSystemLightTheme();
        
#pragma warning disable WPF0001
        Application.Current.ThemeMode = isLight ? ThemeMode.Light : ThemeMode.Dark;
#pragma warning restore WPF0001

        var themeUri = new Uri(isLight 
            ? "pack://application:,,,/Themes/LightTheme.xaml" 
            : "pack://application:,,,/Themes/DarkTheme.xaml", UriKind.Absolute);
        
        var newDict = new ResourceDictionary { Source = themeUri };

        var resources = Application.Current.Resources;
        if (_currentThemeDictionary != null)
        {
            resources.MergedDictionaries.Remove(_currentThemeDictionary);
        }
        
        resources.MergedDictionaries.Add(newDict);
        _currentThemeDictionary = newDict;

        // Apply title bar dark/light mode for all open windows
        foreach (Window window in Application.Current.Windows)
        {
            UpdateTitleBarTheme(window, !isLight);
        }
    }

    public static void UpdateTitleBarTheme(Window window, bool isDark)
    {
        try
        {
            IntPtr hWnd = new WindowInteropHelper(window).Handle;
            if (hWnd != IntPtr.Zero)
            {
                int darkMode = isDark ? 1 : 0;
                DwmSetWindowAttribute(hWnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));
            }
        }
        catch
        {
            // Ignore if DWM is not supported or not available
        }
    }

    public static bool IsSystemLightTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var value = key?.GetValue("AppsUseLightTheme");
            if (value is int i)
            {
                return i > 0;
            }
        }
        catch
        {
            // Fallback to dark if registry fails or isn't available
        }
        return false;
    }
}
