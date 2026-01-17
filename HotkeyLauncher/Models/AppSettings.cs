using HotkeyLauncher.Services;

namespace HotkeyLauncher.Models;

public class AppSettings
{
    public List<HotkeyConfig> Hotkeys { get; set; } = [];

    public bool StartMinimized { get; set; } = true;

    public bool StartWithWindows { get; set; } = false;

    public AppTheme Theme { get; set; } = AppTheme.Dark;

    public static AppSettings CreateDefault()
    {
        return new AppSettings
        {
            Hotkeys = [],
            StartMinimized = true,
            StartWithWindows = false,
            Theme = AppTheme.Dark
        };
    }
}
