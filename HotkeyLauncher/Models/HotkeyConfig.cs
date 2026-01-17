using System.Text.Json.Serialization;

namespace HotkeyLauncher.Models;

public class HotkeyConfig
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public uint Modifiers { get; set; }

    public uint Key { get; set; }

    public string ApplicationPath { get; set; } = string.Empty;

    public string Arguments { get; set; } = string.Empty;

    public string WorkingDirectory { get; set; } = string.Empty;

    [JsonIgnore]
    public int RegisteredId { get; set; }

    [JsonIgnore]
    public string ModifierText => GetModifierText();

    [JsonIgnore]
    public string KeyText => GetKeyText();

    [JsonIgnore]
    public string HotkeyDisplayText => string.IsNullOrEmpty(ModifierText)
        ? KeyText
        : $"{ModifierText} + {KeyText}";

    private string GetModifierText()
    {
        var parts = new List<string>();

        if ((Modifiers & NativeMethods.MOD_CONTROL) != 0) parts.Add("Ctrl");
        if ((Modifiers & NativeMethods.MOD_ALT) != 0) parts.Add("Alt");
        if ((Modifiers & NativeMethods.MOD_SHIFT) != 0) parts.Add("Shift");
        if ((Modifiers & NativeMethods.MOD_WIN) != 0) parts.Add("Win");

        return string.Join(" + ", parts);
    }

    private string GetKeyText()
    {
        return Key switch
        {
            >= NativeMethods.VK_0 and <= NativeMethods.VK_9 => ((char)Key).ToString(),
            >= 0x41 and <= 0x5A => ((char)Key).ToString(),
            NativeMethods.VK_F13 => "F13",
            NativeMethods.VK_F14 => "F14",
            NativeMethods.VK_F15 => "F15",
            NativeMethods.VK_F16 => "F16",
            >= 0x70 and <= 0x7B => $"F{Key - 0x70 + 1}",
            _ => $"0x{Key:X2}"
        };
    }

    public static HotkeyConfig CreateDefault(int number)
    {
        uint key = number switch
        {
            0 => NativeMethods.VK_0,
            _ => (uint)(NativeMethods.VK_1 + number - 1)
        };

        return new HotkeyConfig
        {
            Name = $"Slot {number}",
            Modifiers = NativeMethods.MOD_NONE,
            Key = (uint)NativeMethods.VK_F13 | (key << 16),
        };
    }
}
