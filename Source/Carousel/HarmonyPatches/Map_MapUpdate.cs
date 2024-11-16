using HarmonyLib;
using Verse;

namespace Carousel.HarmonyPatches;

[HarmonyPatch(typeof(Map), nameof(Map.MapUpdate))]
internal static class Map_MapUpdate
{
    private static void Prefix(Map __instance)
    {
        __instance.CarouselComp().Update();
    }
}