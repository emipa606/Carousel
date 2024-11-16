using HarmonyLib;
using Verse;

namespace Carousel.HarmonyPatches;

[HarmonyPatch(typeof(SkyManager), nameof(SkyManager.UpdateOverlays))]
internal static class SkyManager_UpdateOverlays
{
    private static void Prefix()
    {
        MatBases.SunShadowFade.color = MatBases.SunShadow.color;
    }
}