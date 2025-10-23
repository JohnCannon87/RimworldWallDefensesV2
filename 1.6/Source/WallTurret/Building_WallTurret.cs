using RimWorld;
using UnityEngine;
using Verse;

namespace WallShields
{
    public class Building_WallTurret : Building_TurretGun
    {
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            // Draw the wall base / hull section at normal altitude
            Graphic.Draw(drawLoc, Rotation, this);

            // Raise the turret top slightly above hull corners
            float turretAltitude = AltitudeLayer.BuildingOnTop.AltitudeFor() + 0.05f;
            Vector3 turretPos = new Vector3(drawLoc.x, turretAltitude, drawLoc.z);

            Vector3 offset = CalculateWallLinkOffset();
            Top.DrawTurret(turretPos + offset, Vector3.zero, 0f);
        }

        private Vector3 CalculateWallLinkOffset()
        {
            Map map = this.Map;
            IntVec3 pos = this.Position;

            bool linkNorth = ShouldLinkWith(pos + IntVec3.North, map);
            bool linkEast = ShouldLinkWith(pos + IntVec3.East, map);
            bool linkSouth = ShouldLinkWith(pos + IntVec3.South, map);
            bool linkWest = ShouldLinkWith(pos + IntVec3.West, map);

            // Standalone Wall
            float offsetX = 0f;
            float offsetZ = 0.2f;

            bool linkedEastButNotWest = linkEast && !linkWest;
            bool linkedWestButNotEast = linkWest && !linkEast;
            bool linkedNorthButNotSouth = linkNorth && !linkSouth;
            bool linkedSouthButNotNorth = linkSouth && !linkNorth;

            // Example logic — tweak these based on mesh behavior:
            if (linkedEastButNotWest) offsetX += 0.0f;
            else if (linkedWestButNotEast) offsetX -= 0.0f;

            if (linkedNorthButNotSouth) offsetZ += 0.0f;
            else if (linkedSouthButNotNorth) offsetZ -= 0.0f;

            return new Vector3(offsetX, 0f, offsetZ);
        }

        private bool ShouldLinkWith(IntVec3 c, Map map)
        {
            if (!c.InBounds(map)) return false;
            Building b = c.GetEdifice(map);
            return b != null && b.def.graphicData?.linkFlags != null &&
                   (b.def.graphicData.linkFlags & LinkFlags.Wall) != 0;
        }
    }
}
