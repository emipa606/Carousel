using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Carousel.HarmonyPatches;

[HarmonyPatch]
internal static class OverlayDrawer_Render
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        foreach (var m in AccessTools.GetDeclaredMethods(typeof(OverlayDrawer)))
        {
            if (m.Name.StartsWith("Render") && m.GetParameters().Length > 0 &&
                m.GetParameters()[0].ParameterType == typeof(Thing))
            {
                yield return m;
            }
        }
    }

    private static void Prefix([HarmonyArgument(0)] Thing t)
    {
        Graphics_DrawMesh.data = (Find.CurrentMap.CarouselComp().current, t.TrueCenter());
    }

    private static void Postfix()
    {
        Graphics_DrawMesh.data = null;
    }
}