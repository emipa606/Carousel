using HarmonyLib;
using UnityEngine;
using Verse;

namespace Carousel.HarmonyPatches;

[HotSwappable]
[HarmonyPatch(typeof(Printer_Shadow), nameof(Printer_Shadow.PrintShadow), typeof(SectionLayer), typeof(Vector3),
    typeof(Vector3), typeof(Rot4))]
internal static class Printer_Shadow_PrintShadow
{
    private static void Postfix(SectionLayer layer, Vector3 center)
    {
        var mesh = layer.GetSubMesh(MatBases.SunShadowFade);

        if (mesh.verts.Count > 0)
        {
            mesh.uvs.Add(center - Printer_Plane_PrintPlane.currentThingCenter ?? Vector3.zero);
        }
    }
}