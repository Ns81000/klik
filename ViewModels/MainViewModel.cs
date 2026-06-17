using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Klik.Models;
using Klik.Services;

namespace Klik.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private const int MaxMessageLength = 250;
    private const int MinIntervalMs = 10;

    private readonly IKeyboardService _keyboard;
    private readonly IPresetStorageService _storage;

    private CancellationTokenSource? _cts;
    private readonly DispatcherTimer _elapsedTimer;
    private readonly Stopwatch _stopwatch = new();
    private AppSettings _settings;
    private DateTime _lastHotkey = DateTime.MinValue;

    public Func<PresetsViewModel?>? OpenPresetsDialog { get; set; }

    public MainViewModel(IKeyboardService keyboard, IPresetStorageService storage)
    {
        _keyboard = keyboard;
        _storage = storage;
        _settings = _storage.LoadSettings();

        SendKeyName = string.IsNullOrWhiteSpace(_settings.SendKeyName) ? "Enter" : _settings.SendKeyName;
        StopOnDuration = _settings.StopOnDuration;
        StopAfterMinutes = _settings.StopAfterMinutes > 0 ? _settings.StopAfterMinutes : 5;
        IntervalMinutes = _settings.IntervalMinutes;
        IntervalSeconds = _settings.IntervalSeconds;
        IntervalMilliseconds = _settings.IntervalMilliseconds;
        
        // Ensure default interval on first launch
        if (IntervalMinutes == 0 && IntervalSeconds == 0 && IntervalMilliseconds == 0)
        {
            IntervalSeconds = 1;
        }

        IsAlwaysOnTop = _settings.IsAlwaysOnTop;

        // Load presets
        var loaded = _storage.LoadPresets();
        foreach (var p in loaded) Presets.Add(p);
        HasPresets = Presets.Count > 0;

        _elapsedTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _elapsedTimer.Tick += (_, _) => ElapsedTime = _stopwatch.Elapsed.ToString(@"mm\:ss");
    }

    [ObservableProperty] private string _messageText = string.Empty;
    [ObservableProperty] private string _statusText = "Ready";
    [ObservableProperty] private string _statusColor = "#107C10";
    [ObservableProperty] private long _sendCount = 0;
    [ObservableProperty] private string _elapsedTime = "00:00";
    [ObservableProperty] private int _intervalMinutes = 0;
    [ObservableProperty] private int _intervalSeconds = 1;
    [ObservableProperty] private int _intervalMilliseconds = 0;
    [ObservableProperty] private bool _stopOnDuration = false;
    [ObservableProperty] private int _stopAfterMinutes = 5;
    [ObservableProperty] private bool _isRunning = false;
    [ObservableProperty] private bool _isRecordingKey = false;
    [ObservableProperty] private string _sendKeyName = "Enter";
    [ObservableProperty] private string _charCountText = "0 / 250";
    [ObservableProperty] private string _errorText = string.Empty;
    [ObservableProperty] private bool _hasError = false;
    [ObservableProperty] private string _warningText = string.Empty;
    [ObservableProperty] private bool _hasWarning = false;
    [ObservableProperty] private MessagePreset? _selectedPreset;
    [ObservableProperty] private bool _isAlwaysOnTop = false;
    [ObservableProperty] private bool _hasPresets = false;
    [ObservableProperty] private ObservableCollection<MessagePreset> _presets = new();

    public bool IsIdle => !IsRunning;

    partial void OnIsRunningChanged(bool value) => OnPropertyChanged(nameof(IsIdle));

    partial void OnMessageTextChanged(string value)
    {
        var len = value?.Length ?? 0;
        if (len > MaxMessageLength && value is not null)
        {
            MessageText = value.Substring(0, MaxMessageLength);
            return;
        }
        CharCountText = $"{len} / {MaxMessageLength}";
        if (HasError && len > 0) { HasError = false; ErrorText = string.Empty; }

        if (SelectedPreset != null && SelectedPreset.Text != value)
        {
            SelectedPreset = null;
        }
    }

    partial void OnSelectedPresetChanged(MessagePreset? value)
    {
        if (value is not null) MessageText = value.Text;
    }

    partial void OnStopOnDurationChanged(bool value) => PersistSettings();
    partial void OnStopAfterMinutesChanged(int value) => PersistSettings();
    partial void OnIntervalMinutesChanged(int value) => PersistSettings();
    partial void OnIntervalSecondsChanged(int value) => PersistSettings();
    partial void OnIntervalMillisecondsChanged(int value) => PersistSettings();
    partial void OnIsAlwaysOnTopChanged(bool value) => PersistSettings();

    private int TotalIntervalMs =>
        (IntervalMinutes * 60_000) + (IntervalSeconds * 1_000) + IntervalMilliseconds;

    [RelayCommand]
    private async Task StartAsync()
    {
        if (IsRunning) return;

        if (string.IsNullOrWhiteSpace(MessageText))
        {
            HasError = true;
            ErrorText = "Message cannot be empty";
            return;
        }

        if (TotalIntervalMs < MinIntervalMs)
        {
            MessageBox.Show(
                $"The typing interval must be at least {MinIntervalMs}ms.\n\n" +
                "A smaller value can freeze your system.",
                "Klik \u2014 Invalid Interval", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        HasError = false;
        ErrorText = string.Empty;
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        IsRunning = true;
        SendCount = 0;

        try
        {
            // COUNTDOWN
            SetStatus("#FF8C00", "Switch to target window! Starting in 3...");
            for (int i = 3; i >= 1; i--)
            {
                SetStatus("#FF8C00", $"Switch to target window! Starting in {i}...");
                await Task.Delay(1000, token);
            }

            // RUNNING
            SetStatus("#0078D4", "Running");
            _stopwatch.Restart();
            ElapsedTime = "00:00";
            _elapsedTimer.Start();

            if (StopOnDuration && StopAfterMinutes > 0)
                _cts.CancelAfter(TimeSpan.FromMinutes(StopAfterMinutes));

            var msg = MessageText;
            var vk = _settings.SendKeyVirtualCode;
            var interval = TotalIntervalMs;

            while (!token.IsCancellationRequested)
            {
                _keyboard.SendTextAndKey(msg, vk);
                SendCount++;
                await Task.Delay(interval, token);
            }
        }
        catch (OperationCanceledException)
        {
            // expected on stop / auto-timeout
        }
        finally
        {
            Stop();
        }
    }

    [RelayCommand]
    private void Stop()
    {
        if (_cts is not null && !_cts.IsCancellationRequested)
        {
            SetStatus("#FF8C00", "Stopping...");
            _cts.Cancel();
        }

        _elapsedTimer.Stop();
        _stopwatch.Stop();
        _cts?.Dispose();
        _cts = null;

        IsRunning = false;
        SetStatus("#107C10", "Ready");
    }

    public void ToggleStartStop()
    {
        var now = DateTime.UtcNow;
        if ((now - _lastHotkey).TotalMilliseconds < 500) return; // debounce
        _lastHotkey = now;

        if (IsRunning) Stop();
        else _ = StartAsync();
    }

    [RelayCommand]
    private void OpenPresets()
    {
        var result = OpenPresetsDialog?.Invoke();
        
        // Reload presets in case any were added/deleted/renamed
        var loaded = _storage.LoadPresets();
        Presets.Clear();
        foreach (var p in loaded) Presets.Add(p);
        HasPresets = Presets.Count > 0;

        if (result?.ResultPreset is { } preset)
        {
            SelectedPreset = Presets.FirstOrDefault(p => p.Id == preset.Id);
        }
    }

    [RelayCommand]
    private void BeginKeyRecording() => IsRecordingKey = true;

    [RelayCommand]
    public void CancelKeyRecording() => IsRecordingKey = false;

    public void SetSendKey(string keyName, int virtualKeyCode)
    {
        SendKeyName = keyName;
        IsRecordingKey = false;
        _settings.SendKeyName = keyName;
        _settings.SendKeyVirtualCode = virtualKeyCode;
        PersistSettings();
    }

    public void ShowHotkeyWarning()
    {
        HasWarning = true;
        WarningText = "F6 hotkey unavailable \u2014 another app is using it";
    }

    private void SetStatus(string color, string text)
    {
        StatusColor = color;
        StatusText = text;
    }

    private void PersistSettings()
    {
        _settings.StopOnDuration = StopOnDuration;
        _settings.StopAfterMinutes = StopAfterMinutes;
        _settings.IntervalMinutes = IntervalMinutes;
        _settings.IntervalSeconds = IntervalSeconds;
        _settings.IntervalMilliseconds = IntervalMilliseconds;
        _settings.IsAlwaysOnTop = IsAlwaysOnTop;
        _storage.SaveSettings(_settings);
    }
}
