using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Klik.Models;

namespace Klik.Services;

public class PresetStorageService : IPresetStorageService
{
    private static readonly string Dir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Klik");

    private static readonly string PresetsPath = Path.Combine(Dir, "presets.json");
    private static readonly string SettingsPath = Path.Combine(Dir, "settings.json");

    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    private static void EnsureDir()
    {
        if (!Directory.Exists(Dir)) Directory.CreateDirectory(Dir);
    }

    public List<MessagePreset> LoadPresets()
    {
        try
        {
            if (!File.Exists(PresetsPath)) return new List<MessagePreset>();
            var json = File.ReadAllText(PresetsPath);
            return JsonSerializer.Deserialize<List<MessagePreset>>(json) ?? new List<MessagePreset>();
        }
        catch (JsonException ex)
        {
            Debug.WriteLine($"[Klik] Corrupted presets.json, resetting. {ex.Message}");
            return new List<MessagePreset>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Klik] Failed to load presets. {ex.Message}");
            return new List<MessagePreset>();
        }
    }

    public void SavePresets(List<MessagePreset> presets)
    {
        try
        {
            EnsureDir();
            File.WriteAllText(PresetsPath, JsonSerializer.Serialize(presets, Options));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Klik] Failed to save presets. {ex.Message}");
        }
    }

    public AppSettings LoadSettings()
    {
        try
        {
            if (!File.Exists(SettingsPath)) return new AppSettings();
            var json = File.ReadAllText(SettingsPath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch (JsonException ex)
        {
            Debug.WriteLine($"[Klik] Corrupted settings.json, resetting. {ex.Message}");
            return new AppSettings();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Klik] Failed to load settings. {ex.Message}");
            return new AppSettings();
        }
    }

    public void SaveSettings(AppSettings settings)
    {
        try
        {
            EnsureDir();
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(settings, Options));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Klik] Failed to save settings. {ex.Message}");
        }
    }
}
