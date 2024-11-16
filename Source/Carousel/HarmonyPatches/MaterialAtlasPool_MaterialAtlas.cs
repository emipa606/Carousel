using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Carousel.HarmonyPatches;

internal static class MaterialAtlasPool_MaterialAtlas
{
    public static readonly Dictionary<Material, Material> linkedCornerMats = new Dictionary<Material, Material>();
    public static readonly HashSet<Material> linkedCornerMatsSet = [];

    public static void Postfix(MaterialAtlasPool.MaterialAtlas __instance)
    {
        foreach (var mat in __instance.subMats)
        {
            linkedCornerMatsSet.Add(linkedCornerMats[mat] = new Material(mat));
        }
    }
}