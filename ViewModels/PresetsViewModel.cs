using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Klik.Models;
using Klik.Services;

namespace Klik.ViewModels;

public partial class PresetsViewModel : ObservableObject
{
    private readonly IPresetStorageService _storage;

    [ObservableProperty] private ObservableCollection<MessagePreset> _presets = new();
    [ObservableProperty] private MessagePreset? _selectedPreset;
    [ObservableProperty] private string _editingTitle = string.Empty;
    [ObservableProperty] private string _editingText = string.Empty;

    /// <summary>Text from the main window message box, used initially if creating a new preset.</summary>
    public string CurrentMessageText { get; set; } = string.Empty;

    /// <summary>Set on selection/save so the caller can auto-select it when manager closes.</summary>
    [ObservableProperty] private MessagePreset? _resultPreset;

    public PresetsViewModel(IPresetStorageService storage)
    {
        _storage = storage;
        foreach (var p in _storage.LoadPresets()) Presets.Add(p);
        Presets.CollectionChanged += (s, e) => OnPropertyChanged(nameof(HasNoPresets));
    }

    public bool HasNoPresets => Presets.Count == 0;
    public bool IsPresetSelected => SelectedPreset is not null;

    partial void OnSelectedPresetChanged(MessagePreset? value)
    {
        if (value is not null)
        {
            EditingTitle = value.Name;
            EditingText = value.Text;
            ResultPreset = value;
        }
        else
        {
            EditingTitle = string.Empty;
            EditingText = string.Empty;
        }
        OnPropertyChanged(nameof(IsPresetSelected));
    }

    [RelayCommand]
    private void NewPreset()
    {
        SelectedPreset = null;
        EditingTitle = string.Empty;
        EditingText = string.Empty;
    }

    [RelayCommand]
    private void SavePreset()
    {
        var title = EditingTitle?.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            MessageBox.Show("Please enter a preset title.", "Klik \u2014 Validation Error", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (SelectedPreset is not null)
        {
            // Update existing preset (properties are observable, list will auto-refresh)
            SelectedPreset.Name = title;
            SelectedPreset.Text = EditingText ?? string.Empty;
            ResultPreset = SelectedPreset;
        }
        else
        {
            // Create a new preset
            var preset = new MessagePreset { Name = title, Text = EditingText ?? string.Empty };
            Presets.Add(preset);
            ResultPreset = preset;
        }

        Save();

        SelectedPreset = null;
        EditingTitle = string.Empty;
        EditingText = string.Empty;
    }

    [RelayCommand]
    private void DeletePreset()
    {
        if (SelectedPreset is null) return;
        
        var result = MessageBox.Show(
            $"Delete '{SelectedPreset.Name}'? This cannot be undone.",
            "Klik \u2014 Delete Preset", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes) return;

        var toRemove = SelectedPreset;
        SelectedPreset = null;
        Presets.Remove(toRemove);
        
        if (ResultPreset == toRemove)
        {
            ResultPreset = null;
        }

        Save();
    }

    private void Save() => _storage.SavePresets(Presets.ToList());
}
