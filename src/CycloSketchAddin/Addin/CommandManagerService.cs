using System;
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

}
