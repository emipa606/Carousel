using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Carousel.HarmonyPatches;

[HotSwappable]
[HarmonyPatch(typeof(GameConditionManager), nameof(GameConditionManager.DoConditionsUI))]
internal static class GameConditionManager_DoConditionsUI
{
    private static void Prefix(GameConditionManager __instance, Rect rect)
    {
        if (__instance != Find.CurrentMap?.gameConditionManager || CarouselMod.settings.disableCompass)
        {
            return;
        }

        var comp = Find.CurrentMap.CarouselComp();
        var center = new Vector2(UI.screenWidth - 10f - 32f, rect.yMax - 10f - 32f);
        Widgets.DrawTextureRotated(center, CompassWidget.CompassTex, -comp.current - 90f);

        var btnRect = new Rect(center.x - 32f, center.y - 32f, 64f, 64f);

        TooltipHandler.TipRegion(
            btnRect,
            () => $"{"CompassTip".Translate()}\n\n{Carousel.RotateKey.LabelCap}: {Carousel.RotateKey.MainKeyLabel}",
            5799998
        );

        if (Widgets.ButtonInvisible(btnRect))
        {
            comp.RotateBy(-comp.current);
        }
    }
}