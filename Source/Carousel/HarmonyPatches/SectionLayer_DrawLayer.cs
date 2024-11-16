using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Carousel.HarmonyPatches;

[HotSwappable]
[HarmonyPatch(typeof(SectionLayer), nameof(SectionLayer.DrawLayer))]
internal static class SectionLayer_DrawLayer
{
    private static readonly MethodBase MatrixIdentity =
        AccessTools.PropertyGetter(typeof(Matrix4x4), nameof(Matrix4x4.identity));

    private static readonly FieldInfo SubMeshMat =
        AccessTools.Field(typeof(LayerSubMesh), nameof(LayerSubMesh.material));

    private static readonly MethodBase OffsetMethod =
        AccessTools.Method(typeof(SectionLayer_DrawLayer), nameof(AddOffset));

    private static readonly MethodBase TransformMaterialMethod =
        AccessTools.Method(typeof(SectionLayer_DrawLayer), nameof(TransformMaterial));

    // Atlas material => (Linked flags, Original material)
    public static readonly Dictionary<Material, (int, Material)> linkedToSingle =
        new Dictionary<Material, (int, Material)>();

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts)
    {
        foreach (var inst in insts)
        {
            yield return inst;

            if (inst.operand == MatrixIdentity)
            {
                yield return new CodeInstruction(OpCodes.Ldloc_2);
                yield return new CodeInstruction(OpCodes.Ldfld, SubMeshMat);
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Call, OffsetMethod);
            }

            if (inst.operand != SubMeshMat)
            {
                continue;
            }

            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Call, TransformMaterialMethod);
        }
    }

    private static Matrix4x4 AddOffset(Matrix4x4 matrix, Material mat, SectionLayer layer)
    {
        if (MaterialAtlasPool_MaterialAtlas.linkedCornerMatsSet.Contains(mat))
        {
            matrix = Matrix4x4.Translate(
                new Vector3(0, 0, -Graphic_LinkedCornerFiller.ShiftUp) +
                new Vector3(0, 0, Graphic_LinkedCornerFiller.ShiftUp).RotatedBy(layer.Map.CarouselComp().current)
            );
        }

        return matrix;
    }

    public static Material TransformMaterial(Material mat, SectionLayer layer)
    {
        if (Graphic_Linked_Print.linkedMaterials.Contains(mat))
        {
            return RemapLinked(mat, Rot4.FromAngleFlat(layer.Map.CarouselComp().set));
        }

        return Graphic_Print.exchangeMats.TryGetValue(mat, out var data)
            ? data.Item1.mats[(data.Item2.AsInt + Rot4.FromAngleFlat(-layer.Map.CarouselComp().set).AsInt) % 4]
            : mat;
    }

    private static Material RemapLinked(Material mat, Rot4 cameraRot)
    {
        var single = linkedToSingle.GetOrAdd(mat, InitLinkedToSingle);
        var offset = cameraRot.AsInt;

        return MaterialAtlasPool.SubMaterialFromAtlas(
            single.Item2,
            (LinkDirections)(((single.Item1 >> offset) | (single.Item1 << (4 - offset))) & 0xf)
        );
    }

    private static (int, Material) InitLinkedToSingle(Material mat)
    {
        foreach (var kv in MaterialAtlasPool.atlasDict)
        {
            var index = -1;
            for (var i = 0; i < kv.Value.subMats.Length; i++)
            {
                if (kv.Value.subMats[i] == mat)
                {
                    index = i;
                }
            }

            if (index != -1)
            {
                return linkedToSingle[mat] = (index, kv.Key);
            }
        }

        throw new Exception($"Can't map linked material {mat}");
    }
}

// todo handle MinifiedThings

// Early patch

// Early patch