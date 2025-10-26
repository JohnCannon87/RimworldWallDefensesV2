using System;
using System.Collections.Generic;
using UnityEngine;
using RimWorld;
using Verse;

namespace WallShields
{
    class LaserGridEmitterComp : ThingComp
    {
        private readonly int wattPerCell = WallShieldsSettings.laserPowerPerCell;
        private bool manualMode = false;
        private bool gridStatus = false;

        private ThingDef laserGridDef = DefDatabase<ThingDef>.GetNamed("LaserGrid", true);
        private List<IntVec3> gridCells = new List<IntVec3>();
        private Thing receiver;

        public override string CompInspectStringExtra()
        {
            if (!(gridCells.Count > 0))
            {
                return ResourceBank.LaserGridErrorNoReceiver.Translate(WallShieldsSettings.maxLaserGridDistance);
            }
            else
            {
                return $"{ResourceBank.LaserGridPowerUsage}: {PowerUsage} W";
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            ScanForGridCells();
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            DespawnShieldGrid(previousMap);
        }

        private void ScanForGridCells()
        {
            gridCells.Clear();
            IntVec3 direction = IntVec3.South.RotatedBy(parent.Rotation);
            IntVec3 vecFacing = parent.Position;
            gridCells.Add(vecFacing);
            receiver = LaserGridUtils.ScanForReceiver(vecFacing, gridCells, direction, this.parent.Map);
        }

        public override void CompTick()
        {
            if (gridCells.Count == 0)
            {
                ScanForGridCells();
            }

            if (receiver == null || receiver.Destroyed)
            {
                DespawnShieldGrid(this.parent.Map);
                gridCells.Clear();
            }

            if (!manualMode)
            {
                if (IsActive() && !gridStatus)
                {
                    SpawnShieldGrid();
                }
                else if (!IsActive() && gridStatus)
                {
                    DespawnShieldGrid(this.parent.Map);
                }
            }
        }

        private void SpawnShieldGrid()
        {
            foreach (IntVec3 gridCell in gridCells)
            {
                Thing laserGridThing = ThingMaker.MakeThing(laserGridDef);
                Pawn pawnInCell = gridCell.GetFirstPawn(this.parent.Map);
                if (pawnInCell != null)
                {
                    DespawnShieldGrid(this.parent.Map);
                    return;
                }

                GenSpawn.Spawn(laserGridThing, gridCell, this.parent.Map, this.parent.Rotation);
            }
            gridStatus = true;
            SetPowerLevel();
        }

        private void DespawnShieldGrid(Map map)
        {
            foreach (IntVec3 gridCell in gridCells)
            {
                List<Thing> gridCellThings = gridCell.GetThingList(map);
                foreach (Thing thing in gridCellThings)
                {
                    if (thing.def == laserGridDef)
                    {
                        thing.Destroy();
                        break;
                    }
                }
            }
            gridStatus = false;
            SetPowerLevel(0);
        }

        private bool PowerOn => this.parent.GetComp<CompPowerTrader>()?.PowerOn ?? false;

        private void SetPowerLevel() => SetPowerLevel(PowerUsage);

        private void SetPowerLevel(float powerLevel)
        {
            CompPowerTrader comp = this.parent.GetComp<CompPowerTrader>();
            comp.PowerOutput = powerLevel;
        }

        private float PowerUsage => -(float)Math.Pow(gridCells.Count, WallShieldsSettings.laserGridExponent) * wattPerCell;

        private bool IsActive() => this.parent.Spawned && this.PowerOn && IsThereAThreat();

        private bool IsThereAThreat() => GenHostility.AnyHostileActiveThreatToPlayer(parent.Map, true);

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (manualMode)
            {
                yield return new Command_Action
                {
                    action = () => this.SwapMode(),
                    defaultLabel = ResourceBank.ManualModeOff,
                    activateSound = SoundDef.Named("Click"),
                    icon = ContentFinder<Texture2D>.Get("UI/DisableManualMode", true)
                };

                yield return new Command_Action
                {
                    action = () => this.TurnOnOff(),
                    defaultLabel = gridStatus ? ResourceBank.DisableGrid : ResourceBank.EnableGrid,
                    activateSound = SoundDef.Named("Click"),
                    icon = ContentFinder<Texture2D>.Get(gridStatus ? "UI/DisableGrid" : "UI/EnableGrid", true)
                };
            }
            else
            {
                yield return new Command_Action
                {
                    action = () => this.SwapMode(),
                    defaultLabel = ResourceBank.ManualModeOn,
                    activateSound = SoundDef.Named("Click"),
                    icon = ContentFinder<Texture2D>.Get("UI/EnableManualMode", true)
                };
            }
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref manualMode, "manualMode");
            Scribe_Values.Look(ref gridStatus, "gridStatus");
        }

        private void TurnOnOff()
        {
            if (gridStatus)
                DespawnShieldGrid(this.parent.Map);
            else
                SpawnShieldGrid();
        }

        private void SwapMode() => manualMode = !manualMode;
    }
}
