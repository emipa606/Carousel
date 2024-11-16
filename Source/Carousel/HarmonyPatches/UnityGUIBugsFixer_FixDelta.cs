using HarmonyLib;
using Verse;

namespace Carousel.HarmonyPatches;

[HarmonyPatch(typeof(UnityGUIBugsFixer), nameof(UnityGUIBugsFixer.FixDelta))]
internal static class UnityGUIBugsFixer_FixDelta
{
    private static void Postfix()
    {
        if (Find.CurrentMap != null)
        {
            UnityGUIBugsFixer.currentEventDelta =
                UnityGUIBugsFixer.currentEventDelta.RotatedBy(Find.CurrentMap.CarouselComp().current);
        }
    }
}