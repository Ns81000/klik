using System;
using WindowsInput;

namespace Klik.Services;

/// <summary>
/// Wraps H.InputSimulator. Uses SendInput Unicode injection so special characters,
/// emoji and international text are sent verbatim with no escaping required.
/// </summary>
public class KeyboardService : IKeyboardService
{
    private readonly InputSimulator _sim = new();

    public void SendTextAndKey(string text, int virtualKeyCode)
    {
        if (!string.IsNullOrEmpty(text))
        {
            _sim.Keyboard.TextEntry(text);
        }

        // Fall back to Enter if the configured code is invalid (corrupted settings).
        var vk = virtualKeyCode <= 0 ? (VirtualKeyCode)0x0D : (VirtualKeyCode)virtualKeyCode;
        _sim.Keyboard.KeyPress(vk);
    }
}
