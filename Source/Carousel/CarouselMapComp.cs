using System;
using System.Diagnostics;
using Carousel.HarmonyPatches;
using RimWorld;
using UnityEngine;
using Verse;

namespace Carousel;

[HotSwappable]
public class CarouselMapComp(Map map) : MapComponent(map)
{
    public const int AnimTime = 8;
    public const int CameraUpdateTime = 5;

    private static Vector3[] tempUVs = [];
    private static Vector3[] tempVerts = [];
    private IntVec3 cameraPos;
    public float current;
    public int progress;

    private int sectionsDone;
    public float set;
    public float start;
    public float target;
    private int workerIndex;

    public void RotateBy(float by)
    {
        current = target;
        start = current;
        progress = 0;
        sectionsDone = 0;
        workerIndex = 0;
        cameraPos = Find.CameraDriver.MapPosition;

        target += by;
        target = GenMath.PositiveMod(target, 360f);

        //Log.Message($"{Time.frameCount} {target} {current}");
    }

    public static bool HasFlag(ulong flags, MapMeshFlagDef flagDef)
    {
        return (flags & (ulong)flagDef) != 0;
    }

    public void Update()
    {
        var dist = Mathf.DeltaAngle(start, target);
        current = GenMath.PositiveMod(start + (progress * progress * dist / (AnimTime * AnimTime)), 360f);

        if (current != target)
        {
            AnimationStep();
        }

        if (Find.CurrentMap == map)
        {
            Find.Camera.transform.rotation = Quaternion.Euler(90, current, 0);
        }
    }

    private void AnimationStep()
    {
        _ = Stopwatch.StartNew();
        var drawer = map.mapDrawer;
        var mapRect = MapRect();
        var cameraRect = CameraRect();

        var c = Math.Max(drawer.sections.Length - cameraRect.Area, 0);
        var step = Mathf.CeilToInt((float)c / (AnimTime - 1));
        var goal = Math.Min(c, sectionsDone + step);

        if (progress == CameraUpdateTime)
        {
            foreach (var cell in cameraRect)
            {
                if (mapRect.Contains(cell))
                {
                    UpdateSection(drawer.sections[cell.x, cell.z]);
                }
            }

            set = target;
        }
        else
        {
            while (sectionsDone < goal)
            {
                var cell = new IntVec3(workerIndex % mapRect.Width, 0, workerIndex / mapRect.Height);

                if (!cameraRect.Contains(cell))
                {
                    UpdateSection(drawer.sections[cell.x, cell.z]);
                    sectionsDone++;
                }

                workerIndex++;
            }
        }

        progress++;

        //Log.Message($"{current} {start} {Mathf.DeltaAngle(start, target)} {progress} {sectionsDone} {workerIndex} {step} {c} {goal} {watch.ElapsedMilliseconds}");
    }

    public CellRect MapRect()
    {
        var drawer = map.mapDrawer;
        return new CellRect(0, 0, drawer.SectionCount.x, drawer.SectionCount.z);
    }

    public CellRect CameraRect()
    {
        var cameraSection = map.mapDrawer.SectionCoordsAt(cameraPos);
        return new CellRect(cameraSection.x - 2, cameraSection.z - 1, 4, 3).Inside(MapRect());
    }

    public void UpdateSection(Section section)
    {
        foreach (var layer in section.layers)
        {
            if (!HasFlag(layer.relevantChangeTypes, MapMeshFlagDefOf.Buildings) &&
                !HasFlag(layer.relevantChangeTypes, MapMeshFlagDefOf.Things))
            {
                continue;
            }

            foreach (var mesh in layer.subMeshes)
            {
                if (mesh.verts.Count == 0)
                {
                    continue;
                }

                if (mesh.material.HasProperty("_MainTex") &&
                    GlobalTextureAtlasManager_BakeStaticAtlases.atlasTextures.TryGetValue(mesh.material.mainTexture,
                        out var group) &&
                    Printer_Plane_PrintPlane.RightAtlasGroup(group))
                {
                    TransformAtlas(mesh, group);
                    continue;
                }

                TransformVerts(mesh);
                TransformUVs(mesh);

                if (mesh.material == MatBases.SunShadowFade)
                {
                    TransformShadows(mesh);
                }
            }
        }
    }

