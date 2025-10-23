// Verse.PlaceWorker
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace WallShields
{
    class PlaceWorker_AATurret : PlaceWorker
	{
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
		{
			if (WallShieldsSettings.protectionRange > 0f)
			{
				GenDraw.DrawRadiusRing(center, WallShieldsSettings.protectionRange);
			}
		}
	}
}
