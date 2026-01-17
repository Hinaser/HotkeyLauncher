using System.Windows;
using HotkeyLauncher.Models;
using HotkeyLauncher.Services;

namespace HotkeyLauncher;

public partial class App : System.Windows.Application
{
    private TrayIconManager? _trayIconManager;
    private HotkeyManager? _hotkeyManager;
    private SettingsManager? _settingsManager;
    private ProcessLauncher? _processLauncher;
    private MainWindow? _mainWindow;
    private Window? _hiddenWindow;

    private readonly Dictionary<int, HotkeyConfig> _hotkeyIdToConfig = [];

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        _settingsManager = new SettingsManager();
        _settingsManager.Load();

        _processLauncher = new ProcessLauncher();

        _trayIconManager = new TrayIconManager();
        _trayIconManager.SettingsRequested += OnSettingsRequested;
        _trayIconManager.ExitRequested += OnExitRequested;
        _trayIconManager.Show();

        // Create a hidden window for hotkey registration
        _hiddenWindow = new Window
        {
            Width = 0,
            Height = 0,
            WindowStyle = WindowStyle.None,
            ShowInTaskbar = false,
            ShowActivated = false
        };
        _hiddenWindow.Show();
        _hiddenWindow.Hide();

        _hotkeyManager = new HotkeyManager();
        _hotkeyManager.Initialize(_hiddenWindow);

        RegisterAllHotkeys();
    }

    private void Application_Exit(object sender, ExitEventArgs e)
    {
        _hotkeyManager?.Dispose();
        _trayIconManager?.Dispose();
        _hiddenWindow?.Close();
    }

    private void OnSettingsRequested(object? sender, EventArgs e)
    {
        ShowSettingsWindow();
    }

    private void OnExitRequested(object? sender, EventArgs e)
    {
        Shutdown();
    }

    public void ShowSettingsWindow()
    {
        if (_mainWindow == null || !_mainWindow.IsLoaded)
        {
            _mainWindow = new MainWindow();
            _mainWindow.Closed += (s, e) => _mainWindow = null;
            _mainWindow.SettingsSaved += OnSettingsSaved;
        }

        _mainWindow.LoadSettings(_settingsManager!.Settings, _settingsManager.SettingsFilePath);
        _mainWindow.Show();
        _mainWindow.Activate();
    }

    private void OnSettingsSaved(object? sender, AppSettings settings)
    {
        _settingsManager!.Settings = settings;
        _settingsManager.Save();

        UnregisterAllHotkeys();
        RegisterAllHotkeys();
    }

    private void RegisterAllHotkeys()
    {
        _hotkeyIdToConfig.Clear();

        foreach (var config in _settingsManager!.Settings.Hotkeys)
        {
            if (string.IsNullOrWhiteSpace(config.ApplicationPath))
            {
                continue;
            }

            if (_hotkeyManager!.TryRegisterHotkey(config.Modifiers, config.Key, null, out int id))
            {
                config.RegisteredId = id;
                _hotkeyIdToConfig[id] = config;
                _hotkeyManager.SetAction(id, () => OnHotkeyPressed(config));
            }
            else
            {
                _trayIconManager?.ShowBalloonTip(
                    "HotkeyLauncher",
                    $"Failed to register hotkey: {config.HotkeyDisplayText}",
                    System.Windows.Forms.ToolTipIcon.Warning);
            }
        }
    }

    private void UnregisterAllHotkeys()
    {
        _hotkeyManager?.UnregisterAllHotkeys();
        _hotkeyIdToConfig.Clear();
    }

    private void OnHotkeyPressed(HotkeyConfig config)
    {
        _processLauncher?.LaunchOrActivate(config);
    }
}
