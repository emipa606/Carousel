using HarmonyLib;
using RimWorld;
using Verse;

namespace Carousel.HarmonyPatches;

[HarmonyPatch(typeof(ThingSelectionUtility), nameof(ThingSelectionUtility.GetMapRect))]
internal static class ThingSelectionUtility_GetMapRect
{
    private static void Postfix(ref CellRect __result)
    {
        __result = CellRect.FromLimits(__result.minX, __result.minZ, __result.maxX, __result.maxZ);
    }
}