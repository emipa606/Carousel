using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Carousel.HarmonyPatches;

[HotSwappable]
[HarmonyPatch]
//[HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.RenderPawnAt))]
internal static class PawnRenderer_RenderPawnAt_Rotation
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(PawnRenderer), nameof(PawnRenderer.RenderPawnAt));
        yield return AccessTools.Method(typeof(PawnRenderer), nameof(PawnRenderer.ParallelPreRenderPawnAt));
    }

    private static void Prefix(PawnRenderer __instance, ref Rot4? rotOverride)
    {
        var pawn = __instance.pawn;
        if (pawn.Map == null || pawn.GetPosture() != PawnPosture.Standing || pawn.Downed)
        {
            return;
        }

        HandleRotation(pawn, ref rotOverride);
    }

    private static void HandleRotation(Pawn pawn, ref Rot4? rotOverride)
    {
        var comp = pawn.Map.CarouselComp();
        var camera = Rot4.FromAngleFlat(-comp.current).AsInt;

        // Conditions from Pawn_RotationTracker.UpdateRotation
        if (!pawn.Destroyed && !pawn.jobs.HandlingFacing && pawn.pather.Moving &&
            pawn.pather.curPath is { NodesLeftCount: >= 1 })
        {
            var movingRotation =
                FaceAdjacentCell(pawn.Position, pawn.pather.nextCell, Rot4.FromAngleFlat(-comp.current));
            if (movingRotation != null)
            {
                rotOverride = new Rot4(movingRotation.Value.AsInt);
                return;
            }
        }

        rotOverride = new Rot4((rotOverride?.AsInt ?? pawn.Rotation.AsInt) + camera);
    }

    private static Rot4? FaceAdjacentCell(IntVec3 pawn, IntVec3 c, Rot4 cameraRot)
    {
        if (c == pawn)
        {
            return null;
        }

        var diff = (c - pawn).RotatedBy(cameraRot);

        switch (diff.x)
        {
            case > 0:
                return Rot4.East;
            case < 0:
                return Rot4.West;
            default:
                return diff.z > 0 ? Rot4.North : Rot4.South;
        }
    }
}