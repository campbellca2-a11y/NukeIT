using System;
using System.IO;

namespace NukeIT
{
    public static class AnchorManager
    {
        public static void EnsureDesktopAnchor()
        {
            try
            {
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string shortcutPath = Path.Combine(desktop, "NukeIT.lnk");
                string exePath = Environment.ProcessPath ?? string.Empty;

                if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
                    return;

                if (File.Exists(shortcutPath))
                    return;

                Type? shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null)
                    return;

                dynamic shell = Activator.CreateInstance(shellType)!;
                dynamic shortcut = shell.CreateShortcut(shortcutPath);
                shortcut.TargetPath = exePath;
                shortcut.WorkingDirectory = Path.GetDirectoryName(exePath) ?? desktop;
                shortcut.Description = "NukeIT";
                shortcut.WindowStyle = 1;
                shortcut.Save();
            }
            catch
            {
            }
        }
    }
}