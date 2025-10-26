using Verse;
using UnityEngine;

namespace WallShields
{
    [StaticConstructorOnStartup]
    public static class ResourceBank
    {
        public const string ModId = "com.Arcjc007.WallDefensesV2";
        public const string ModName = "Wall Defenses V2";

        // ───────────────────────────────
        //  General Keys
        // ───────────────────────────────
        public static readonly string NeedsWallKey = "WS_NeedsWall";
        public static readonly string WallAlreadyOccupiedKey = "WS_WallAlreadyOccupied";
        public static readonly string NoReflectorFoundKey = "WS_NoReflectorFound";
        public static readonly string SettingsCategoryKey = "WS_SettingsCategory";

        // ───────────────────────────────
        //  Section Headers
        // ───────────────────────────────
        public static readonly string LaserGridSettingsKey = "WS_LaserGridSettings";
        public static readonly string ShieldSettingsKey = "WS_ShieldSettings";
        public static readonly string AATurretSettingsKey = "WS_AATurretSettings";
        public static readonly string WallTurretSettingsKey = "WS_WallTurretSettings";

        // ───────────────────────────────
        //  Laser Grid Settings Keys
        // ───────────────────────────────
        public static readonly string MaxDistanceKey = "WS_MaxDistance";
        public static readonly string LaserPowerPerCellKey = "WS_LaserPowerPerCell";
        public static readonly string CellExponentValueKey = "WS_CellExponentValue";
        public static readonly string TotalPowerFormulaKey = "WS_TotalPowerFormula";

        // ───────────────────────────────
        //  Shield Keys
        // ───────────────────────────────
        public static readonly string ShieldPowerPerCellKey = "WS_ShieldPowerPerCell";
        public static readonly string ShieldCellExponentKey = "WS_ShieldCellExponent";
        public static readonly string ShieldInterceptCostWdKey = "WS_ShieldInterceptCostWd";
        public static readonly string PercentageOfDamageDrainedKey = "WS_PercentageOfDamageDrained";
        public static readonly string ShieldCooldownTicksKey = "WS_ShieldCooldownTicks";
        public static readonly string ShieldRecommendationKey = "WS_ShieldRecommendation";

        // ───────────────────────────────
        //  AA Turret Keys
        // ───────────────────────────────
        public static readonly string ProtectionRangeKey = "WS_ProtectionRange";
        public static readonly string ReloadSpeedKey = "WS_ReloadSpeed";
        public static readonly string AmmoCountKey = "WS_AmmoCount";
        public static readonly string NoteMultipleTurretsKey = "WS_NoteMultipleTurrets";
        public static readonly string ChanceDestroyDropPodKey = "WS_ChanceDestroyDropPod";
        public static readonly string DamageToDropPodKey = "WS_DamageToDropPod";
        public static readonly string MaxHitsToDropPodKey = "WS_MaxHitsToDropPod";

        // ───────────────────────────────
        //  Wall Turret Keys
        // ───────────────────────────────
        public static readonly string BatteryDrainKey = "WS_BatteryDrain";
        public static readonly string LaserDamageKey = "WS_LaserDamage";
        public static readonly string WallTurretNoteKey = "WS_WallTurretNote";

        // ───────────────────────────────
        //  LaserGridEmitterComp Keys
        // ───────────────────────────────
        public static readonly string LaserGridErrorNoReceiverKey = "WS_LaserGridErrorNoReceiver";
        public static readonly string LaserGridPowerUsageKey = "WS_LaserGridPowerUsage";
        public static readonly string ManualModeOnKey = "WS_ManualModeOn";
        public static readonly string ManualModeOffKey = "WS_ManualModeOff";
        public static readonly string EnableGridKey = "WS_EnableGrid";
        public static readonly string DisableGridKey = "WS_DisableGrid";

        // ───────────────────────────────
        //  WallMountedShieldEmitterComp Keys
        // ───────────────────────────────
        public static readonly string ShieldSelectRegionKey = "WS_ShieldSelectRegion";
        public static readonly string ShieldChangeColorKey = "WS_ShieldChangeColor";
        public static readonly string ShieldChangeColorDescKey = "WS_ShieldChangeColorDesc";
        public static readonly string ShieldOverloadMessageKey = "WS_ShieldOverloadMessage";

        public static readonly string ShieldStatusCooldownKey = "WS_ShieldStatusCooldown";
        public static readonly string ShieldStatusOfflineKey = "WS_ShieldStatusOffline";
        public static readonly string ShieldStatusIdleKey = "WS_ShieldStatusIdle";
        public static readonly string ShieldStatusActiveKey = "WS_ShieldStatusActive";

