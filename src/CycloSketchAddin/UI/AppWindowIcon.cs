using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace CycloSketchAddin.UI;

internal static class AppWindowIcon
{
    public static Icon? TryGetIcon()
    {
        var pngPath = ResolveLogoPath("Cycloidal.png");
        if (string.IsNullOrWhiteSpace(pngPath))
            return null;

        try
        {
            using var bmp = new Bitmap(pngPath);
            IntPtr hIcon = bmp.GetHicon();
            try
            {
                using var raw = Icon.FromHandle(hIcon);
                return (Icon)raw.Clone();
            }
            finally
            {
                _ = DestroyIcon(hIcon);
            }
        }
        catch
        {
            return null;
        }
    }

    private static string? ResolveLogoPath(string fileName)
    {
        string[] candidates =
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "logo", fileName),
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "assets", "logo", fileName),
            Path.Combine(Directory.GetCurrentDirectory(), "assets", "logo", fileName)
        };

        foreach (var candidate in candidates)
        {
            if (!string.IsNullOrWhiteSpace(candidate) && File.Exists(candidate))
                return candidate;
        }

        var root = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? AppDomain.CurrentDomain.BaseDirectory);
        for (int i = 0; i < 8 && root != null; i++)
        {
            var nested = Path.Combine(root.FullName, "assets", "logo", fileName);
            if (File.Exists(nested))
                return nested;

            root = root.Parent;
        }

        return null;
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool DestroyIcon(IntPtr handle);
}
