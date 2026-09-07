using System;
using System.Globalization;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using CycloSketchAddin.Core;
using CycloSketchAddin.SolidWorks;
using CycloSketchAddin.UI;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swpublished;
using SolidWorks.Interop.swconst;

namespace CycloSketchAddin.Addin;

[ComVisible(true)]
[Guid("8718D6EA-69C1-44BF-AF89-A271CC8D7E3F")]
[ProgId("CycloSketchAddin.SwAddin")]
public sealed class SwAddin : ISwAddin
{
    private SldWorks? _app;
    private int _cookie;
    private CommandManagerService? _commandManager;
    private CycloPropertyManagerPage? _fallbackUi;
    private CycloPropertyManagerPageNative? _nativeUi;

    public bool ConnectToSW(object thisSw, int cookie)
    {
        _app = (SldWorks)thisSw;
        _cookie = cookie;

        _app.SetAddinCallbackInfo2(0, this, _cookie);

        _fallbackUi = new CycloPropertyManagerPage(GenerateSketches);
        _nativeUi = new CycloPropertyManagerPageNative(_app, _cookie, GenerateSketches, () => _fallbackUi.Show());
        _commandManager = new CommandManagerService(_app, _cookie, nameof(OnCreateCycloReducer));
        _commandManager.CreateUi();

        _app.SendMsgToUser2("CycloSketch Add-In loaded.",
            (int)swMessageBoxIcon_e.swMbInformation,
            (int)swMessageBoxBtn_e.swMbOk);

        return true;
    }

    public bool DisconnectFromSW()
    {
        _nativeUi = null;
        _fallbackUi = null;
        _commandManager = null;
        _app = null;
        return true;
    }

    public void OnCreateCycloReducer()
    {
        _fallbackUi?.Show();
    }

    public bool GenerateSketches(CycloParams p)
    {
        if (_app == null) return false;

        var model = _app.IActiveDoc2 as ModelDoc2;
        if (model == null)
        {
            _app.SendMsgToUser2("Open a Part document first.",
                (int)swMessageBoxIcon_e.swMbWarning,
                (int)swMessageBoxBtn_e.swMbOk);
            return false;
        }

        if (model.GetType() != (int)swDocumentTypes_e.swDocPART)
        {
            _app.SendMsgToUser2("Active document is not a Part.",
                (int)swMessageBoxIcon_e.swMbWarning,
                (int)swMessageBoxBtn_e.swMbOk);
            return false;
        }

        if (!CycloValidator.TryValidate(p, out var error))
        {
            _app.SendMsgToUser2(error,
                (int)swMessageBoxIcon_e.swMbStop,
                (int)swMessageBoxBtn_e.swMbOk);
            return false;
        }

        try
        {
            var generator = new SketchGenerator(model, new PlaneSelector());
            var generatedSketches = generator.Generate(p);

            GenerationSummaryDialog.Show(BuildGenerationSummary(p, generatedSketches));
            return true;
        }
        catch (Exception ex)
        {
            _app.SendMsgToUser2($"Generation error: {ex.Message}",
                (int)swMessageBoxIcon_e.swMbStop,
                (int)swMessageBoxBtn_e.swMbOk);
            return false;
        }
    }

