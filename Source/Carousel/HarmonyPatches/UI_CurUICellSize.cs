using System;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Carousel.HarmonyPatches;

[HarmonyPatch(typeof(UI), nameof(UI.CurUICellSize))]
internal static class UI_CurUICellSize
{
    private static void Postfix(ref float __result)
    {
        __result = Math.Abs((new Vector3(1f, 0f, 1f).MapToUIPosition() - new Vector3(0f, 0f, 0f).MapToUIPosition()).x);
    }
}