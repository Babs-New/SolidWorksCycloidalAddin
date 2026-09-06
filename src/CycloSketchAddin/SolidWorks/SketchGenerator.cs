using System;
using CycloSketchAddin.Core;
using SolidWorks.Interop.sldworks;

namespace CycloSketchAddin.SolidWorks;

public sealed class SketchGenerator
{
    private readonly ModelDoc2 _model;
    private readonly PlaneSelector _planeSelector;

    public SketchGenerator(ModelDoc2 model, PlaneSelector planeSelector)
    {
        _model = model;
        _planeSelector = planeSelector;
    }

    public void Generate(CycloParams p)
    {
        var sketchMgr = _model.SketchManager;

        CreateReferenceSketch(sketchMgr, SketchNamingService.RefAxes);
        CreateCycloProfileSketch(sketchMgr, p, SketchNamingService.CycloProfile);

        if (p.DrawCenterHole)
            CreateCenterHoleSketch(sketchMgr, p, SketchNamingService.CenterBore);

        if (p.DrawAroundHoles)
            CreateAroundHolesSketch(sketchMgr, p, SketchNamingService.OutputHoles);

        CreateRingPinsSketch(sketchMgr, p, SketchNamingService.RingPins);

        if (p.DrawOutputDiskPins)
            CreateOutputPinsSketch(sketchMgr, p, SketchNamingService.OutputPins);

        _model.ClearSelection2(true);
        _model.GraphicsRedraw2();
    }

    private void CreateReferenceSketch(SketchManager sketchMgr, string sketchName)
    {
        StartSketch();
        sketchMgr.CreateCenterLine(-0.05, 0, 0, 0.05, 0, 0);
        sketchMgr.CreateCenterLine(0, -0.05, 0, 0, 0.05, 0);
        EndAndNameSketch(sketchName);
    }

    private void CreateCycloProfileSketch(SketchManager sketchMgr, CycloParams p, string sketchName)
    {
        StartSketch();

        var pts = CycloGeometry.BuildCycloidalParallelCurvePoints(p);
        var coords = new double[(pts.Count) * 3];
        for (int i = 0; i < pts.Count; i++)
        {
            coords[i * 3] = UnitConverter.MmToM(pts[i].X);
            coords[i * 3 + 1] = UnitConverter.MmToM(pts[i].Y);
            coords[i * 3 + 2] = 0.0;
        }

        sketchMgr.CreateSpline(coords);
        EndAndNameSketch(sketchName);
    }

    private void CreateCenterHoleSketch(SketchManager sketchMgr, CycloParams p, string sketchName)
    {
        StartSketch();
        sketchMgr.CreateCircleByRadius(UnitConverter.MmToM(p.EccentricMm), 0, 0, UnitConverter.MmToM(p.CenterHoleDiaMm * 0.5));
        EndAndNameSketch(sketchName);
    }

    private void CreateAroundHolesSketch(SketchManager sketchMgr, CycloParams p, string sketchName)
    {
        StartSketch();

        double cx0 = UnitConverter.MmToM(p.EccentricMm);
        double posR = UnitConverter.MmToM(p.AroundHolePositionDiaMm * 0.5);
        double holeR = UnitConverter.MmToM(p.AroundHoleDiaMm * 0.5);

        for (int i = 0; i < p.AroundHoleNum; i++)
        {
            double t = 2.0 * Math.PI * (i / (double)p.AroundHoleNum);
            sketchMgr.CreateCircleByRadius(cx0 + posR * Math.Cos(t), posR * Math.Sin(t), 0, holeR);
        }

        EndAndNameSketch(sketchName);
    }

    private void CreateRingPinsSketch(SketchManager sketchMgr, CycloParams p, string sketchName)
    {
        StartSketch();

        var centers = CycloGeometry.BuildRingPinCenters(p);
        double r = UnitConverter.MmToM(p.RingPinDiaMm * 0.5);
        foreach (var c in centers)
            sketchMgr.CreateCircleByRadius(UnitConverter.MmToM(c.X), UnitConverter.MmToM(c.Y), 0, r);

        EndAndNameSketch(sketchName);
    }

    private void CreateOutputPinsSketch(SketchManager sketchMgr, CycloParams p, string sketchName)
    {
        StartSketch();

        int n = p.OutputPinNum;
        double dia = p.OutputPinDiaMm;
        double posDia = p.OutputPinPositionDiaMm;

        if (p.LinkOutputPinsToAroundHoles)
        {
            n = p.AroundHoleNum;
            dia = p.AroundHoleDiaMm - (2.0 * p.EccentricMm);
            posDia = p.AroundHolePositionDiaMm;
        }

        double posR = UnitConverter.MmToM(posDia * 0.5);
        double pinR = UnitConverter.MmToM(dia * 0.5);

        for (int i = 0; i < n; i++)
        {
            double t = 2.0 * Math.PI * (i / (double)n);
            sketchMgr.CreateCircleByRadius(posR * Math.Cos(t), posR * Math.Sin(t), 0, pinR);
        }

        EndAndNameSketch(sketchName);
    }

    private void StartSketch()
    {
        if (!_planeSelector.TrySelectSketchPlane(_model))
            throw new InvalidOperationException("No sketch plane could be selected.");

        _model.SketchManager.InsertSketch(true);
    }

    private void EndAndNameSketch(string name)
    {
        _model.SketchManager.InsertSketch(true);

        var feat = _model.ISelectionManager.GetSelectedObject6(1, -1) as Feature;
        if (feat != null)
            feat.Name = name;

        _model.ClearSelection2(true);
    }
}
