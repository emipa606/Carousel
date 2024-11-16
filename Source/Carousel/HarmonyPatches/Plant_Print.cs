using Carousel.HarmonyPatches;
using HarmonyLib;
using RimWorld;

namespace Carousel;

[HotSwappable]
[HarmonyPatch(typeof(Plant), nameof(Plant.Print))]
internal static class Plant_Print
{
    public static bool printing;

    private static void Prefix(Plant __instance)
    {
        printing = true;
        Printer_Plane_PrintPlane.currentThingCenter = __instance.TrueCenter();
    }

    private static void Postfix()
    {
        Printer_Plane_PrintPlane.currentThingCenter = null;
        printing = false;
    }
}