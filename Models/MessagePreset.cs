using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Klik.Models;

public partial class MessagePreset : ObservableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _text = string.Empty;
}
