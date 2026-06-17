using System;
using System.Threading;
using System.Windows;
using Klik.Services;
using Klik.ViewModels;
using Klik.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Klik;

public partial class App : Application
{
    private static readonly Mutex _mutex = new(true, "Klik_SingleInstance");
    private IHost? _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        if (!_mutex.WaitOne(TimeSpan.Zero, true))
        {
            MessageBox.Show("Klik is already running. Check your system tray.",
                "Klik", MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }

        base.OnStartup(e);

        Klik.Helpers.ThemeHelper.Initialize();

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<IKeyboardService, KeyboardService>();
                services.AddSingleton<IHotkeyService, HotkeyService>();
                services.AddSingleton<IPresetStorageService, PresetStorageService>();

                services.AddSingleton<MainViewModel>();
                services.AddTransient<PresetsViewModel>();

                services.AddSingleton<MainWindow>();
                services.AddTransient<PresetsWindow>();
            })
            .Build();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _host?.Dispose();
        try { _mutex.ReleaseMutex(); } catch { /* not owned */ }
        base.OnExit(e);
    }
}
