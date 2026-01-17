using Microsoft.Win32;

namespace HotkeyLauncher.Services;

public static class StartupManager
{
    private const string AppName = "HotkeyLauncher";
    private const string RegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

    public static bool IsRegistered
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, false);
            return key?.GetValue(AppName) != null;
        }
    }

    public static void Register()
    {
        var exePath = Environment.ProcessPath;
        if (string.IsNullOrEmpty(exePath)) return;

        using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, true);
        key?.SetValue(AppName, $"\"{exePath}\"");
    }

    public static void Unregister()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, true);
        key?.DeleteValue(AppName, false);
    }

    public static void SetStartup(bool enable)
    {
        if (enable)
            Register();
        else
            Unregister();
    }
}
