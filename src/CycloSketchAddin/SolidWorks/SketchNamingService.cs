namespace CycloSketchAddin.SolidWorks;

using System.Collections.Generic;
using CycloSketchAddin.Core;

public sealed class SketchNamingService
{
    public const string CycloProfileA = "SK_CYCLO_DISC_PROFILE_A";
    public const string CycloProfileB180 = "SK_CYCLO_DISC_PROFILE_B_180DEG";
    public const string RingPinsReference = "SK_RING_PINS_REFERENCE";
    public const string CenterBoreA = "SK_CENTER_BORE_A";
    public const string CenterBoreB180 = "SK_CENTER_BORE_B_180DEG";
    public const string OutputHolesDiscA = "SK_OUTPUT_HOLES_DISC_A";
    public const string OutputHolesDiscB180 = "SK_OUTPUT_HOLES_DISC_B_180DEG";
    public const string OutputPinsDisc = "SK_OUTPUT_PINS_DISC";
    public const string ReferenceAxes = "SK_REFERENCE_AXES";

    public static SketchNames Resolve(CycloParams p)
    {
        return new SketchNames(
            ResolveOrDefault(p.CycloProfileASketchName, CycloProfileA),
            ResolveOrDefault(p.CycloProfileBSketchName, CycloProfileB180),
            ResolveOrDefault(p.RingPinsReferenceSketchName, RingPinsReference),
            ResolveOrDefault(p.CenterBoreASketchName, CenterBoreA),
            ResolveOrDefault(p.CenterBoreBSketchName, CenterBoreB180),
            ResolveOrDefault(p.OutputHolesDiscASketchName, OutputHolesDiscA),
            ResolveOrDefault(p.OutputHolesDiscBSketchName, OutputHolesDiscB180),
            ResolveOrDefault(p.OutputPinsDiscSketchName, OutputPinsDisc),
            ResolveOrDefault(p.ReferenceAxesSketchName, ReferenceAxes));
    }

    public static IReadOnlyList<string> BuildExpectedNamesInGenerationOrder(CycloParams p)
    {
        var names = Resolve(p);
        var generated = new List<string>(9)
        {
            names.CycloProfileA
        };

        if (p.DrawCenterHole)
            generated.Add(names.CenterBoreA);

        if (p.DrawAroundHoles)
            generated.Add(names.OutputHolesDiscA);

        generated.Add(names.CycloProfileB180);

        if (p.DrawCenterHole)
            generated.Add(names.CenterBoreB180);

        if (p.DrawAroundHoles)
            generated.Add(names.OutputHolesDiscB180);

        generated.Add(names.RingPinsReference);

        if (p.DrawOutputDiskPins)
            generated.Add(names.OutputPinsDisc);

        generated.Add(names.ReferenceAxes);
        return generated;
    }

    private static string ResolveOrDefault(string? customName, string fallbackName)
    {
        return string.IsNullOrWhiteSpace(customName)
            ? fallbackName
            : customName.Trim();
    }
}

public sealed class SketchNames
{
    public SketchNames(
        string cycloProfileA,
        string cycloProfileB180,
        string ringPinsReference,
        string centerBoreA,
        string centerBoreB180,
        string outputHolesDiscA,
        string outputHolesDiscB180,
        string outputPinsDisc,
        string referenceAxes)
    {
        CycloProfileA = cycloProfileA;
        CycloProfileB180 = cycloProfileB180;
        RingPinsReference = ringPinsReference;
        CenterBoreA = centerBoreA;
        CenterBoreB180 = centerBoreB180;
        OutputHolesDiscA = outputHolesDiscA;
        OutputHolesDiscB180 = outputHolesDiscB180;
        OutputPinsDisc = outputPinsDisc;
        ReferenceAxes = referenceAxes;
    }

    public string CycloProfileA { get; }
    public string CycloProfileB180 { get; }
    public string RingPinsReference { get; }
    public string CenterBoreA { get; }
    public string CenterBoreB180 { get; }
    public string OutputHolesDiscA { get; }
    public string OutputHolesDiscB180 { get; }
    public string OutputPinsDisc { get; }
    public string ReferenceAxes { get; }
}
