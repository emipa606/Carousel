using HarmonyLib;
using UnityEngine;
using Verse;

namespace Carousel.HarmonyPatches;

[HarmonyPatch(typeof(CameraDriver), nameof(CameraDriver.CalculateCurInputDollyVect))]
internal static class CameraDriver_CalculateCurInputDollyVect
{
    private static void Postfix(ref Vector2 __result)
    {
        __result = __result.RotatedBy(-Find.CurrentMap.CarouselComp().current);
    }
}