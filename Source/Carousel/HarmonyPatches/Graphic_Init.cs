using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace Carousel.HarmonyPatches;

internal static class Graphic_Init
{
    private static readonly MethodBase MatFrom = AccessTools.Method(typeof(MaterialPool), nameof(MaterialPool.MatFrom),
        [typeof(MaterialRequest)]);

    private static readonly MethodBase TransformRequestMethod =
        AccessTools.Method(typeof(Graphic_Init), nameof(TransformRequest));

    private static readonly Dictionary<(GraphicData, List<ShaderParameter>), List<ShaderParameter>> shaderParamMap =
        new Dictionary<(GraphicData, List<ShaderParameter>), List<ShaderParameter>>();

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts)
    {
        foreach (var inst in insts)
        {
            if (inst.operand == MatFrom)
            {
                yield return new CodeInstruction(OpCodes.Ldarg_1);
                yield return new CodeInstruction(OpCodes.Call, TransformRequestMethod);
            }

            yield return inst;
        }
    }

    private static MaterialRequest TransformRequest(MaterialRequest req, GraphicRequest graphicRequest)
    {
        req.shaderParameters = shaderParamMap.GetOrAdd(
            (graphicRequest.graphicData, req.shaderParameters),
            // Create a copy of the list so the MatRequest hashcode/equals is different and a new material
            // is created for every Graphic instance even when the material parameters are the same.
            // Materials are used to distinguish objects and have to be separated for different ones.
            k => k.Item2 == null ? [] : [.. k.Item2]
        );

        return req;
    }
}