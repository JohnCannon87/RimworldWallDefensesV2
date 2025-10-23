using System.Collections.Generic;
using Verse;

namespace WallShields
{
    class LaserGridUtils
    {
        /**
         * Scan in a direction searching for an emitter, add all cells inbetween to the gridCells List 
         **/
        public static Thing ScanForReceiver(IntVec3 startCell, List<IntVec3> gridCells, IntVec3 direction, Map map)
        {
            //jcLog.jc("gridCells Count: " + gridCells.Count);
            //jcLog.jc("Scanning Cell: " + startCell);
            IntVec3 nextCell = startCell + direction;
            List<Thing> things = nextCell.GetThingList(map);
            //jcLog.jc("Things In Cell: " + things);
            foreach (Thing thing in things)
            {
                if (thing.def.defName == "LaserGridReceiver")
                {
                    //jcLog.jc("FOUND RECEIVER YAY !");
                    gridCells.Add(nextCell);
                    return thing;
                }
            }
            if (gridCells.Count >= WallShieldsSettings.maxLaserGridDistance)
            {
                //jcLog.jc("Hit Max Cell Search Count !");
                gridCells.Clear();
                return null;
            }
            else
            {
                //jcLog.jc("Checking Next Cell !");
                gridCells.Add(nextCell);
                return ScanForReceiver(nextCell, gridCells, direction, map);
            }
        }

    }
}

