using System;

namespace CycloSketchAddin.Core;

public static class CycloValidator
{
    public static bool TryValidate(CycloParams p, out string error)
    {
        if (p.ReductionRatio < 2) return Fail("Reduction ratio must be >= 2.", out error);
        if (p.EccentricMm <= 0) return Fail("Eccentric amount must be > 0.", out error);
        if (p.RingPinDiaMm <= 0) return Fail("Ring pin diameter must be > 0.", out error);
        if (p.RingPinPitchDiaMm <= 0) return Fail("Ring pin pitch diameter must be > 0.", out error);
        if (p.PlotPerTooth < 5) return Fail("Cycloidal curve plot per tooth must be >= 5.", out error);

        if (p.EccentricMm * (p.ReductionRatio + 1) >= p.RingPinPitchDiaMm * 0.5)
            return Fail("Invalid geometry: eccentric * (ratio + 1) must be < ring pin pitch diameter / 2.", out error);

        if (p.DrawCenterHole && p.CenterHoleDiaMm <= 0)
            return Fail("Center hole diameter must be > 0.", out error);

        if (p.DrawAroundHoles)
        {
            if (p.AroundHoleNum < 1) return Fail("Around hole number must be >= 1.", out error);
            if (p.AroundHoleDiaMm <= 0) return Fail("Around hole diameter must be > 0.", out error);
            if (p.AroundHolePositionDiaMm <= 0) return Fail("Around hole position diameter must be > 0.", out error);
        }

        if (p.DrawOutputDiskPins)
        {
            if (p.LinkOutputPinsToAroundHoles)
            {
                if (!p.DrawAroundHoles) return Fail("Around holes must be enabled when pins are linked.", out error);
                if (p.AroundHoleDiaMm - 2.0 * p.EccentricMm <= 0)
                    return Fail("Linked output pin diameter <= 0. Need around hole dia > 2 * eccentric.", out error);
            }
            else
            {
                if (p.OutputPinNum < 1) return Fail("Output pin number must be >= 1.", out error);
                if (p.OutputPinDiaMm <= 0) return Fail("Output pin diameter must be > 0.", out error);
                if (p.OutputPinPositionDiaMm <= 0) return Fail("Output pin position diameter must be > 0.", out error);
            }
        }

        error = string.Empty;
        return true;
    }

    private static bool Fail(string message, out string error)
    {
        error = message;
        return false;
    }
}
