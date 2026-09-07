using System;
using System.Collections.Generic;
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

    public IReadOnlyList<string> Generate(CycloParams p)
    {
        var sketchMgr = _model.SketchManager;
        var names = SketchNamingService.Resolve(p);
        var generated = new List<string>(9);
        const double phaseA = 0.0;
        double phaseB = Math.PI;

        // A-group first in the feature tree.
        CreateCycloProfileSketch(sketchMgr, p, names.CycloProfileA, +p.EccentricMm, phaseA);
        generated.Add(names.CycloProfileA);

        if (p.DrawCenterHole)
        {
            CreateCenterHoleSketch(sketchMgr, p, names.CenterBoreA, +p.EccentricMm);
            generated.Add(names.CenterBoreA);
        }

        if (p.DrawAroundHoles)
        {
            CreateAroundHolesSketch(sketchMgr, p, names.OutputHolesDiscA, +p.EccentricMm, phaseA);
            generated.Add(names.OutputHolesDiscA);
        }

        // B-group second in the feature tree.
        CreateCycloProfileSketch(sketchMgr, p, names.CycloProfileB180, -p.EccentricMm, phaseB);
        generated.Add(names.CycloProfileB180);

        if (p.DrawCenterHole)
        {
            CreateCenterHoleSketch(sketchMgr, p, names.CenterBoreB180, -p.EccentricMm);
            generated.Add(names.CenterBoreB180);
        }

        if (p.DrawAroundHoles)
        {
            CreateAroundHolesSketch(sketchMgr, p, names.OutputHolesDiscB180, -p.EccentricMm, phaseB);
            generated.Add(names.OutputHolesDiscB180);
        }

        // Shared sketches after A/B groups.
        CreateRingPinsSketch(sketchMgr, p, names.RingPinsReference);
        generated.Add(names.RingPinsReference);

        if (p.DrawOutputDiskPins)
        {
            CreateOutputPinsSketch(sketchMgr, p, names.OutputPinsDisc);
            generated.Add(names.OutputPinsDisc);
        }

        CreateReferenceSketch(sketchMgr, names.ReferenceAxes);
        generated.Add(names.ReferenceAxes);

        _model.ClearSelection2(true);
        _model.GraphicsRedraw2();
        return generated;
    }

    private void CreateReferenceSketch(SketchManager sketchMgr, string sketchName)
    {
        StartSketch();
        sketchMgr.CreateCenterLine(-0.05, 0, 0, 0.05, 0, 0);
        sketchMgr.CreateCenterLine(0, -0.05, 0, 0, 0.05, 0);
        EndAndNameSketch(sketchName);
    }

    private void CreateCycloProfileSketch(SketchManager sketchMgr, CycloParams p, string sketchName, double centerOffsetMm, double phaseShiftRad)
    {
        StartSketch();

        var pts = CycloGeometry.BuildCycloidalParallelCurvePoints(p, centerOffsetMm, phaseShiftRad);
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

    private void CreateCenterHoleSketch(SketchManager sketchMgr, CycloParams p, string sketchName, double centerOffsetMm)
    {
        StartSketch();
        sketchMgr.CreateCircleByRadius(UnitConverter.MmToM(centerOffsetMm), 0, 0, UnitConverter.MmToM(p.CenterHoleDiaMm * 0.5));
        EndAndNameSketch(sketchName);
    }

    private void CreateAroundHolesSketch(SketchManager sketchMgr, CycloParams p, string sketchName, double centerOffsetMm, double phaseShiftRad)
    {
        StartSketch();

        double cx0 = UnitConverter.MmToM(centerOffsetMm);
        double posR = UnitConverter.MmToM(p.AroundHolePositionDiaMm * 0.5);
        double holeR = UnitConverter.MmToM(p.AroundHoleDiaMm * 0.5);

        for (int i = 0; i < p.AroundHoleNum; i++)
        {
            double t = (2.0 * Math.PI * (i / (double)p.AroundHoleNum)) + phaseShiftRad;
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
