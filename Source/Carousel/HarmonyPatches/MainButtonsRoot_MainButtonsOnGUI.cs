using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Carousel.HarmonyPatches;

[HotSwappable]
[HarmonyPatch(typeof(MainButtonsRoot), nameof(MainButtonsRoot.MainButtonsOnGUI))]
public static class MainButtonsRoot_MainButtonsOnGUI
{
    private static void Prefix()
    {
        if (!Carousel.RotateKey.KeyDownEvent || Find.CurrentMap == null)
        {
            return;
        }

        Find.CurrentMap.CarouselComp().RotateBy(Event.current.shift ? -90f : 90f);
        Event.current.Use();
    }
}