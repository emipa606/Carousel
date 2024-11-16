using HarmonyLib;
using Verse;

namespace Carousel.HarmonyPatches;

[HarmonyPatch(typeof(LayerSubMesh), nameof(LayerSubMesh.FinalizeMesh))]
internal static class LayerSubMesh_FinalizeMesh
{
    private static void Prefix(LayerSubMesh __instance, ref MeshParts parts)
    {
        if (__instance.material == MatBases.SunShadowFade)
        {
            parts &= ~MeshParts.UVs;
        }
    }
}