        public static readonly string ShieldRegionInfoKey = "WS_ShieldRegionInfo";
        public static readonly string ShieldRegionNotSelectedKey = "WS_ShieldRegionNotSelected";
        public static readonly string ShieldPowerUseKey = "WS_ShieldPowerUse";
        public static readonly string ShieldPowerOffKey = "WS_ShieldPowerOff";
        public static readonly string ShieldOverloadDisabledKey = "WS_ShieldOverloadDisabled";
        public static readonly string ShieldOverloadCooldownKey = "WS_ShieldOverloadCooldown";

        // ───────────────────────────────
        //  Accessors
        // ───────────────────────────────
        public static string NeedsWall => NeedsWallKey.Translate();
        public static string WallAlreadyOccupied => WallAlreadyOccupiedKey.Translate();
        public static string NoReflectorFound => NoReflectorFoundKey.Translate();
        public static string SettingsCategory => SettingsCategoryKey.Translate();

        public static string LaserGridSettings => LaserGridSettingsKey.Translate();
        public static string ShieldSettings => ShieldSettingsKey.Translate();
        public static string AATurretSettings => AATurretSettingsKey.Translate();
        public static string WallTurretSettings => WallTurretSettingsKey.Translate();

        public static string MaxDistance => MaxDistanceKey.Translate();
        public static string LaserPowerPerCell => LaserPowerPerCellKey.Translate();
        public static string CellExponentValue => CellExponentValueKey.Translate();
        public static string TotalPowerFormula => TotalPowerFormulaKey.Translate();

        public static string ShieldPowerPerCell => ShieldPowerPerCellKey.Translate();
        public static string ShieldCellExponent => ShieldCellExponentKey.Translate();
        public static string ShieldInterceptCostWd => ShieldInterceptCostWdKey.Translate();
        public static string PercentageOfDamageDrained => PercentageOfDamageDrainedKey.Translate();
        public static string ShieldCooldownTicks => ShieldCooldownTicksKey.Translate();
        public static string ShieldRecommendation => ShieldRecommendationKey.Translate();

        public static string ProtectionRange => ProtectionRangeKey.Translate();
        public static string ReloadSpeed => ReloadSpeedKey.Translate();
        public static string AmmoCount => AmmoCountKey.Translate();
        public static string NoteMultipleTurrets => NoteMultipleTurretsKey.Translate();
        public static string ChanceDestroyDropPod => ChanceDestroyDropPodKey.Translate();
        public static string DamageToDropPod => DamageToDropPodKey.Translate();
        public static string MaxHitsToDropPod => MaxHitsToDropPodKey.Translate();

        public static string BatteryDrain => BatteryDrainKey.Translate();
        public static string LaserDamage => LaserDamageKey.Translate();
        public static string WallTurretNote => WallTurretNoteKey.Translate();

        public static string LaserGridErrorNoReceiver => LaserGridErrorNoReceiverKey.Translate();
        public static string LaserGridPowerUsage => LaserGridPowerUsageKey.Translate();
        public static string ManualModeOn => ManualModeOnKey.Translate();
        public static string ManualModeOff => ManualModeOffKey.Translate();
        public static string EnableGrid => EnableGridKey.Translate();
        public static string DisableGrid => DisableGridKey.Translate();

        // WallMountedShieldEmitterComp Accessors
        public static string ShieldSelectRegion => ShieldSelectRegionKey.Translate();
        public static string ShieldChangeColor => ShieldChangeColorKey.Translate();
        public static string ShieldChangeColorDesc => ShieldChangeColorDescKey.Translate();
        public static string ShieldOverloadMessage => ShieldOverloadMessageKey.Translate();

        public static TaggedString ShieldStatusCooldown => ShieldStatusCooldownKey.Translate();
        public static string ShieldStatusOffline => ShieldStatusOfflineKey.Translate();
        public static string ShieldStatusIdle => ShieldStatusIdleKey.Translate();
        public static string ShieldStatusActive => ShieldStatusActiveKey.Translate();

        public static TaggedString ShieldRegionInfo => ShieldRegionInfoKey.Translate();
        public static TaggedString ShieldRegionNotSelected => ShieldRegionNotSelectedKey.Translate();
        public static TaggedString ShieldPowerUse => ShieldPowerUseKey.Translate();
        public static string ShieldPowerOff => ShieldPowerOffKey.Translate();
        public static string ShieldOverloadDisabled => ShieldOverloadDisabledKey.Translate();
        public static TaggedString ShieldOverloadCooldown => ShieldOverloadCooldownKey.Translate();
    }
}
