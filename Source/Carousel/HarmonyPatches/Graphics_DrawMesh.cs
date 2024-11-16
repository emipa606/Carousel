using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Carousel.HarmonyPatches;

[HotSwappable]
[HarmonyPatch]
internal static class Graphics_DrawMesh
{
    // Rotation angle and center
    public static (float, Vector3)? data;

    private static IEnumerable<MethodBase> TargetMethods()
    {
        foreach (var m in AccessTools.GetDeclaredMethods(typeof(Graphics)))
        {
            if (m.Name == "DrawMesh" && m.GetParameters().Length == 12 && m.GetParameters()[1].Name == "matrix")
            {
                yield return m;
            }
        }

        foreach (var m in AccessTools.GetDeclaredMethods(typeof(DrawBatch)))
        {
            if (m.Name == "DrawMesh")
            {
                yield return m;
            }
        }
    }

    private static void Prefix(ref Matrix4x4 matrix, Material material)
    {
        if (data == null || material == MatBases.SunShadowFade)
        {
            return;
        }

        var rotCenter = data.Value.Item2;
        var current = data.Value.Item1;

        var drawX = matrix.m03;
        var drawZ = matrix.m23;

        matrix.m03 = drawX - rotCenter.x;
        matrix.m23 = drawZ - rotCenter.z;

        matrix = Matrix4x4.Rotate(Quaternion.Euler(0, current, 0)) * matrix;

        matrix.m03 += rotCenter.x;
        matrix.m23 += rotCenter.z;
    }
}