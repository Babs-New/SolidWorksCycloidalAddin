using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace CycloSketchAddin.Addin;

public sealed class CommandManagerService
{
    private const int MainCmdGroupId = 5;
    private const int MainCmdId = 1;

    private readonly SldWorks _app;
    private readonly int _cookie;
    private readonly string _callbackMethodName;

    public CommandManagerService(SldWorks app, int cookie, string callbackMethodName)
    {
        _app = app;
        _cookie = cookie;
        _callbackMethodName = callbackMethodName;
    }

    public int CreateUi()
    {
        int errors = 0;
        var cmdMgr = _app.GetCommandManager(_cookie);

        var cmdGroup = cmdMgr.CreateCommandGroup2(
            MainCmdGroupId,
            "CycloSketch",
            "Cycloidal Drive Sketch Generator",
            "Tools",
            -1,
            true,
            ref errors
        );

        cmdGroup.HasToolbar = true;
        cmdGroup.HasMenu = true;
        ConfigureCommandGroupIcon(cmdGroup);

        int commandIndex = cmdGroup.AddCommandItem2(
            "Create Cyclo Reducer",
            -1,
            "Generate cycloidal sketches",
            "Create Cyclo Reducer",
            0,
            _callbackMethodName,
            string.Empty,
            MainCmdId,
            (int)swCommandItemType_e.swMenuItem | (int)swCommandItemType_e.swToolbarItem
        );

        cmdGroup.Activate();

        return commandIndex;
    }

    private static void ConfigureCommandGroupIcon(object cmdGroup)
    {
        var pngPath = ResolveIconPath("Cycloidal.png");
        if (string.IsNullOrWhiteSpace(pngPath))
            return;

        var icons = EnsureBitmapIcons(pngPath);
        if (icons == null)
            return;

        // SolidWorks command UI expects bitmap icon strips for best compatibility.
        TrySetProperty(cmdGroup, "SmallMainIcon", icons.SmallBitmapPath);
        TrySetProperty(cmdGroup, "LargeMainIcon", icons.LargeBitmapPath);
        TrySetProperty(cmdGroup, "SmallIconList", icons.SmallBitmapPath);
        TrySetProperty(cmdGroup, "LargeIconList", icons.LargeBitmapPath);
        TrySetProperty(cmdGroup, "IconList", icons.SmallBitmapPath);
        TrySetProperty(cmdGroup, "MainIconList", icons.LargeBitmapPath);
    }

    private static GeneratedIcons? EnsureBitmapIcons(string pngPath)
    {
        try
        {
            var dir = Path.GetDirectoryName(pngPath);
            if (string.IsNullOrWhiteSpace(dir)) return null;

            string small = Path.Combine(dir, "Cycloidal_20.bmp");
            string large = Path.Combine(dir, "Cycloidal_32.bmp");

            bool regen = !File.Exists(small) || !File.Exists(large) ||
                File.GetLastWriteTimeUtc(small) < File.GetLastWriteTimeUtc(pngPath) ||
                File.GetLastWriteTimeUtc(large) < File.GetLastWriteTimeUtc(pngPath);

            if (regen)
            {
                using var src = Image.FromFile(pngPath);
                SaveScaledBitmap(src, small, 20, 20);
                SaveScaledBitmap(src, large, 32, 32);
            }

            return new GeneratedIcons(small, large);
        }
        catch
        {
            return null;
        }
    }

    private static void SaveScaledBitmap(Image source, string targetPath, int width, int height)
    {
        using var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.Clear(Color.Transparent);
            g.DrawImage(source, 0, 0, width, height);
        }

        bmp.Save(targetPath, ImageFormat.Bmp);
    }

    private static string? ResolveIconPath(string fileName)
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

    private static void TrySetProperty(object target, string propertyName, object value)
    {
        try
        {
            var property = target.GetType().GetProperty(propertyName);
            property?.SetValue(target, value);
        }
        catch
        {
            // Some SolidWorks interop versions do not expose all icon properties.
        }
    }

    private sealed class GeneratedIcons
    {
        public GeneratedIcons(string smallBitmapPath, string largeBitmapPath)
        {
            SmallBitmapPath = smallBitmapPath;
            LargeBitmapPath = largeBitmapPath;
        }

        public string SmallBitmapPath { get; }
        public string LargeBitmapPath { get; }
    }

}
