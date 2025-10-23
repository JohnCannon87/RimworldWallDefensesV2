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
            if(!(gridCells.Count > 0))
            {
                return "Error, No matching receiver found within distance of " + WallShieldsSettings.maxLaserGridDistance + "!";
            }
            else
            {
                return "Power when active: " + PowerUsage + " W";
            }            
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            ScanForGridCells();
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            //jcLog.jc("Destroying and Despawning Laser Grid !");
            DespawnShieldGrid(previousMap);
        }

        private void ScanForGridCells()
        {
            gridCells.Clear();
            IntVec3 direction = IntVec3.South.RotatedBy(parent.Rotation);
            IntVec3 vecFacing = parent.Position; //This should be the tile in front of us
            gridCells.Add(vecFacing);
            //jcLog.jc("Starting Cell: " + vecFacing);
            receiver = LaserGridUtils.ScanForReceiver(vecFacing, gridCells, direction, this.parent.Map);
        }

        

        public override void CompTick()
        {
            if(gridCells.Count == 0)
            {
                ScanForGridCells();
            }
            if(receiver == null || receiver.Destroyed)
            {
                DespawnShieldGrid(this.parent.Map);
                gridCells.Clear();
            }
            if (!manualMode)
            {
                if(IsActive() && !gridStatus)
                {
                    //If we should be active and grid status is false then spawn Shield Grid
                    SpawnShieldGrid();
                }else if(!IsActive() && gridStatus)
                {
                    //If we should not be active and grid status is true then despawn Shield Grid
                    DespawnShieldGrid(this.parent.Map);
                }
            }
            //Do Nothing on tick if in manual mode
        }
                
        private void SpawnShieldGrid()
        {
            //jcLog.jc("Spawning Shield Grid");
            foreach (IntVec3 gridCell in gridCells)
            {
                //jcLog.jc("Spawning Grid In Cell: " + gridCell);
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
            //jcLog.jc("Despawning Shield Grid");
            foreach (IntVec3 gridCell in gridCells)
            {
                //jcLog.jc("Despawning Grid In Cell: " + gridCell);
                List<Thing> gridCellThings = gridCell.GetThingList(map);
                foreach (Thing thing in gridCellThings)
                {
                    if(thing.def == laserGridDef)
                    {
                        thing.Destroy();
                        break;
                    }
                }
            }
            gridStatus = false;
            SetPowerLevel(0);
        }

        private bool PowerOn
        {
            get
            {
                CompPowerTrader comp = this.parent.GetComp<CompPowerTrader>();
                return comp != null && comp.PowerOn;
            }
        }

        private void SetPowerLevel()
        {
            SetPowerLevel(PowerUsage);
        }

        private void SetPowerLevel(float powerLevel)
        {
            CompPowerTrader comp = this.parent.GetComp<CompPowerTrader>();
            comp.PowerOutput = powerLevel;
        }

        private float PowerUsage
        {
            get
            {
                return -(float)Math.Pow(gridCells.Count, WallShieldsSettings.laserGridExponent) * wattPerCell;
            }
        }

        private bool IsActive()
        {
            bool isActive = this.parent.Spawned && this.PowerOn && IsThereAThreat();

            return isActive;
        }

        private bool IsThereAThreat()
        {
            if (GenHostility.AnyHostileActiveThreatToPlayer(parent.Map, true))
            {
                return true;
            }
            return false;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (manualMode)
            {
                {
                    Command_Action act = new Command_Action();
                    act.action = () => this.SwapMode();
                    act.defaultLabel = "Turn off Manual Mode";
                    act.activateSound = SoundDef.Named("Click");
                    act.icon = ContentFinder<Texture2D>.Get("UI/DisableManualMode", true);
                    yield return act;
                }
                if (gridStatus)
                {
                    {
                        Command_Action act = new Command_Action();
                        act.action = () => this.TurnOnOff();
                        act.defaultLabel = "Disable Shield Grid";
                        act.activateSound = SoundDef.Named("Click");
                        act.icon = ContentFinder<Texture2D>.Get("UI/DisableGrid", true);
                        yield return act;
                    }
                }
                else
                {
                    {
                        Command_Action act = new Command_Action();
                        act.action = () => this.TurnOnOff();
                        act.defaultLabel = "Enable Shield Grid";
                        act.activateSound = SoundDef.Named("Click");
                        act.icon = ContentFinder<Texture2D>.Get("UI/EnableGrid", true);
                        yield return act;
                    }
                }
            }
            else
            {
                {
                    Command_Action act = new Command_Action();
                    act.action = () => this.SwapMode();
                    act.defaultLabel = "Turn on Manual Mode";
                    act.activateSound = SoundDef.Named("Click");
                    act.icon = ContentFinder<Texture2D>.Get("UI/EnableManualMode", true);
                    yield return act;
                }
            }
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref manualMode, "manualMode");
            Scribe_Values.Look(ref gridStatus, "gridStatus");
        }

        private void TurnOnOff()
        {
            //jcLog.jc("GridStatus: " + gridStatus);
            if (gridStatus)
            { 
                DespawnShieldGrid(this.parent.Map);
            }
            else
            {
                SpawnShieldGrid();
            }
        }

        private void SwapMode()
        {
            manualMode = !manualMode;
        }

    }
}
