using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Carousel.HarmonyPatches;

[HotSwappable]
[HarmonyPatch(typeof(Graphic), nameof(Graphic.Print))]
internal static class Graphic_Single_Multi_Print
{
    public static readonly Dictionary<Graphic_Multi, int> graphicToInt = new Dictionary<Graphic_Multi, int>();
    public static readonly List<Graphic_Multi> intToGraphic = [];

    private static void Prefix(Graphic __instance, Thing thing, ref int __state)
    {
        if (Printer_Plane_PrintPlane.currentThingCenter == null &&
            Graphic_Print.ShouldRotateVertices(__instance, thing))
        {
            Printer_Plane_PrintPlane.currentThingCenter = thing.TrueCenter();
            __state |= 1;
        }

        if (Printer_Plane_PrintPlane.currentThingData != null ||
            !Graphic_Print.ShouldExchangeVertices(__instance))
        {
            return;
        }

        var multi = (Graphic_Multi)__instance;
        if (!graphicToInt.TryGetValue(multi, out var id))
        {
            id = intToGraphic.Count;
            graphicToInt[multi] = id;
            intToGraphic.Add(multi);
        }

        Printer_Plane_PrintPlane.currentThingData = new Vector3(
            Printer_Plane_PrintPlane.SPECIAL_X,
            id,
            ((__instance.WestFlipped ? 2 : 0) + (__instance.EastFlipped ? 1 : 0)) | (thing.Rotation.AsInt << 2)
        );

        __state |= 2;
    }

    private static void Postfix(int __state)
    {
        if ((__state & 1) == 1)
        {
            Printer_Plane_PrintPlane.currentThingCenter = null;
        }

        if ((__state & 2) == 2)
        {
            Printer_Plane_PrintPlane.currentThingData = null;
        }
    }
}