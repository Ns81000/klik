using System.Windows.Input;

namespace Klik.Helpers;

/// <summary>
/// Converts a WPF <see cref="Key"/> into a Win32 virtual key code suitable for
/// H.InputSimulator. Uses the built-in WPF KeyInterop, which already maps to the
/// same virtual-key codes that SendInput expects.
/// </summary>
public static class KeyInteropHelper
{
    public static int ToVirtualKeyCode(Key key) => KeyInterop.VirtualKeyFromKey(key);
}
