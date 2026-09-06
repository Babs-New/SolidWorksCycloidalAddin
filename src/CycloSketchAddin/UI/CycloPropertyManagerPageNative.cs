using System;
using CycloSketchAddin.Core;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace CycloSketchAddin.UI;

// Native PropertyManagerPage wrapper.
// Uses dynamic COM access so this scaffold stays resilient across minor interop variations.
public sealed class CycloPropertyManagerPageNative
{
    private readonly SldWorks _app;
    private readonly int _cookie;
    private readonly Func<CycloParams, bool> _onGenerate;
    private readonly Action _fallbackShow;

    private dynamic? _page;
    private readonly CycloParams _params = new();

    private dynamic? _nbReduction;
    private dynamic? _nbEccentric;
    private dynamic? _nbRingPinDia;
    private dynamic? _nbRingPinPitchDia;
    private dynamic? _nbPlotPerTooth;

    private dynamic? _chkCenterHole;
    private dynamic? _nbCenterHoleDia;

    private dynamic? _chkAroundHoles;
    private dynamic? _chkOutputPins;
    private dynamic? _optSetAboutAround;
    private dynamic? _optSetAboutOutput;
    private dynamic? _nbAroundNum;
    private dynamic? _nbAroundDia;
    private dynamic? _nbAroundPosDia;
    private dynamic? _nbOutputNum;
    private dynamic? _nbOutputDia;
    private dynamic? _nbOutputPosDia;

    private dynamic? _chkSeparateSketch;

    public CycloPropertyManagerPageNative(
        SldWorks app,
        int cookie,
        Func<CycloParams, bool> onGenerate,
        Action fallbackShow)
    {
        _app = app;
        _cookie = cookie;
        _onGenerate = onGenerate;
        _fallbackShow = fallbackShow;
    }

    public void Show()
    {
        try
        {
            if (!EnsurePage())
            {
                _fallbackShow();
                return;
            }

            _page.Show2(0);
        }
        catch
        {
            _fallbackShow();
        }
    }

