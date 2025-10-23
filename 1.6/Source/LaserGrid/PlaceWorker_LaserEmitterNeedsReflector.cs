using Verse;
using System.Collections.Generic;

namespace WallShields
{
    class PlaceWorker_LaserEmitterNeedsReflector : PlaceWorker
    {

        public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            //jcLog.jc("Place Worker For Laser Grid Emitter");
            IntVec3 direction = IntVec3.South.RotatedBy(rot);
            IntVec3 vecFacing = center + IntVec3.South.RotatedBy(rot); //This should be the tile in front of us

            List<IntVec3> gridCells = new List<IntVec3>();
            gridCells.Add(vecFacing);

            Thing receiver = LaserGridUtils.ScanForReceiver(vecFacing, gridCells, direction, map);

            if(receiver == null)
            {
                return ResourceBank.NoReflectorFound;
            }
            else
            {
                return true;
            }            
        }
    }
}
