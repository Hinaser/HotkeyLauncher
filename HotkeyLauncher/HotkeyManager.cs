using System.Windows;
using System.Windows.Interop;

namespace HotkeyLauncher;

public class HotkeyManager : IDisposable
{
    private readonly Dictionary<int, Action> _hotkeyActions = [];
    private HwndSource? _hwndSource;
    private IntPtr _windowHandle;
    private int _currentId;
    private bool _disposed;

    public event EventHandler<HotkeyEventArgs>? HotkeyPressed;

    public void Initialize(Window window)
    {
        var helper = new WindowInteropHelper(window);
        helper.EnsureHandle();
        _windowHandle = helper.Handle;
        _hwndSource = HwndSource.FromHwnd(_windowHandle);
        _hwndSource?.AddHook(WndProc);
    }

    public int RegisterHotkey(uint modifiers, uint key, Action? action = null)
    {
        int id = ++_currentId;

        if (!NativeMethods.RegisterHotKey(_windowHandle, id, modifiers | NativeMethods.MOD_NOREPEAT, key))
        {
            int error = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
            throw new InvalidOperationException($"Failed to register hotkey. Error code: {error}");
        }

        if (action != null)
        {
            _hotkeyActions[id] = action;
        }

        return id;
    }

    public bool TryRegisterHotkey(uint modifiers, uint key, Action? action, out int id)
    {
        id = ++_currentId;

        if (!NativeMethods.RegisterHotKey(_windowHandle, id, modifiers | NativeMethods.MOD_NOREPEAT, key))
        {
            id = 0;
            return false;
        }

        if (action != null)
        {
            _hotkeyActions[id] = action;
        }

        return true;
    }

    public void UnregisterHotkey(int id)
    {
        NativeMethods.UnregisterHotKey(_windowHandle, id);
        _hotkeyActions.Remove(id);
    }

    public void UnregisterAllHotkeys()
    {
        foreach (var id in _hotkeyActions.Keys.ToList())
        {
            UnregisterHotkey(id);
        }
    }

    public void SetAction(int id, Action action)
    {
        _hotkeyActions[id] = action;
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_HOTKEY)
        {
            int id = wParam.ToInt32();

            HotkeyPressed?.Invoke(this, new HotkeyEventArgs(id));

            if (_hotkeyActions.TryGetValue(id, out var action))
            {
                action.Invoke();
                handled = true;
            }
        }

        return IntPtr.Zero;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            UnregisterAllHotkeys();
            _hwndSource?.RemoveHook(WndProc);
            _hwndSource?.Dispose();
        }

        _disposed = true;
    }

    ~HotkeyManager()
    {
        Dispose(false);
    }
}

public class HotkeyEventArgs(int id) : EventArgs
{
    public int Id { get; } = id;
}
