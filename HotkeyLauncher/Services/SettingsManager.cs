using System.IO;
using System.Text.Json;
using HotkeyLauncher.Models;

namespace HotkeyLauncher.Services;

public class SettingsManager
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly string _settingsPath;

    public AppSettings Settings { get; set; } = AppSettings.CreateDefault();

    public SettingsManager()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataPath, "HotkeyLauncher");
        Directory.CreateDirectory(appFolder);
        _settingsPath = Path.Combine(appFolder, "settings.json");
    }

    public SettingsManager(string settingsPath)
    {
        _settingsPath = settingsPath;
        var directory = Path.GetDirectoryName(settingsPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public void Load()
    {
        if (!File.Exists(_settingsPath))
        {
            Settings = AppSettings.CreateDefault();
            Save();
            return;
        }

        try
        {
            var json = File.ReadAllText(_settingsPath);
            Settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? AppSettings.CreateDefault();
        }
        catch (JsonException)
        {
            Settings = AppSettings.CreateDefault();
            Save();
        }
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(Settings, JsonOptions);
        File.WriteAllText(_settingsPath, json);
    }

    public void AddHotkey(HotkeyConfig config)
    {
        Settings.Hotkeys.Add(config);
        Save();
    }

    public void UpdateHotkey(HotkeyConfig config)
    {
        var index = Settings.Hotkeys.FindIndex(h => h.Id == config.Id);
        if (index >= 0)
        {
            Settings.Hotkeys[index] = config;
            Save();
        }
    }

    public void RemoveHotkey(Guid id)
    {
        Settings.Hotkeys.RemoveAll(h => h.Id == id);
        Save();
    }

    public string SettingsFilePath => _settingsPath;
}
