using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Carousel.HarmonyPatches;

[HotSwappable]
[HarmonyPatch(typeof(Printer_Plane), nameof(Printer_Plane.PrintPlane))]
internal static class Printer_Plane_PrintPlane
{
    public const int SPECIAL_X = 9999;
    public const int EMPTY_X = 99999;
    public static Vector3? currentThingCenter;
    public static Vector3? currentThingData;

    public static readonly HashSet<Material> plantMats = [];
    public static readonly Vector3 EMPTY = new Vector3(EMPTY_X, 0, 0);

    private static void Prefix(ref Material mat, Vector2[] uvs)
    {
        var atlasTexture = mat.HasProperty("_MainTex") &&
                           GlobalTextureAtlasManager_BakeStaticAtlases.atlasTextures.TryGetValue(mat.mainTexture,
                               out var atlasGroup) &&
                           RightAtlasGroup(atlasGroup);

        if (uvs == Graphic_LinkedCornerFiller.CornerFillUVs)
        {
            mat = MaterialAtlasPool_MaterialAtlas.linkedCornerMats[mat];
        }

        if (Plant_Print.printing && !atlasTexture)
        {
            plantMats.Add(mat);
        }

        var toAdd = currentThingCenter ?? (atlasTexture ? currentThingData ?? EMPTY : null);

        if (toAdd != null)
        {
            SectionLayer_FinalizeMesh.dataBuffers.GetOrAdd(mat, _ => []).Add(toAdd.Value);
        }
    }

    public static bool RightAtlasGroup(TextureAtlasGroup atlasGroup)
    {
        return atlasGroup == TextureAtlasGroup.Plant ||
               atlasGroup == TextureAtlasGroup.Building ||
               atlasGroup == TextureAtlasGroup.Item;
    }
}