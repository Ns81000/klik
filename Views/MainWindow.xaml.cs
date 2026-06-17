using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Klik.Helpers;
using Klik.Services;
using Klik.ViewModels;

namespace Klik.Views;

public partial class MainWindow : Window
{
    private const int HotkeyId = 9000;
    private const int WmHotkey = 0x0312;
    private const uint VkF6 = 0x75;

    private readonly MainViewModel _viewModel;
    private readonly IHotkeyService _hotkey;
    private readonly IServiceProvider _services;
    private bool _isRecordingKeyCaptured;

    public MainWindow(MainViewModel viewModel, IHotkeyService hotkey, IServiceProvider services)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _hotkey = hotkey;
        _services = services;
        DataContext = _viewModel;

        _viewModel.OpenPresetsDialog = OpenPresetsDialog;

        _hotkey.HotkeyPressed += OnHotkeyPressed;

        SourceInitialized += OnSourceInitialized;
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        var helper = new WindowInteropHelper(this);
        var source = HwndSource.FromHwnd(helper.Handle);
        source?.AddHook(WndProc);

        var ok = _hotkey.Register(helper.Handle, HotkeyId, 0, VkF6);
        if (!ok) _viewModel.ShowHotkeyWarning();

        // Apply title bar theme based on current system theme
        ThemeHelper.UpdateTitleBarTheme(this, !ThemeHelper.IsSystemLightTheme());
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WmHotkey && wParam.ToInt32() == HotkeyId)
        {
            _hotkey.ProcessHotkeyMessage(HotkeyId);
            handled = true;
        }
        return IntPtr.Zero;
    }

    private void OnHotkeyPressed(object? sender, int id)
    {
        if (id == HotkeyId) Dispatcher.Invoke(_viewModel.ToggleStartStop);
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (!_viewModel.IsRecordingKey) { base.OnPreviewKeyDown(e); return; }

        e.Handled = true;

        // Resolve System keys (like Alt)
        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        if (key == Key.Escape)
        {
            _viewModel.CancelKeyRecording();
            return;
        }

        if (key == Key.None)
        {
            return;
        }

        _isRecordingKeyCaptured = true;
        var vk = KeyInteropHelper.ToVirtualKeyCode(key);
        
        string keyName = GetFriendlyKeyName(key);
        _viewModel.SetSendKey(keyName, vk);
    }

    private string GetFriendlyKeyName(Key key)
    {
        return key switch
        {
            Key.Return => "Enter",
            Key.LeftShift => "Left Shift",
            Key.RightShift => "Right Shift",
            Key.LeftCtrl => "Left Ctrl",
            Key.RightCtrl => "Right Ctrl",
            Key.LeftAlt => "Left Alt",
            Key.RightAlt => "Right Alt",
            Key.LWin => "Left Windows",
            Key.RWin => "Right Windows",
            Key.Capital => "Caps Lock",
            Key.Back => "Backspace",
            Key.PageUp => "Page Up",
            Key.PageDown => "Page Down",
            Key.NumPad0 => "Num 0",
            Key.NumPad1 => "Num 1",
            Key.NumPad2 => "Num 2",
            Key.NumPad3 => "Num 3",
            Key.NumPad4 => "Num 4",
            Key.NumPad5 => "Num 5",
            Key.NumPad6 => "Num 6",
            Key.NumPad7 => "Num 7",
            Key.NumPad8 => "Num 8",
            Key.NumPad9 => "Num 9",
            Key.Multiply => "Num *",
            Key.Add => "Num +",
            Key.Separator => "Num Separator",
            Key.Subtract => "Num -",
            Key.Decimal => "Num .",
            Key.Divide => "Num /",
            _ => key.ToString()
        };
    }

    protected override void OnPreviewKeyUp(KeyEventArgs e)
    {
        if (_isRecordingKeyCaptured)
        {
            e.Handled = true;
            _isRecordingKeyCaptured = false;
            return;
        }
        base.OnPreviewKeyUp(e);
    }

    private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            element.Focus();
        }
    }

    private PresetsViewModel? OpenPresetsDialog()
    {
        var vm = (PresetsViewModel)_services.GetService(typeof(PresetsViewModel))!;
        vm.CurrentMessageText = _viewModel.MessageText;
        var window = new PresetsWindow(vm) { Owner = this };
        window.ShowDialog();
        return vm;
    }

    private void NumericOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
    }

    private void NumericOnly_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(typeof(string)))
        {
            var text = (string)e.DataObject.GetData(typeof(string));
            if (!Regex.IsMatch(text, "^[0-9]+$"))
            {
                e.CancelCommand();
            }
        }
        else
        {
            e.CancelCommand();
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        var helper = new WindowInteropHelper(this);
        _hotkey.Unregister(helper.Handle, HotkeyId);
        base.OnClosed(e);
        Application.Current.Shutdown();
    }
}
