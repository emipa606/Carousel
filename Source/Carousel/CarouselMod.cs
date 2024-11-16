using Carousel.HarmonyPatches;
using HarmonyLib;
using Mlie;
using UnityEngine;
using Verse;

namespace Carousel;

public class CarouselMod : Mod
{
    public static readonly Harmony harmony = new Harmony("carousel");
    public static Settings settings;
    private static string currentVersion;

    public CarouselMod(ModContentPack content) : base(content)
    {
        settings = GetSettings<Settings>();
        currentVersion = VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);

        harmony.Patch(
            AccessTools.Constructor(typeof(MaterialAtlasPool.MaterialAtlas), [typeof(Material)]),
            postfix: new HarmonyMethod(typeof(MaterialAtlasPool_MaterialAtlas),
                nameof(MaterialAtlasPool_MaterialAtlas.Postfix))
        );

        harmony.Patch(
            AccessTools.Method(typeof(Graphic_Single), nameof(Graphic_Single.Init)),
            transpiler: new HarmonyMethod(typeof(Graphic_Init), nameof(Graphic_Init.Transpiler))
        );

        harmony.Patch(
            AccessTools.Method(typeof(Graphic_Multi), nameof(Graphic_Multi.Init)),
            transpiler: new HarmonyMethod(typeof(Graphic_Init), nameof(Graphic_Init.Transpiler))
        );
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        var listing = new Listing_Standard();
        listing.Begin(inRect);
        listing.ColumnWidth = 220f;

        listing.CheckboxLabeled("Car.DisableCompass".Translate(), ref settings.disableCompass);
        if (currentVersion != null)
        {
            listing.Gap();
            GUI.contentColor = Color.gray;
            listing.Label("Car.ModVersion".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        listing.End();
    }

    public override string SettingsCategory()
    {
        return "Carousel";
    }
}