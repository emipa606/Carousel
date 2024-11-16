using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Carousel;

[HotSwappable]
[HarmonyPatch(typeof(SectionLayer), nameof(SectionLayer.FinalizeMesh))]
internal static class SectionLayer_FinalizeMesh
{
    public static readonly Dictionary<Material, List<Vector3>> dataBuffers = new Dictionary<Material, List<Vector3>>();

    private static void Postfix(SectionLayer __instance)
    {
        if (!dataBuffers.Any(kv => kv.Value.Count > 0))
        {
            return;
        }

        foreach (var kv in dataBuffers)
        {
            var mesh = __instance.GetSubMesh(kv.Key);

            mesh.verts.InsertRange(mesh.verts.Count, kv.Value);
            NoAllocHelpers.ResizeList(kv.Value, 0);
        }
    }
}