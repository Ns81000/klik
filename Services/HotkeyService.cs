using System;
using System.Runtime.InteropServices;

namespace Klik.Services;

public class HotkeyService : IHotkeyService
{
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    public event EventHandler<int>? HotkeyPressed;

    public bool Register(IntPtr windowHandle, int id, uint modifiers, uint virtualKey)
        => RegisterHotKey(windowHandle, id, modifiers, virtualKey);

    public void Unregister(IntPtr windowHandle, int id)
        => UnregisterHotKey(windowHandle, id);

    public void ProcessHotkeyMessage(int id) => HotkeyPressed?.Invoke(this, id);
}
