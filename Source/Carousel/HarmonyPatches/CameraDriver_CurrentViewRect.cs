using System.Linq;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Carousel.HarmonyPatches;

[HarmonyPatch(typeof(CameraDriver), nameof(CameraDriver.CurrentViewRect), MethodType.Getter)]
internal static class CameraDriver_CurrentViewRect
{
    private static int lastViewRectGetFrame = -1;
    private static CellRect lastViewRect;

    private static void Postfix(ref CellRect __result)
    {
        if (Find.CurrentMap == null)
        {
            return;
        }

        if (Time.frameCount != lastViewRectGetFrame)
        {
            var center = __result.CenterVector3;
            var corners = __result.Corners.Select(c =>
                (c.ToVector3Shifted() - center).RotatedBy(-Find.CurrentMap.CarouselComp().current) + center);
            var min = corners.Aggregate(Vector3.Min);
            var max = corners.Aggregate(Vector3.Max);

            lastViewRectGetFrame = Time.frameCount;
            lastViewRect = CellRect.FromLimits(FloorVec(min), CeilVec(max));
        }

        __result = lastViewRect;
    }

    private static IntVec3 FloorVec(Vector3 v)
    {
        return new IntVec3(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y), Mathf.FloorToInt(v.z));
    }

    private static IntVec3 CeilVec(Vector3 v)
    {
        return new IntVec3(Mathf.CeilToInt(v.x), Mathf.CeilToInt(v.y), Mathf.CeilToInt(v.z));
    }
}