using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Carousel.HarmonyPatches;

[HarmonyPatch(typeof(GlobalTextureAtlasManager), nameof(GlobalTextureAtlasManager.BakeStaticAtlases))]
internal static class GlobalTextureAtlasManager_BakeStaticAtlases
{
    public static readonly Dictionary<Texture, TextureAtlasGroup> atlasTextures =
        new Dictionary<Texture, TextureAtlasGroup>();

    private static void Postfix()
    {
        atlasTextures.Clear();
        foreach (var atlas in GlobalTextureAtlasManager.staticTextureAtlases)
        {
            atlasTextures[atlas.ColorTexture] = atlas.groupKey.group;
        }
    }
}