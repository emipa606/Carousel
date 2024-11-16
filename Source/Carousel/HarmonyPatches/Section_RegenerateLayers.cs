using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace Carousel;

[HotSwappable]
[HarmonyPatch]
internal static class Section_RegenerateLayers
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Section), nameof(Section.RegenerateAllLayers));
        yield return AccessTools.Method(typeof(Section), nameof(Section.RegenerateDirtyLayers));
    }

    private static void Postfix(Section __instance)
    {
        var comp = __instance.map.CarouselComp();

        if (comp.progress < CarouselMapComp.CameraUpdateTime &&
            comp.CameraRect().Contains(new IntVec3(__instance.botLeft.x / 17, 0, __instance.botLeft.z / 17)))
        {
            return;
        }

        comp.UpdateSection(__instance);
    }
}