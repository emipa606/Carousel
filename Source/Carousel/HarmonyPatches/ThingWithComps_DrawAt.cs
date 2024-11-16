using HarmonyLib;
using RimWorld;
using Verse;

namespace Carousel.HarmonyPatches;

[HotSwappable]
[HarmonyPatch(typeof(ThingWithComps), nameof(ThingWithComps.DrawAt))]
internal static class ThingWithComps_DrawAt
{
    private static void Prefix(ThingWithComps __instance)
    {
        if (__instance is Corpse or Pawn)
        {
            return;
        }

        Graphics_DrawMesh.data = (__instance.Map.CarouselComp().current, __instance.TrueCenter());
    }

    private static void Postfix()
    {
        Graphics_DrawMesh.data = null;
    }
}