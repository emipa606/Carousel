using HarmonyLib;
using UnityEngine;
using Verse;

namespace Carousel;

[StaticConstructorOnStartup]
internal static class Carousel
{
    public static readonly KeyBindingDef RotateKey = KeyBindingDef.Named("CarouselRotate");

    static Carousel()
    {
        // Make SunShadowFade != SunShadow which isn't the case in vanilla
        AccessTools.Field(typeof(MatBases), nameof(MatBases.SunShadowFade))
            .SetValue(null, new Material(MatBases.SunShadowFade));

        CarouselMod.harmony.PatchAll();
    }
}