using HarmonyLib;
using RimWorld;
using UnityEngine;

namespace Carousel.HarmonyPatches;

[HarmonyPatch(typeof(Tornado), nameof(Tornado.DrawAt))]
internal static class Tornado_DrawAt
{
    private static void Prefix(Tornado __instance)
    {
        Graphics_DrawMesh.data = (__instance.Map.CarouselComp().current,
            new Vector3(__instance.realPosition.x, 0, __instance.realPosition.y));
    }

    private static void Postfix()
    {
        Graphics_DrawMesh.data = null;
    }
}