using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Carousel.HarmonyPatches;

[HotSwappable]
[HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.RenderPawnAt))]
internal static class PawnRenderer_RenderPawnAt
{
    private static void Prefix(PawnRenderer __instance, Vector3 drawLoc, ref bool __state)
    {
        var pawn = __instance.pawn;
        if (pawn.Map == null || pawn.GetPosture() != PawnPosture.Standing || pawn.Downed)
        {
            return;
        }

        if (Graphics_DrawMesh.data != null)
        {
            return;
        }

        Graphics_DrawMesh.data = (pawn.Map.CarouselComp().current, drawLoc);
        __state = true;
    }

    private static void Postfix(bool __state)
    {
        if (__state)
        {
            Graphics_DrawMesh.data = null;
        }
    }
}