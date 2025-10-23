using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

[HarmonyPatch(typeof(SectionLayer_GravshipHull))]
[HarmonyPatch(nameof(SectionLayer_GravshipHull.ShouldDrawCornerPiece))]
public static class HullTurret_Postfix
{
    // Keep the same order as the original code:
    // 0=N, 1=E, 2=S, 3=W, 4=NW, 5=NE, 6=SE, 7=SW
    private static readonly IntVec3[] Directions =
    {
        IntVec3.North,
        IntVec3.East,
        IntVec3.South,
        IntVec3.West,
        IntVec3.North + IntVec3.West,
        IntVec3.North + IntVec3.East,
        IntVec3.South + IntVec3.East,
        IntVec3.South + IntVec3.West
    };

    // Copied from original: used to pick color source
    private static readonly int[][] DirectionPairs = new int[][]
    {
        new[] { 0, 2 },
        new[] { 1, 3 },
        new[] { 4, 6 },
        new[] { 5, 7 }
    };

    static void Postfix(
        IntVec3 pos, Map map, TerrainGrid terrGrid,
        ref SectionLayer_GravshipHull.CornerType cornerType,
        ref Color color, ref bool __result)
    {
        // If vanilla already drew it, don't touch it.
        if (__result) return;

        // *** Mirror original early-outs exactly ***
        if (pos.GetEdifice(map) != null) return;

        TerrainDef terrainDef = terrGrid.FoundationAt(pos);
        if (terrainDef != null && terrainDef.IsSubstructure) return;

        // Build the same tmpChecks but with "hull-like" detection
        bool[] tmpChecks = new bool[Directions.Length];
        for (int j = 0; j < Directions.Length; j++)
        {
            var edifice = (pos + Directions[j]).GetEdificeSafe(map);
            var def = edifice?.def;
            tmpChecks[j] = IsHullLike(def);
        }

        // *** Exact corner detection from original ***
        cornerType = SectionLayer_GravshipHull.CornerType.None;

        if (tmpChecks[0] && tmpChecks[3] && !tmpChecks[2] && !tmpChecks[1])
        {
            cornerType = tmpChecks[4]
                ? SectionLayer_GravshipHull.CornerType.Corner_NW
                : SectionLayer_GravshipHull.CornerType.Diagonal_NW;
        }
        else if (tmpChecks[0] && tmpChecks[1] && !tmpChecks[2] && !tmpChecks[3])
        {
            cornerType = tmpChecks[5]
                ? SectionLayer_GravshipHull.CornerType.Corner_NE
                : SectionLayer_GravshipHull.CornerType.Diagonal_NE;
        }
        else if (tmpChecks[2] && tmpChecks[1] && !tmpChecks[0] && !tmpChecks[3])
        {
            cornerType = tmpChecks[6]
                ? SectionLayer_GravshipHull.CornerType.Corner_SE
                : SectionLayer_GravshipHull.CornerType.Diagonal_SE;
        }
        else if (tmpChecks[2] && tmpChecks[3] && !tmpChecks[0] && !tmpChecks[1])
        {
            cornerType = tmpChecks[7]
                ? SectionLayer_GravshipHull.CornerType.Corner_SW
                : SectionLayer_GravshipHull.CornerType.Diagonal_SW;
        }

        if (cornerType == SectionLayer_GravshipHull.CornerType.None)
            return;

        // *** Exact color-pick logic from original ***
        // (This fixes the "wrong graphic tint" you saw.)
        color = Color.white; // default; will be overwritten below if any neighbor is found
        foreach (var pair in DirectionPairs)
        {
            var list = pair.Where(i => tmpChecks[i]).ToList();
            if (list.Count > 0)
            {
                int idx = list.First();
                var src = (pos + Directions[idx]).GetEdificeSafe(map);
                if (src != null)
                    color = src.DrawColor;
                break;
            }
        }

        __result = true;
    }

    private static bool IsHullLike(ThingDef def)
    {
        if (def == null) return false;

        // Resolve GravshipHull dynamically (Odyssey content)
        var gravHull = DefDatabase<ThingDef>.GetNamedSilentFail("GravshipHull");
        if (def == gravHull) return true;

        // Your turret(s)
        if (def.defName == "HullTurret") return true;

        // Optional: support a category or multiple defs if you expand later
        // if (def.thingCategories?.Any(c => c.defName.Contains("Hull")) == true) return true;

        return false;
    }
}
