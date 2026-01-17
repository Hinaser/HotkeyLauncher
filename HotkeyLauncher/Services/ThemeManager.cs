using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Color = System.Windows.Media.Color;

namespace HotkeyLauncher.Services;

public enum AppTheme
{
    Dark,
    Light
}

public static class ThemeManager
{
    public static AppTheme CurrentTheme { get; private set; } = AppTheme.Dark;

    public static void ApplyTheme(Window window, AppTheme theme)
    {
        CurrentTheme = theme;
        ApplyTitleBarTheme(window, theme);
        ApplyWindowTheme(window, theme);
    }

    private static void ApplyTitleBarTheme(Window window, AppTheme theme)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero) return;

        int useDarkMode = theme == AppTheme.Dark ? 1 : 0;
        NativeMethods.DwmSetWindowAttribute(hwnd, NativeMethods.DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDarkMode, sizeof(int));
    }

    private static void ApplyWindowTheme(Window window, AppTheme theme)
    {
        var resources = window.Resources;

        if (theme == AppTheme.Dark)
        {
            resources["PrimaryColor"] = Color.FromRgb(0, 120, 212);
            resources["PrimaryHoverColor"] = Color.FromRgb(16, 132, 216);
            resources["BackgroundColor"] = Color.FromRgb(30, 30, 30);
            resources["SurfaceColor"] = Color.FromRgb(45, 45, 45);
            resources["SurfaceHoverColor"] = Color.FromRgb(61, 61, 61);
            resources["BorderColor"] = Color.FromRgb(64, 64, 64);
            resources["TextColor"] = Color.FromRgb(255, 255, 255);
            resources["TextSecondaryColor"] = Color.FromRgb(160, 160, 160);
            resources["DangerColor"] = Color.FromRgb(240, 85, 69);
            resources["DangerHoverColor"] = Color.FromRgb(255, 102, 89);

            resources["PrimaryBrush"] = new SolidColorBrush(Color.FromRgb(0, 120, 212));
            resources["BackgroundBrush"] = new SolidColorBrush(Color.FromRgb(30, 30, 30));
            resources["SurfaceBrush"] = new SolidColorBrush(Color.FromRgb(45, 45, 45));
            resources["BorderBrush"] = new SolidColorBrush(Color.FromRgb(64, 64, 64));
            resources["TextBrush"] = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            resources["TextSecondaryBrush"] = new SolidColorBrush(Color.FromRgb(160, 160, 160));
            resources["InputBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(37, 37, 37));
            resources["SurfaceHoverBrush"] = new SolidColorBrush(Color.FromRgb(58, 58, 58));
            resources["BadgeBrush"] = new SolidColorBrush(Color.FromRgb(64, 64, 64));

            window.Background = new SolidColorBrush(Color.FromRgb(30, 30, 30));
        }
        else
        {
            resources["PrimaryColor"] = Color.FromRgb(0, 120, 212);
            resources["PrimaryHoverColor"] = Color.FromRgb(16, 132, 216);
            resources["BackgroundColor"] = Color.FromRgb(249, 249, 249);
            resources["SurfaceColor"] = Color.FromRgb(255, 255, 255);
            resources["SurfaceHoverColor"] = Color.FromRgb(243, 243, 243);
            resources["BorderColor"] = Color.FromRgb(200, 200, 200);
            resources["TextColor"] = Color.FromRgb(32, 32, 32);
            resources["TextSecondaryColor"] = Color.FromRgb(96, 96, 96);
            resources["DangerColor"] = Color.FromRgb(196, 43, 28);
            resources["DangerHoverColor"] = Color.FromRgb(220, 60, 45);

            resources["PrimaryBrush"] = new SolidColorBrush(Color.FromRgb(0, 120, 212));
            resources["BackgroundBrush"] = new SolidColorBrush(Color.FromRgb(249, 249, 249));
            resources["SurfaceBrush"] = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            resources["BorderBrush"] = new SolidColorBrush(Color.FromRgb(200, 200, 200));
            resources["TextBrush"] = new SolidColorBrush(Color.FromRgb(32, 32, 32));
            resources["TextSecondaryBrush"] = new SolidColorBrush(Color.FromRgb(96, 96, 96));
            resources["InputBackgroundBrush"] = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            resources["SurfaceHoverBrush"] = new SolidColorBrush(Color.FromRgb(230, 230, 230));
            resources["BadgeBrush"] = new SolidColorBrush(Color.FromRgb(220, 220, 220));

            window.Background = new SolidColorBrush(Color.FromRgb(249, 249, 249));
        }
    }
}
