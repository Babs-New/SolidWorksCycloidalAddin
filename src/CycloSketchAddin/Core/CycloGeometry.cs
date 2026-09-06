using System;
using System.Collections.Generic;
using System.Drawing;

namespace CycloSketchAddin.Core;

public static class CycloGeometry
{
    public static IReadOnlyList<PointF> BuildCycloidalParallelCurvePoints(CycloParams p)
    {
        int ringPinNum = p.ReductionRatio + 1;
        int troToothNum = ringPinNum - 1;
        int samples = Math.Max(100, p.ReductionRatio * p.PlotPerTooth);

        double eccentric = p.EccentricMm;
        double ringPinRadius = p.RingPinDiaMm * 0.5;
        double ringPinPitchRadius = p.RingPinPitchDiaMm * 0.5;

        double redRatio = troToothNum / (double)(ringPinNum - troToothNum);
        double rm = ringPinPitchRadius / (redRatio + 1.0);
        double rc = rm * redRatio;
        double rd = eccentric;
        double d = ringPinRadius;

        var result = new List<PointF>(samples + 1);
        for (int i = 0; i <= samples; i++)
        {
            double t = (2.0 * Math.PI * i) / samples;
            double x = Fxp(t, rc, rm, rd, d) + rd;
            double y = Fyp(t, rc, rm, rd, d);
            result.Add(new PointF((float)x, (float)y));
        }

        return result;
    }

    public static IReadOnlyList<PointF> BuildRingPinCenters(CycloParams p)
    {
        int ringPinNum = p.ReductionRatio + 1;
        double pr = p.RingPinPitchDiaMm * 0.5;
        var result = new List<PointF>(ringPinNum);

        for (int i = 0; i < ringPinNum; i++)
        {
            double t = 2.0 * Math.PI * (i / (double)ringPinNum);
            result.Add(new PointF((float)(pr * Math.Cos(t)), (float)(pr * Math.Sin(t))));
        }

        return result;
    }

    private static double Fxa(double p, double rc, double rm, double rd)
        => (rc + rm) * Math.Cos(p) - rd * Math.Cos(((rc + rm) / rm) * p);

    private static double Fya(double p, double rc, double rm, double rd)
        => (rc + rm) * Math.Sin(p) - rd * Math.Sin(((rc + rm) / rm) * p);

    private static double Dfxa(double p, double rc, double rm, double rd)
        => -(rc + rm) * Math.Sin(p) + ((rc + rm) / rm) * rd * Math.Sin(((rc + rm) / rm) * p);

    private static double Dfya(double p, double rc, double rm, double rd)
        => (rc + rm) * Math.Cos(p) - ((rc + rm) / rm) * rd * Math.Cos(((rc + rm) / rm) * p);

    private static double Fxp(double p, double rc, double rm, double rd, double d)
    {
        double dxa = Dfxa(p, rc, rm, rd);
        double dya = Dfya(p, rc, rm, rd);
        double den = Math.Sqrt(dxa * dxa + dya * dya);
        return Fxa(p, rc, rm, rd) - d * dya / den;
    }

    private static double Fyp(double p, double rc, double rm, double rd, double d)
    {
        double dxa = Dfxa(p, rc, rm, rd);
        double dya = Dfya(p, rc, rm, rd);
        double den = Math.Sqrt(dxa * dxa + dya * dya);
        return Fya(p, rc, rm, rd) + d * dxa / den;
    }
}
