using System;

namespace Klik.Services;

public interface IHotkeyService
{
    bool Register(IntPtr windowHandle, int id, uint modifiers, uint virtualKey);
    void Unregister(IntPtr windowHandle, int id);
    event EventHandler<int> HotkeyPressed; // fires with hotkey id
    void ProcessHotkeyMessage(int id);
}
