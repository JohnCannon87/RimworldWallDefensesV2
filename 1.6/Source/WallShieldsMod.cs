using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using UnityEngine;
using HarmonyLib;

namespace WallShields
{
    public class WallShieldsMod : Mod
    {
        private Vector2 scrollPosition = Vector2.zero;

        public WallShieldsMod(ModContentPack content) : base(content)
        {
            LongEventHandler.ExecuteWhenFinished(GetSettings);
            var harmony = new Harmony(ResourceBank.ModId);
            harmony.PatchAll();
            Log.Message($"[{ResourceBank.ModName}] Patches loaded!");
        }

        public void GetSettings()
        {
            GetSettings<WallShieldsSettings>();
        }

        public static IEnumerable<ThingDef> PossibleThingDefs()
        {
            return DefDatabase<ThingDef>.AllDefs
                .Where(d => d.category == ThingCategory.Item && d.scatterableOnMapGen && !d.destroyOnDrop && !d.MadeFromStuff);
        }

        public override void DoSettingsWindowContents(Rect rect)
        {
            Rect viewRect = new Rect(0f, 0f, rect.width - 16f, 1600f);
            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);

            Listing_Standard list = new Listing_Standard()
            {
                ColumnWidth = viewRect.width
            };
            list.Begin(viewRect);

            DrawLaserGridSection(list);
            DrawShieldSection(list);
            DrawAATurretSection(list);
            DrawWallTurretSection(list);

            list.End();
            Widgets.EndScrollView();
        }

        public override string SettingsCategory() => ResourceBank.SettingsCategory;

        // ───────────────────────────────
        //  Laser Grid Section
        // ───────────────────────────────
        private void DrawLaserGridSection(Listing_Standard list)
        {
            DrawSectionHeader(list, ResourceBank.LaserGridSettings);
            list.GapLine();

            DrawIntSetting(list, ResourceBank.MaxDistance,
                ref WallShieldsSettings.maxLaserGridDistance, 0, 100);

            DrawIntSetting(list, ResourceBank.LaserPowerPerCell,
                ref WallShieldsSettings.laserPowerPerCell, 0, 100);

            DrawIntSetting(list, ResourceBank.CellExponentValue,
                ref WallShieldsSettings.laserGridExponent, 1, 20);

            list.Label(ResourceBank.TotalPowerFormula);

            list.GapLine();
            list.Gap(12f);
        }

        // ───────────────────────────────
        //  Shield Section
        // ───────────────────────────────
        private void DrawShieldSection(Listing_Standard list)
        {
            DrawSectionHeader(list, ResourceBank.ShieldSettings);
            list.GapLine();

            DrawIntSetting(list, ResourceBank.ShieldPowerPerCell,
                ref WallShieldsSettings.shieldPowerPerCell, 0, 100);

            DrawIntSetting(list, ResourceBank.ShieldCellExponent,
                ref WallShieldsSettings.shieldCellExponent, 1, 20);

            list.Label(ResourceBank.TotalPowerFormula);
            list.Label(ResourceBank.ShieldRecommendation);

            DrawFloatSetting(list, ResourceBank.ShieldInterceptCostWd,
                ref WallShieldsSettings.shieldInterceptCostWd, 0, 100);

            DrawIntSetting(list, ResourceBank.PercentageOfDamageDrained,
                ref WallShieldsSettings.percentageOfDamageDrained, 0, 100);

            DrawIntSetting(list, ResourceBank.ShieldCooldownTicks,
                ref WallShieldsSettings.shieldCooldownTicks, 0, 5000);

            list.GapLine();
            list.Gap(12f);
        }

        // ───────────────────────────────
        //  AA Turret Section
        // ───────────────────────────────
        private void DrawAATurretSection(Listing_Standard list)
        {
            DrawSectionHeader(list, ResourceBank.AATurretSettings);
            list.GapLine();

            DrawIntSetting(list, ResourceBank.ProtectionRange,
                ref WallShieldsSettings.protectionRange, 1, 56);

            DrawIntSetting(list, ResourceBank.ReloadSpeed,
                ref WallShieldsSettings.reloadSpeed, 1, 240);

            DrawIntSetting(list, ResourceBank.AmmoCount,
                ref WallShieldsSettings.ammoCount, 1, 20);

            list.Label(ResourceBank.NoteMultipleTurrets);
            list.Gap(6f);

            DrawIntSetting(list, ResourceBank.ChanceDestroyDropPod,
                ref WallShieldsSettings.chanceOfCompletelyDestroyingDropPod, 0, 100);

            DrawFloatSetting(list, ResourceBank.DamageToDropPod,
                ref WallShieldsSettings.bulletDamage, 1, 100);

            DrawIntSetting(list, ResourceBank.MaxHitsToDropPod,
                ref WallShieldsSettings.maxShotsAtDropPodOccupant, 1, 100);

            list.GapLine();
            list.Gap(12f);
        }

        // ───────────────────────────────
        //  Wall Turret Section
        // ───────────────────────────────
        private void DrawWallTurretSection(Listing_Standard list)
        {
            DrawSectionHeader(list, ResourceBank.WallTurretSettings);
            list.GapLine();

            DrawIntSetting(list, ResourceBank.BatteryDrain,
                ref WallShieldsSettings.laserCannonDrain, 1, 100);

            DrawFloatSetting(list, ResourceBank.LaserDamage,
                ref WallShieldsSettings.laserCannonDamage, 1f, 200f);

            list.GapLine();
            list.Gap(12f);
        }

        // ───────────────────────────────
        //  Shared Helpers
        // ───────────────────────────────
        private void DrawFloatSetting(Listing_Standard list, string label, ref float value, float min, float max)
        {
            list.Label($"{label}: {value:F2}");
            float newValue = Widgets.HorizontalSlider(list.GetRect(22f), value, min, max);
            value = (float)Math.Round(newValue, 2);

            Rect textRect = list.GetRect(24f);
            string buffer = value.ToString("F2");
            Widgets.TextFieldNumeric(textRect, ref value, ref buffer, min, max);
            list.Gap(6f);
        }

        private void DrawIntSetting(Listing_Standard list, string label, ref int value, int min, int max)
        {
            list.Label($"{label}: {value}");
            Rect textRect = list.GetRect(24f);
            string buffer = value.ToString();
            Widgets.IntEntry(textRect, ref value, ref buffer, 1);
            value = Mathf.Clamp(value, min, max);
            list.Gap(6f);
        }

        private void DrawSectionHeader(Listing_Standard list, string title)
        {
            Rect headerRect = list.GetRect(30f);
            Color oldColor = GUI.color;

            GUI.color = new Color(0.1f, 0.2f, 0.45f, 0.85f);
            Widgets.DrawBoxSolid(headerRect, GUI.color);

            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Medium;
            GUI.color = Color.white;
            Widgets.Label(headerRect, title);

            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            GUI.color = oldColor;

            list.Gap(4f);
        }
    }
}
