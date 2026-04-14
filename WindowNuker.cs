using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace NukeIT
{
    public static class WindowNuker
    {
        private static readonly HashSet<string> Whitelist = new(StringComparer.OrdinalIgnoreCase)
        {
            "explorer",
            "dwm",
            "winlogon",
            "csrss",
            "system",
            "idle",

            // Logitech (confirmed)
            "logioptionsplus",
            "logioptionsplus_agent",
            "logioptionsplus_appbroker",
            "logioptionsplus_updater",
            "logipluginservice",
            "logipluginserviceext",
            "logi_lamparray_service"
        };

        public static void Nuke()
        {
            new Thread(() =>
            {
                int currentProcessId = Environment.ProcessId;

                // First pass: politely close visible app windows
                EnumWindows((hWnd, lParam) =>
                {
                    if (!IsWindowVisible(hWnd))
                        return true;

                    if (GetWindowTextLength(hWnd) == 0)
                        return true;

                    GetWindowThreadProcessId(hWnd, out uint processId);

                    if (processId == 0 || (int)processId == currentProcessId)
                        return true;

                    try
                    {
                        var proc = Process.GetProcessById((int)processId);
                        string name = proc.ProcessName.ToLowerInvariant();

                        if (Whitelist.Contains(name))
                            return true;

                        PostMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                    }
                    catch
                    {
                    }

                    return true;
                }, IntPtr.Zero);

                Thread.Sleep(150);

                // Second pass: only kill processes that still own visible windows
                EnumWindows((hWnd, lParam) =>
                {
                    if (!IsWindowVisible(hWnd))
                        return true;

                    if (GetWindowTextLength(hWnd) == 0)
                        return true;

                    GetWindowThreadProcessId(hWnd, out uint processId);

                    if (processId == 0 || (int)processId == currentProcessId)
                        return true;

                    try
                    {
                        var proc = Process.GetProcessById((int)processId);
                        string name = proc.ProcessName.ToLowerInvariant();

                        if (Whitelist.Contains(name))
                            return true;

                        if (!proc.HasExited)
                        {
                            proc.Kill(true);
                        }
                    }
                    catch
                    {
                    }

                    return true;
                }, IntPtr.Zero);

            })
            { IsBackground = true }.Start();
        }

        private const int WM_CLOSE = 0x0010;

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
    }
}