    private void TransformUVs(LayerSubMesh mesh)
    {
        // Fix texture flip
        if (!Graphic_Print.exchangeMats.TryGetValue(mesh.material, out var matData))
        {
            return;
        }

        var uvsc = mesh.uvs.Count;
        var graphic = matData.Item1;

        var relRot = GenMath.PositiveMod(matData.Item2.AsInt - Rot4.FromAngleFlat(target).AsInt, 4);
        var flipped = relRot == 1 && graphic.EastFlipped || relRot == 3 && graphic.WestFlipped ? 1 : 0;

        Util.ResizeIfNeeded(ref tempUVs, uvsc);

        for (var i = 0; i < uvsc; i += 4)
        {
            FixUVs(tempUVs, Printer_Plane.defaultUvs, i, flipped);
        }

        mesh.mesh.SetUVs(tempUVs, uvsc);
    }

    private void TransformVerts(LayerSubMesh mesh)
    {
        var offset = Rot4.FromAngleFlat(target);
        var offseti = offset.AsInt;

        // Rotate around a center
        int vertsc;
        Vector3[] vertsArr;
        if (Printer_Plane_PrintPlane.plantMats.Contains(mesh.material) ||
            Graphic_Print.graphicSingle.Contains(mesh.material))
        {
            vertsc = mesh.verts.Count / 5 * 4;
            vertsArr = NoAllocHelpers.ExtractArrayFromListT(mesh.verts);

            Util.ResizeIfNeeded(ref tempVerts, vertsc);

            for (var i = 0; i < vertsc; i += 4)
            {
                // The mesh data lists are only used during mesh building and are otherwise unused.
                // In between mesh rebuilding, Carousel reuses the lists to recalculate the meshes
                // but also appends additional information to the end of the vertex list
                var center = vertsArr[vertsc + (i / 4)];

                RotateVerts(tempVerts, vertsArr, i, center, offset);
            }

            mesh.mesh.SetVertices(tempVerts, vertsc);
        }

        // Exchange vertices
        // This doesn't change the set of their values but changes their order
        if (!Graphic_Print.exchangeMats.ContainsKey(mesh.material) &&
            !Graphic_Linked_Print.linkedMaterials.Contains(mesh.material))
        {
            return;
        }

        vertsc = mesh.verts.Count;
        vertsArr = NoAllocHelpers.ExtractArrayFromListT(mesh.verts);

        Util.ResizeIfNeeded(ref tempVerts, vertsc);

        for (var i = 0; i < vertsc; i += 4)
        {
            ExchangeVerts(tempVerts, vertsArr, i, offseti);
        }

        mesh.mesh.SetVertices(tempVerts, vertsc);
    }

    private void TransformAtlas(LayerSubMesh mesh, TextureAtlasGroup group)
    {
        var offset = Rot4.FromAngleFlat(target);
        var offseti = offset.AsInt;
        var vertsc = mesh.verts.Count / 5 * 4;
        var uvsc = mesh.uvs.Count;
        var vertsArr = NoAllocHelpers.ExtractArrayFromListT(mesh.verts);
        var uvsArr = NoAllocHelpers.ExtractArrayFromListT(mesh.uvs);

        Util.ResizeIfNeeded(ref tempVerts, vertsc);
        Util.ResizeIfNeeded(ref tempUVs, uvsc);

        for (var i = 0; i < vertsc; i += 4)
        {
            var data = vertsArr[vertsc + (i / 4)];

            if (data.x == Printer_Plane_PrintPlane.SPECIAL_X)
            {
                ExchangeVerts(tempVerts, vertsArr, i, offseti);

                var rotData = ((int)data.z & 0b1100) >> 2;
                var flipData = (int)data.z & 0b0011;

                var relRot = GenMath.PositiveMod(rotData - Rot4.FromAngleFlat(target).AsInt, 4);
                var flipped = relRot == 1 && (flipData & 1) == 1 || relRot == 3 && (flipData & 2) == 2 ? 1 : 0;

                var rotatedMat = Graphic_Single_Multi_Print.intToGraphic[(int)data.y]
                    .mats[(rotData + Rot4.FromAngleFlat(-target).AsInt) % 4];
                Graphic.TryGetTextureAtlasReplacementInfo(rotatedMat, group, false, false, out _, out var uvs, out _);

                FixUVs(
                    tempUVs,
                    uvs,
                    i,
                    flipped
                );
            }
            else if (data.x != Printer_Plane_PrintPlane.EMPTY_X)
            {
                RotateVerts(tempVerts, vertsArr, i, data, offset);
                Array.Copy(uvsArr, i, tempUVs, i, 4);
            }
            else
            {
                Array.Copy(vertsArr, i, tempVerts, i, 4);
                Array.Copy(uvsArr, i, tempUVs, i, 4);
            }
        }

        mesh.mesh.SetVertices(tempVerts, vertsc);
        mesh.mesh.SetUVs(tempUVs, uvsc);
    }

