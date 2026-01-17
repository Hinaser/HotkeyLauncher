namespace HotkeyLauncher.Models;

public class AppSettings
{
    public List<HotkeyConfig> Hotkeys { get; set; } = [];

    public bool StartMinimized { get; set; } = true;

    public bool StartWithWindows { get; set; } = false;

    public static AppSettings CreateDefault()
    {
        return new AppSettings
        {
            Hotkeys = [],
            StartMinimized = true,
            StartWithWindows = false
        };
    }
}