    private static string BuildGenerationSummary(CycloParams p, System.Collections.Generic.IReadOnlyList<string> generatedSketches)
    {
        var culture = CultureInfo.InvariantCulture;
        var sb = new StringBuilder();

        sb.AppendLine("Cycloidal sketch generation succeeded.");
        sb.AppendLine();
        sb.AppendLine("Generated sketches:");
        for (int i = 0; i < generatedSketches.Count; i++)
            sb.AppendLine($"{i + 1}. {generatedSketches[i]}");

        sb.AppendLine();
        sb.AppendLine("Main parameters:");
        sb.AppendLine($"Reduction ratio: {p.ReductionRatio.ToString(culture)}");
        sb.AppendLine($"Eccentric amount [mm]: {p.EccentricMm.ToString("0.###", culture)}");
        sb.AppendLine($"Ring pin diameter [mm]: {p.RingPinDiaMm.ToString("0.###", culture)}");
        sb.AppendLine($"Ring pin pitch diameter [mm]: {p.RingPinPitchDiaMm.ToString("0.###", culture)}");
        sb.AppendLine($"Cycloidal curve plot per tooth: {p.PlotPerTooth.ToString(culture)}");

        sb.AppendLine();
        sb.AppendLine("Options:");
        sb.AppendLine($"Draw center hole: {p.DrawCenterHole}");
        if (p.DrawCenterHole)
            sb.AppendLine($"Center hole diameter [mm]: {p.CenterHoleDiaMm.ToString("0.###", culture)}");

        sb.AppendLine($"Draw around holes: {p.DrawAroundHoles}");
        if (p.DrawAroundHoles)
        {
            sb.AppendLine($"Around hole num: {p.AroundHoleNum.ToString(culture)}");
            sb.AppendLine($"Around hole diameter [mm]: {p.AroundHoleDiaMm.ToString("0.###", culture)}");
            sb.AppendLine($"Around hole position diameter [mm]: {p.AroundHolePositionDiaMm.ToString("0.###", culture)}");
        }

        sb.AppendLine($"Draw output disk pins: {p.DrawOutputDiskPins}");
        if (p.DrawOutputDiskPins)
        {
            sb.AppendLine($"Link output pins to around holes: {p.LinkOutputPinsToAroundHoles}");
            if (!p.LinkOutputPinsToAroundHoles)
            {
                sb.AppendLine($"Output pin num: {p.OutputPinNum.ToString(culture)}");
                sb.AppendLine($"Output pin diameter [mm]: {p.OutputPinDiaMm.ToString("0.###", culture)}");
                sb.AppendLine($"Output pin position diameter [mm]: {p.OutputPinPositionDiaMm.ToString("0.###", culture)}");
            }
        }

        sb.AppendLine($"Separate sketches: {p.SeparateSketches}");
        return sb.ToString();
    }

    [ComRegisterFunction]
    public static void Register(Type t)
    {
        var guid = $"{{{t.GUID}}}";
        var assemblyDir = Path.GetDirectoryName(t.Assembly.Location) ?? string.Empty;
        var logoPath = Path.Combine(assemblyDir, "assets", "logo", "Cycloidal.png");

        Microsoft.Win32.Registry.SetValue(
            $@"HKEY_LOCAL_MACHINE\SOFTWARE\SolidWorks\Addins\{guid}",
            null,
            1,
            Microsoft.Win32.RegistryValueKind.DWord);

        Microsoft.Win32.Registry.SetValue(
            $@"HKEY_CURRENT_USER\SOFTWARE\SolidWorks\AddInsStartup\{guid}",
            null,
            1,
            Microsoft.Win32.RegistryValueKind.DWord);

        Microsoft.Win32.Registry.SetValue(
            $@"HKEY_LOCAL_MACHINE\SOFTWARE\SolidWorks\Addins\{guid}",
            "Title",
            "CycloSketch Add-In");

        Microsoft.Win32.Registry.SetValue(
            $@"HKEY_LOCAL_MACHINE\SOFTWARE\SolidWorks\Addins\{guid}",
            "Description",
            "Cycloidal Drive Sketch Generator");

        if (File.Exists(logoPath))
        {
            Microsoft.Win32.Registry.SetValue(
                $@"HKEY_LOCAL_MACHINE\SOFTWARE\SolidWorks\Addins\{guid}",
                "Icon",
                logoPath);

            Microsoft.Win32.Registry.SetValue(
                $@"HKEY_LOCAL_MACHINE\SOFTWARE\SolidWorks\Addins\{guid}",
                "ToolbarImage",
                logoPath);
        }
    }

    [ComUnregisterFunction]
    public static void Unregister(Type t)
    {
        var guid = $"{{{t.GUID}}}";

        Microsoft.Win32.Registry.LocalMachine.DeleteSubKeyTree(
            $@"SOFTWARE\SolidWorks\Addins\{guid}", false);
        Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(
            $@"SOFTWARE\SolidWorks\AddInsStartup\{guid}", false);
    }
}