    private void TransformShadows(LayerSubMesh mesh)
    {
        if (mesh.uvs.Count == 0)
        {
            return;
        }

        var rot = Rot4.FromAngleFlat(target);
        var uvs = mesh.uvs;
        var vertsc = mesh.verts.Count;
        var origVerts = NoAllocHelpers.ExtractArrayFromListT(mesh.verts);

        Util.ResizeIfNeeded(ref tempVerts, vertsc);

        for (var i = 0; i <= vertsc - 5; i += 5)
        {
            var offset = uvs[i / 10];
            if (offset == Vector3.zero)
            {
                Array.Copy(origVerts, i, tempVerts, i, 5);
                continue;
            }

            var r = offset.RotatedBy(rot) - offset;

            tempVerts[i] = origVerts[i] + r;
            tempVerts[i + 1] = origVerts[i + 1] + r;
            tempVerts[i + 2] = origVerts[i + 2] + r;
            tempVerts[i + 3] = origVerts[i + 3] + r;
            tempVerts[i + 4] = origVerts[i + 4] + r;
        }

        mesh.mesh.SetVertices(tempVerts, vertsc);
    }

    private static void FixUVs(Vector3[] uVs, Vector2[] origUvs, int i, int flipped)
    {
        uVs[i + ((0 + (3 * flipped)) & 3)] = origUvs[0];
        uVs[i + ((1 + (1 * flipped)) & 3)] = origUvs[1];
        uVs[i + ((2 + (3 * flipped)) & 3)] = origUvs[2];
        uVs[i + ((3 + (1 * flipped)) & 3)] = origUvs[3];
    }

    private static void ExchangeVerts(Vector3[] vector3s, Vector3[] vertsArr, int i, int offseti)
    {
        vector3s[i].SetXZY(ref vertsArr[i + (offseti & 3)], vertsArr[i].y);
        vector3s[i + 1].SetXZY(ref vertsArr[i + ((offseti + 1) & 3)], vertsArr[i + 1].y);
        vector3s[i + 2].SetXZY(ref vertsArr[i + ((offseti + 2) & 3)], vertsArr[i + 2].y);
        vector3s[i + 3].SetXZY(ref vertsArr[i + ((offseti + 3) & 3)], vertsArr[i + 3].y);
    }

    private static void RotateVerts(Vector3[] vector3s, Vector3[] vertsArr, int i, Vector3 c, Rot4 offset)
    {
        vector3s[i] = c + (vertsArr[i] - c).RotatedBy(offset);
        vector3s[i + 1] = c + (vertsArr[i + 1] - c).RotatedBy(offset);
        vector3s[i + 2] = c + (vertsArr[i + 2] - c).RotatedBy(offset);
        vector3s[i + 3] = c + (vertsArr[i + 3] - c).RotatedBy(offset);
    }

    public override void ExposeData()
    {
        // Prevent loading errors when the mod is removed
        if (Scribe.mode == LoadSaveMode.Saving)
        {
            Scribe.saver.WriteAttribute("IsNull", "True");
        }
    }
}