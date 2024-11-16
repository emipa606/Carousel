using HarmonyLib;
using UnityEngine;
using Verse;

namespace Carousel.HarmonyPatches;

[HarmonyPatch(typeof(GenMapUI), nameof(GenMapUI.LabelDrawPosFor), typeof(Thing), typeof(float))]
internal static class GenMapUI_LabelDrawPosFor
{
    private static void Postfix(ref Vector2 __result, Thing thing, float worldOffsetZ)
    {
        var drawPos = thing.DrawPos;
        drawPos += new Vector3(0, 0, worldOffsetZ).RotatedBy(Rot4.FromAngleFlat(thing.Map.CarouselComp().current));
        Vector2 vector = Find.Camera.WorldToScreenPoint(drawPos) / Prefs.UIScale;
        vector.y = UI.screenHeight - vector.y;

        __result = vector;
    }
}