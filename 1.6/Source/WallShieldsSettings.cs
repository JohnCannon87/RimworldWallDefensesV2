using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;

namespace WallShields
{
    class WallShieldsSettings : ModSettings
    {
        internal static int shieldPowerPerCell = 1;
        internal static int shieldCellExponent = 2;
        internal static int laserPowerPerCell = 1;
        internal static int laserGridExponent = 2;
        internal static int maxLaserGridDistance = 5;
        internal static int protectionRange = 55;
        internal static int reloadSpeed = 120;
        internal static int ammoCount = 2;
        internal static int maxShotsAtDropPodOccupant = 3;
        internal static float bulletDamage = 10;
        internal static int chanceOfCompletelyDestroyingDropPod = 100;
        internal static int laserCannonDrain = 20;
        internal static float laserCannonDamage = 10f;
        public static float shieldInterceptCostWd = 6f;
        public static int percentageOfDamageDrained = 50;
        public static int shieldCooldownTicks = 5000;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref shieldPowerPerCell, "shieldPowerPerCell");
            Scribe_Values.Look(ref shieldCellExponent, "shieldCellExponent");
            Scribe_Values.Look(ref shieldInterceptCostWd, "shieldInterceptCostWd");
            Scribe_Values.Look(ref percentageOfDamageDrained, "percentageOfDamageDrained");
            Scribe_Values.Look(ref shieldCooldownTicks, "shieldCooldownTicks");

            Scribe_Values.Look(ref laserPowerPerCell, "laserPowerPerCell");
            Scribe_Values.Look(ref laserGridExponent, "laserGridExponent");
            Scribe_Values.Look(ref maxLaserGridDistance, "maxLaserGridDistance");

            Scribe_Values.Look(ref protectionRange, "protectionRange");
            Scribe_Values.Look(ref reloadSpeed, "reloadSpeed");
            Scribe_Values.Look(ref ammoCount, "ammoCount");
            Scribe_Values.Look(ref chanceOfCompletelyDestroyingDropPod, "chanceOfCompletelyDestroyingDropPod");
            Scribe_Values.Look(ref bulletDamage, "bulletDamage");
            Scribe_Values.Look(ref maxShotsAtDropPodOccupant, "maxShotsAtDropPodOccupant");
            Scribe_Values.Look(ref laserCannonDrain, "laserCannonDrain");
            Scribe_Values.Look(ref laserCannonDamage, "laserCannonDamage");

        }
    }
}
