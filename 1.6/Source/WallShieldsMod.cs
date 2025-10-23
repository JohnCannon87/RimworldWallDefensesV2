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
            var harmony = new Harmony("com.Arcjc007.WallDefensesV2");
            harmony.PatchAll();
            Log.Message("[WallDefensesV2] Patches loaded!");
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

        public override string SettingsCategory() => "Wall Defenses";

        // ───────────────────────────────
        //  Laser Grid Section
        // ───────────────────────────────
        private void DrawLaserGridSection(Listing_Standard list)
        {
            DrawSectionHeader(list, "Laser Grid Settings");
            list.GapLine();

            DrawIntSetting(list, "Max Distance Between Emitter and Reflector",
                ref WallShieldsSettings.maxLaserGridDistance, 0, 100);

            DrawIntSetting(list, "Power Per Cell Covered",
                ref WallShieldsSettings.laserPowerPerCell, 0, 100);

            DrawIntSetting(list, "Cell Exponent Multiplication Value",
                ref WallShieldsSettings.laserGridExponent, 1, 20);

            list.Label("Total power = (Cells Covered ^ Cell Exponent) * Power Per Cell");

            list.GapLine();
            list.Gap(12f);
        }

        // ───────────────────────────────
        //  Shield Section
        // ───────────────────────────────
        private void DrawShieldSection(Listing_Standard list)
        {
            DrawSectionHeader(list, "Shield Settings");
            list.GapLine();

            DrawIntSetting(list, "Power Per Cell Covered",
                ref WallShieldsSettings.shieldPowerPerCell, 0, 100);

            DrawIntSetting(list, "Cell Exponent Multiplication Value",
                ref WallShieldsSettings.shieldCellExponent, 1, 20);

            list.Label("Total power = (Cells Covered ^ Cell Exponent) * Power Per Cell");

            list.Label("HIGHLY recommend only having the intercept cost on with a low power draw or vice versa");

            DrawFloatSetting(list, "Shield Intercept Cost Wd",
                ref WallShieldsSettings.shieldInterceptCostWd, 0, 100);

            DrawIntSetting(list, "Percentage Of Damage Drained",
                ref WallShieldsSettings.percentageOfDamageDrained, 0, 100);

            DrawIntSetting(list, "Shield Cooldown Ticks, 0 disables breaking feature, Roughly 60 ticks to 1 second",
                ref WallShieldsSettings.shieldCooldownTicks, 0, 5000);

            list.GapLine();
            list.Gap(12f);
        }

        // ───────────────────────────────
        //  AA Turret Section
        // ───────────────────────────────
        private void DrawAATurretSection(Listing_Standard list)
        {
            DrawSectionHeader(list, "AA Turret Settings");
            list.GapLine();

            DrawIntSetting(list, "Protection Range (max 56)",
                ref WallShieldsSettings.protectionRange, 1, 56);

            DrawIntSetting(list, "Reload Speed (ticks)",
                ref WallShieldsSettings.reloadSpeed, 1, 240);

            DrawIntSetting(list, "Ammo Count",
                ref WallShieldsSettings.ammoCount, 1, 20);

            list.Label("Note: Multiple turrets can heavily damage or destroy droppods in range.");
            list.Gap(6f);

            DrawIntSetting(list, "Chance To Completely Destroy Droppod (%)",
                ref WallShieldsSettings.chanceOfCompletelyDestroyingDropPod, 0, 100);

            DrawFloatSetting(list, "Damage To Droppod Occupant Per Hit",
                ref WallShieldsSettings.bulletDamage, 1, 100);

            DrawIntSetting(list, "Max Hits To Droppod Occupant",
                ref WallShieldsSettings.maxShotsAtDropPodOccupant, 1, 100);

            list.GapLine();
            list.Gap(12f);
        }

        // ───────────────────────────────
        //  Wall Turret Section
        // ───────────────────────────────
        private void DrawWallTurretSection(Listing_Standard list)
        {
            DrawSectionHeader(list, "Wall Turret Settings");
            list.GapLine();

            DrawIntSetting(list, "Battery Drain Per Beam Fired",
                ref WallShieldsSettings.laserCannonDrain, 1, 100);

            DrawFloatSetting(list, "Wall Turret Beam - Damage per Shot",
                ref WallShieldsSettings.laserCannonDamage, 1f, 200f);

            list.GapLine();
            list.Gap(12f);
        }

        // ───────────────────────────────
        //  Shared Input Helpers
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

        // ───────────────────────────────
        //  Section Header Helper
        // ───────────────────────────────
        private void DrawSectionHeader(Listing_Standard list, string title)
        {
            Rect headerRect = list.GetRect(30f); // height of the color band
            Color oldColor = GUI.color;

            // Deep blue background
            GUI.color = new Color(0.1f, 0.2f, 0.45f, 0.85f);
            Widgets.DrawBoxSolid(headerRect, GUI.color);

            // Header text
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Medium;
            GUI.color = Color.white;
            Widgets.Label(headerRect, title);

            // Reset text + color
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            GUI.color = oldColor;

            list.Gap(4f);
        }
    }
}
