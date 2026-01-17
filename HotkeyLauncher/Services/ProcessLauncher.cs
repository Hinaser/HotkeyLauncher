using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using HotkeyLauncher.Models;

namespace HotkeyLauncher.Services;

public class ProcessLauncher
{
    public void LaunchOrActivate(HotkeyConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.ApplicationPath))
        {
            return;
        }

        // URLs should always be launched (no existing process check)
        if (IsUrl(config.ApplicationPath))
        {
            LaunchProcess(config);
            return;
        }

        var exeName = Path.GetFileNameWithoutExtension(config.ApplicationPath);
        if (string.IsNullOrEmpty(exeName))
        {
            LaunchProcess(config);
            return;
        }

        var existingProcesses = Process.GetProcessesByName(exeName);

        if (existingProcesses.Length > 0)
        {
            ActivateWindow(existingProcesses[0]);
        }
        else
        {
            LaunchProcess(config);
        }
    }

    private static bool IsUrl(string path)
    {
        return path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
    }

    private static void LaunchProcess(HotkeyConfig config)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = config.ApplicationPath,
                Arguments = config.Arguments,
                UseShellExecute = true
            };

            if (!string.IsNullOrWhiteSpace(config.WorkingDirectory))
            {
                startInfo.WorkingDirectory = config.WorkingDirectory;
            }

            if (config.RunAsAdmin)
            {
                startInfo.Verb = "runas";
            }

            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to launch process: {ex.Message}");
        }
    }

    private static void ActivateWindow(Process process)
    {
        try
        {
            var mainWindow = process.MainWindowHandle;

            if (mainWindow == IntPtr.Zero)
            {
                mainWindow = FindMainWindow(process.Id);
            }

            if (mainWindow == IntPtr.Zero)
            {
                return;
            }

            if (NativeMethods.IsIconic(mainWindow))
            {
                NativeMethods.ShowWindow(mainWindow, NativeMethods.SW_RESTORE);
            }

            ForceForegroundWindow(mainWindow);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to activate window: {ex.Message}");
        }
    }

    private static void ForceForegroundWindow(IntPtr targetWindow)
    {
        var foregroundWindow = NativeMethods.GetForegroundWindow();

        if (foregroundWindow == targetWindow)
        {
            return;
        }

        var foregroundThread = NativeMethods.GetWindowThreadProcessId(foregroundWindow, out _);
        var currentThread = NativeMethods.GetCurrentThreadId();

        if (foregroundThread != currentThread)
        {
            NativeMethods.AttachThreadInput(currentThread, foregroundThread, true);
            NativeMethods.SetForegroundWindow(targetWindow);
            NativeMethods.AttachThreadInput(currentThread, foregroundThread, false);
        }
        else
        {
            NativeMethods.SetForegroundWindow(targetWindow);
        }
    }

    private static IntPtr FindMainWindow(int processId)
    {
        IntPtr foundWindow = IntPtr.Zero;
        var gcHandle = GCHandle.Alloc((processId, IntPtr.Zero));

        try
        {
            NativeMethods.EnumWindows((hWnd, lParam) =>
            {
                NativeMethods.GetWindowThreadProcessId(hWnd, out var windowProcessId);

                if (windowProcessId == processId && NativeMethods.IsWindowVisible(hWnd))
                {
                    foundWindow = hWnd;
                    return false;
                }

                return true;
            }, IntPtr.Zero);
        }
        finally
        {
            gcHandle.Free();
        }

        return foundWindow;
    }
}
