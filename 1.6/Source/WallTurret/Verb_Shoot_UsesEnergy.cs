using RimWorld;
using Verse;
using UnityEngine;
using System.Linq;

namespace WallShields
{
    public class Verb_ShootBeam_UsesEnergy : Verb_ShootBeam
    {
        public float EnergyCost => WallShieldsSettings.laserCannonDrain;
                protected override bool TryCastShot()
        {
            //jcLog.Message("[Verb_ShootBeam_UsesEnergy] TryCastShot called");

            var building = this.caster as Building;
            if (building == null)
            {
                //jcLog.Message("[Verb_ShootBeam_UsesEnergy] Caster is not a building");
                return false;
            }

            var powerComp = building.GetComp<CompPowerTrader>();
            if (powerComp == null)
            {
                //jcLog.Message("[Verb_ShootBeam_UsesEnergy] No CompPowerTrader found");
                return false;
            }

            if (powerComp.PowerNet == null)
            {
                //jcLog.Message("[Verb_ShootBeam_UsesEnergy] PowerNet is null");
                return false;
            }

            float availableEnergy = powerComp.PowerNet.batteryComps.Sum(b => b.StoredEnergy);
            //jcLog.Message($"[Verb_ShootBeam_UsesEnergy] Available energy: {availableEnergy}, Required energy: {EnergyCost}");

            if (availableEnergy < EnergyCost)
            {
                //jcLog.Message("[Verb_ShootBeam_UsesEnergy] Not enough power to fire");
                MoteMaker.ThrowText(building.DrawPos, building.Map, "Not enough power!", 8f);
                return false;
            }

            // Draw power from batteries
            float remaining = EnergyCost;
            foreach (var battery in powerComp.PowerNet.batteryComps)
            {
                float draw = Mathf.Min(battery.StoredEnergy, remaining);
                battery.DrawPower(draw);
                remaining -= draw;
                //jcLog.Message($"[Verb_ShootBeam_UsesEnergy] Drew {draw} power, remaining to draw: {remaining}");
                if (remaining <= 0f)
                    break;
            }

            //jcLog.Message("[Verb_ShootBeam_UsesEnergy] Energy drawn successfully, firing beam");

            // Now call base firing logic
            return base.TryCastShot();
        }


    }
}
