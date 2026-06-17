using System.Collections.Generic;
using Klik.Models;

namespace Klik.Services;

public interface IPresetStorageService
{
    List<MessagePreset> LoadPresets();
    void SavePresets(List<MessagePreset> presets);
    AppSettings LoadSettings();
    void SaveSettings(AppSettings settings);
}
