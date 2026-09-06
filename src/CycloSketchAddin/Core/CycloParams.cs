namespace CycloSketchAddin.Core;

public sealed class CycloParams
{
    public int ReductionRatio { get; set; } = 10;
    public double EccentricMm { get; set; } = 2.0;
    public double RingPinDiaMm { get; set; } = 10.0;
    public double RingPinPitchDiaMm { get; set; } = 80.0;
    public int PlotPerTooth { get; set; } = 40;

    public bool DrawCenterHole { get; set; } = true;
    public double CenterHoleDiaMm { get; set; } = 16.0;

    public bool DrawAroundHoles { get; set; } = true;
    public int AroundHoleNum { get; set; } = 8;
    public double AroundHoleDiaMm { get; set; } = 12.0;
    public double AroundHolePositionDiaMm { get; set; } = 42.0;

    public bool DrawOutputDiskPins { get; set; } = true;
    public int OutputPinNum { get; set; } = 8;
    public double OutputPinDiaMm { get; set; } = 8.0;
    public double OutputPinPositionDiaMm { get; set; } = 42.0;

    public bool LinkOutputPinsToAroundHoles { get; set; } = true;

    public bool SeparateSketches { get; set; } = true;
}
