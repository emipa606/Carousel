using HarmonyLib;
using RimWorld;
using Verse;

namespace Carousel.HarmonyPatches;

[HarmonyPatch(typeof(GameConditionManager), nameof(GameConditionManager.TotalHeightAt))]
internal static class GameConditionManager_TotalHeightAt
{
    private static void Postfix(GameConditionManager __instance, ref float __result)
    {
        if (__instance != Find.CurrentMap?.gameConditionManager || CarouselMod.settings.disableCompass)
        {
            return;
        }

        __result += 84f;
    }
}