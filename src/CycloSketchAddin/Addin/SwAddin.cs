using System;
using System.Runtime.InteropServices;
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
            generator.Generate(p);

            _app.SendMsgToUser2("Cycloidal sketches generated.",
                (int)swMessageBoxIcon_e.swMbInformation,
                (int)swMessageBoxBtn_e.swMbOk);
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

    [ComRegisterFunction]
    public static void Register(Type t)
    {
        var guid = $"{{{t.GUID}}}";

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
