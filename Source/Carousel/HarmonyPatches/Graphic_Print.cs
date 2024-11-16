using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Carousel.HarmonyPatches;

[HotSwappable]
[HarmonyPatch(typeof(Graphic), nameof(Graphic.Print))]
internal static class Graphic_Print
{
    public static readonly Dictionary<Material, (Graphic_Multi, Rot4)> exchangeMats =
        new Dictionary<Material, (Graphic_Multi, Rot4)>();

    public static readonly Dictionary<Graphic, Material> copiedForFlip = new Dictionary<Graphic, Material>();

    public static readonly HashSet<Material> graphicSingle = [];

    private static readonly MethodBase MatAt = AccessTools.Method(typeof(Graphic), nameof(Graphic.MatAt));

    private static readonly MethodBase CacheMaterialMethod =
        AccessTools.Method(typeof(Graphic_Print), nameof(CacheMaterial));

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts)
    {
        foreach (var inst in insts)
        {
            yield return inst;

            if (inst.operand != MatAt)
            {
                continue;
            }

            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Call, CacheMaterialMethod);
        }
    }

    private static Material CacheMaterial(Material mat, Graphic graphic, Thing t)
    {
        if (ShouldExchangeVertices(graphic))
        {
            var outMat = mat;
            var rot = t.Rotation;

            if (rot == Rot4.East && graphic.EastFlipped || rot == Rot4.West && graphic.WestFlipped)
            {
                if (!copiedForFlip.TryGetValue(graphic, out outMat))
                {
                    copiedForFlip[graphic] = outMat = new Material(mat);

                    if (MaterialPool.matDictionaryReverse.TryGetValue(mat, out var matreq))
                    {
                        MaterialPool.matDictionaryReverse[outMat] = matreq;
                    }
                }
            }

            exchangeMats[outMat] = ((Graphic_Multi)graphic, rot);

            return outMat;
        }

        if (ShouldRotateVertices(graphic, t))
        {
            graphicSingle.Add(mat);
        }

        return mat;
    }

    public static bool ShouldExchangeVertices(Graphic graphic)
    {
        // For these Graphics, not an actual rotation but a texture switch is used for rotating
        return graphic.GetType() == typeof(Graphic_Multi) && !graphic.ShouldDrawRotated;
    }

    public static bool ShouldRotateVertices(Graphic graphic, Thing t)
    {
        // If something has a single texture but rotates anyway it means it looks good from every side and
        // doesn't have to rotate with the camera
        return graphic.GetType() == typeof(Graphic_Single) && (!graphic.ShouldDrawRotated || !t.def.rotatable) ||
               graphic.GetType() == typeof(Graphic_Multi) && graphic.ShouldDrawRotated ||
               graphic.GetType() == typeof(Graphic_StackCount) ||
               t.def.category == ThingCategory.Item;
    }
}