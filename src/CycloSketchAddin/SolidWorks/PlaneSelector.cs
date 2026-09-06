using SolidWorks.Interop.sldworks;

namespace CycloSketchAddin.SolidWorks;

public sealed class PlaneSelector
{
    private static readonly string[] CommonPlaneNames =
    {
        "Top Plane", "Top", "Plan de dessus", "Plan de Dessus",
        "Ebene oben", "Oben", "Plano superior", "Piano superiore",
        "Vlak boven", "Toppplan", "Topplan"
    };

    public bool TrySelectSketchPlane(ModelDoc2 model)
    {
        foreach (var name in CommonPlaneNames)
        {
            model.ClearSelection2(true);
            if (model.Extension.SelectByID2(name, "PLANE", 0, 0, 0, false, 0, null, 0))
                return true;
        }

        var feat = model.FirstFeature() as Feature;
        while (feat != null)
        {
            if (feat.GetTypeName2()?.ToLowerInvariant() == "refplane")
            {
                model.ClearSelection2(true);
                if (feat.Select2(false, 0)) return true;
            }
            feat = feat.GetNextFeature() as Feature;
        }

        return false;
    }
}
