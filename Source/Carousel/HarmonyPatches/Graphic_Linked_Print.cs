using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Carousel.HarmonyPatches;

[HarmonyPatch(typeof(Graphic_Linked), nameof(Graphic_Linked.Print))]
internal static class Graphic_Linked_Print
{
    public static readonly HashSet<Material> linkedMaterials = [];

    private static void Prefix(Graphic_Linked __instance, Thing thing)
    {
        linkedMaterials.Add(__instance.LinkedDrawMatFrom(thing, thing.Position));
    }
}