    private bool EnsurePage()
    {
        if (_page != null) return true;

        try
        {
            int errors = 0;
            dynamic cmdMgr = _app.GetCommandManager(_cookie);

            int options =
                (int)swPropertyManagerPageOptions_e.swPropertyManagerOptions_OkayButton |
                (int)swPropertyManagerPageOptions_e.swPropertyManagerOptions_CancelButton;

            _page = cmdMgr.CreatePropertyManagerPage(
                "Create Cycloidal Reducer Sketches",
                options,
                new PmpHandlerBridge(this),
                ref errors);

            if (_page == null || errors != 0) return false;

            BuildUi();
            PushDefaultsToControls();
            ApplyEnableVisibilityRules();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void BuildUi()
    {
        const int tabNecessary = 1;
        const int tabOptional = 2;
        const int tabDetailed = 3;

        dynamic t1 = _page.AddTab(tabNecessary, "Necessary param", "", 0);
        dynamic t2 = _page.AddTab(tabOptional, "Optional param", "", 0);
        dynamic t3 = _page.AddTab(tabDetailed, "Detailed setting", "", 0);

        dynamic g1 = t1.AddGroupBox(10, "Necessary parameters", (int)swAddGroupBoxOptions_e.swGroupBoxOptions_Expanded);
        dynamic g2a = t2.AddGroupBox(20, "Cycloidal gear center hole", (int)swAddGroupBoxOptions_e.swGroupBoxOptions_Expanded);
        dynamic g2b = t2.AddGroupBox(21, "Cycloidal gear to output disk", (int)swAddGroupBoxOptions_e.swGroupBoxOptions_Expanded);
        dynamic g3 = t3.AddGroupBox(30, "Separate", (int)swAddGroupBoxOptions_e.swGroupBoxOptions_Expanded);

        _nbReduction = AddNumberBox(g1, 1001, "Reduction ratio", "-");
        _nbEccentric = AddNumberBox(g1, 1002, "Eccentric amount", "mm");
        _nbRingPinDia = AddNumberBox(g1, 1003, "Ring pin diameter", "mm");
        _nbRingPinPitchDia = AddNumberBox(g1, 1004, "Ring pin pitch diameter", "mm");
        _nbPlotPerTooth = AddNumberBox(g1, 1005, "Cycloidal curve plot per tooth", "-");

        _chkCenterHole = AddCheckBox(g2a, 2001, "Draw center hole");
        _nbCenterHoleDia = AddNumberBox(g2a, 2002, "Diameter", "mm");

        _chkAroundHoles = AddCheckBox(g2b, 2101, "Draw around hole");
        _chkOutputPins = AddCheckBox(g2b, 2102, "Draw output disk pin");

        dynamic setAboutLabel = g2b.AddControl2(
            2103,
            (short)swPropertyManagerPageControlType_e.swControlType_Label,
            "Set about",
            (short)swPropertyManagerPageControlLeftAlign_e.swControlAlign_LeftEdge,
            0,
            "Choose master geometry");

        _optSetAboutAround = AddOption(g2b, 2104, "Cycloidal gear hole");
        _optSetAboutOutput = AddOption(g2b, 2105, "Output disk pin");

        _nbAroundNum = AddNumberBox(g2b, 2110, "Hole num", "-");
        _nbAroundDia = AddNumberBox(g2b, 2111, "Hole diameter", "mm");
        _nbAroundPosDia = AddNumberBox(g2b, 2112, "Hole position diameter", "mm");

        _nbOutputNum = AddNumberBox(g2b, 2120, "Pin num", "-");
        _nbOutputDia = AddNumberBox(g2b, 2121, "Pin diameter", "mm");
        _nbOutputPosDia = AddNumberBox(g2b, 2122, "Pin position diameter", "mm");

        _chkSeparateSketch = AddCheckBox(g3, 3001, "Separate sketch");

        _ = setAboutLabel;
    }

    private dynamic AddNumberBox(dynamic group, int id, string caption, string tip)
    {
        dynamic ctrl = group.AddControl2(
            id,
            (short)swPropertyManagerPageControlType_e.swControlType_Numberbox,
            caption,
            (short)swPropertyManagerPageControlLeftAlign_e.swControlAlign_LeftEdge,
            0,
            tip);
        return ctrl;
    }

    private dynamic AddCheckBox(dynamic group, int id, string caption)
    {
        return group.AddControl2(
            id,
            (short)swPropertyManagerPageControlType_e.swControlType_Checkbox,
            caption,
            (short)swPropertyManagerPageControlLeftAlign_e.swControlAlign_LeftEdge,
            0,
            caption);
    }

    private dynamic AddOption(dynamic group, int id, string caption)
    {
        return group.AddControl2(
            id,
            (short)swPropertyManagerPageControlType_e.swControlType_Option,
            caption,
            (short)swPropertyManagerPageControlLeftAlign_e.swControlAlign_LeftEdge,
            0,
            caption);
    }

    private void PushDefaultsToControls()
    {
        TrySet(_nbReduction, "Value", _params.ReductionRatio);
        TrySet(_nbEccentric, "Value", _params.EccentricMm);
        TrySet(_nbRingPinDia, "Value", _params.RingPinDiaMm);
        TrySet(_nbRingPinPitchDia, "Value", _params.RingPinPitchDiaMm);
        TrySet(_nbPlotPerTooth, "Value", _params.PlotPerTooth);

        TrySet(_chkCenterHole, "Checked", _params.DrawCenterHole);
        TrySet(_nbCenterHoleDia, "Value", _params.CenterHoleDiaMm);

        TrySet(_chkAroundHoles, "Checked", _params.DrawAroundHoles);
        TrySet(_chkOutputPins, "Checked", _params.DrawOutputDiskPins);

        TrySet(_optSetAboutAround, "Checked", _params.LinkOutputPinsToAroundHoles);
        TrySet(_optSetAboutOutput, "Checked", !_params.LinkOutputPinsToAroundHoles);

        TrySet(_nbAroundNum, "Value", _params.AroundHoleNum);
        TrySet(_nbAroundDia, "Value", _params.AroundHoleDiaMm);
        TrySet(_nbAroundPosDia, "Value", _params.AroundHolePositionDiaMm);

        TrySet(_nbOutputNum, "Value", _params.OutputPinNum);
        TrySet(_nbOutputDia, "Value", _params.OutputPinDiaMm);
        TrySet(_nbOutputPosDia, "Value", _params.OutputPinPositionDiaMm);

        TrySet(_chkSeparateSketch, "Checked", _params.SeparateSketches);
    }

    internal void OnControlChanged(int id)
    {
        PullControlsToParams();
        ApplyEnableVisibilityRules();
    }

    internal void OnPageClosed(int reason)
    {
        // 1 is expected to be OK across SolidWorks versions.
        if (reason != 1) return;

        PullControlsToParams();

        if (!CycloValidator.TryValidate(_params, out var error))
        {
            _app.SendMsgToUser2(error,
                (int)swMessageBoxIcon_e.swMbWarning,
                (int)swMessageBoxBtn_e.swMbOk);
            return;
        }

        _ = _onGenerate(_params);
    }

    private void PullControlsToParams()
    {
        _params.ReductionRatio = ToInt(GetDouble(_nbReduction, _params.ReductionRatio));
        _params.EccentricMm = GetDouble(_nbEccentric, _params.EccentricMm);
        _params.RingPinDiaMm = GetDouble(_nbRingPinDia, _params.RingPinDiaMm);
        _params.RingPinPitchDiaMm = GetDouble(_nbRingPinPitchDia, _params.RingPinPitchDiaMm);
        _params.PlotPerTooth = ToInt(GetDouble(_nbPlotPerTooth, _params.PlotPerTooth));

        _params.DrawCenterHole = GetBool(_chkCenterHole, _params.DrawCenterHole);
        _params.CenterHoleDiaMm = GetDouble(_nbCenterHoleDia, _params.CenterHoleDiaMm);

        _params.DrawAroundHoles = GetBool(_chkAroundHoles, _params.DrawAroundHoles);
        _params.DrawOutputDiskPins = GetBool(_chkOutputPins, _params.DrawOutputDiskPins);
        _params.LinkOutputPinsToAroundHoles = GetBool(_optSetAboutAround, _params.LinkOutputPinsToAroundHoles);

        _params.AroundHoleNum = ToInt(GetDouble(_nbAroundNum, _params.AroundHoleNum));
        _params.AroundHoleDiaMm = GetDouble(_nbAroundDia, _params.AroundHoleDiaMm);
        _params.AroundHolePositionDiaMm = GetDouble(_nbAroundPosDia, _params.AroundHolePositionDiaMm);

        _params.OutputPinNum = ToInt(GetDouble(_nbOutputNum, _params.OutputPinNum));
        _params.OutputPinDiaMm = GetDouble(_nbOutputDia, _params.OutputPinDiaMm);
        _params.OutputPinPositionDiaMm = GetDouble(_nbOutputPosDia, _params.OutputPinPositionDiaMm);

        _params.SeparateSketches = GetBool(_chkSeparateSketch, _params.SeparateSketches);
    }

    private void ApplyEnableVisibilityRules()
    {
        bool centerEnabled = _params.DrawCenterHole;
        TrySet(_nbCenterHoleDia, "Enabled", centerEnabled);

        bool aroundEnabled = _params.DrawAroundHoles;
        TrySet(_nbAroundNum, "Enabled", aroundEnabled);
        TrySet(_nbAroundDia, "Enabled", aroundEnabled);
        TrySet(_nbAroundPosDia, "Enabled", aroundEnabled);

        bool outputEnabled = _params.DrawOutputDiskPins;
        TrySet(_optSetAboutAround, "Enabled", outputEnabled && aroundEnabled);
        TrySet(_optSetAboutOutput, "Enabled", outputEnabled);

        bool outputManual = outputEnabled && !_params.LinkOutputPinsToAroundHoles;
        TrySet(_nbOutputNum, "Enabled", outputManual);
        TrySet(_nbOutputDia, "Enabled", outputManual);
        TrySet(_nbOutputPosDia, "Enabled", outputManual);
    }

    private static int ToInt(double value)
    {
        if (value < int.MinValue) return int.MinValue;
        if (value > int.MaxValue) return int.MaxValue;
        return (int)Math.Round(value);
    }

    private static double GetDouble(dynamic? ctrl, double fallback)
    {
        try { return (double)ctrl.Value; }
        catch { return fallback; }
    }

    private static bool GetBool(dynamic? ctrl, bool fallback)
    {
        try { return (bool)ctrl.Checked; }
        catch { return fallback; }
    }

    private static void TrySet(dynamic? ctrl, string propName, object value)
    {
        if (ctrl == null) return;
        try
        {
            var t = ctrl.GetType();
            var p = t.GetProperty(propName);
            p?.SetValue(ctrl, value);
        }
        catch
        {
            // Best-effort across interop variants.
        }
    }

    // Bridge object exposing common callback names expected by PropertyManagerPage.
    private sealed class PmpHandlerBridge
    {
        private readonly CycloPropertyManagerPageNative _owner;

        public PmpHandlerBridge(CycloPropertyManagerPageNative owner)
        {
            _owner = owner;
        }

        public void OnClose(int reason)
        {
            _owner.OnPageClosed(reason);
        }

        public void OnNumberboxChanged(int id, double value)
        {
            _owner.OnControlChanged(id);
        }

        public void OnCheckboxCheck(int id, bool state)
        {
            _owner.OnControlChanged(id);
        }

        public void OnOptionCheck(int id)
        {
            _owner.OnControlChanged(id);
        }

        public bool OnSubmitSelection(int id, object selection, int selType, ref string itemText)
        {
            return true;
        }

        public void AfterClose()
        {
        }
    }
}
