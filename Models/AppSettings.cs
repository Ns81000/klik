using System;

namespace Klik.Models;

public class AppSettings
{
    public string SendKeyName { get; set; } = "Enter";
    public int SendKeyVirtualCode { get; set; } = 0x0D; // VK_RETURN
    public bool StopOnDuration { get; set; } = false;
    public int StopAfterMinutes { get; set; } = 5;
    public int IntervalMinutes { get; set; } = 0;
    public int IntervalSeconds { get; set; } = 1;
    public int IntervalMilliseconds { get; set; } = 0;
    public bool IsAlwaysOnTop { get; set; } = false;
    public Guid? LastPresetId { get; set; }
}
