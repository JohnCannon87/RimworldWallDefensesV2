using RimWorld;
using UnityEngine;
using Verse;

namespace WallShields
{
    class AATurretTop 
    {

		private AATurretComp parentComp;

		private ThingWithComps parentTurret;

		private float curRotationInt;

		public int ticksUntilIdleTurn;

		private int idleTurnTicksLeft;

		private bool idleTurnClockwise;

		public static readonly int ArtworkRotation = -90;

		public float CurRotation
		{
			get
			{
				return curRotationInt;
			}
			set
			{
				curRotationInt = value;
				if (curRotationInt > 360f)
				{
					curRotationInt -= 360f;
				}
				if (curRotationInt < 0f)
				{
					curRotationInt += 360f;
				}
			}
		}

		public void SetRotationFromOrientation()
		{
			CurRotation = parentTurret.Rotation.AsAngle;
		}

		public AATurretTop(AATurretComp ParentTurret)
		{
			parentComp = ParentTurret;
			parentTurret = ParentTurret.parent;
		}

		public void TurretTopTick()
		{
			if (ticksUntilIdleTurn > 0)
			{
				ticksUntilIdleTurn--;
				if (ticksUntilIdleTurn == 0)
				{
					if (Rand.Value < 0.5f)
					{
						idleTurnClockwise = true;
					}
					else
					{
						idleTurnClockwise = false;
					}
					idleTurnTicksLeft = 140;
				}
			}
			else
			{
				if (idleTurnClockwise)
				{
					CurRotation += 0.26f;
				}
				else
				{
					CurRotation -= 0.26f;
				}
				idleTurnTicksLeft--;
				if (idleTurnTicksLeft <= 0)
				{
					ticksUntilIdleTurn = Rand.RangeInclusive(150, 350);
				}
			}
		}

		public void DrawTurret()
		{
			Vector3 moveCentreVector = new Vector3(parentComp.AATurretComps.turretTopOffset.x, 0f, parentComp.AATurretComps.turretTopOffset.y);
			Vector3 vector = new Vector3(0f, 0f, 0f).RotatedBy(CurRotation);
			float turretTopDrawSize = parentComp.AATurretComps.turretTopDrawSize;
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(parentTurret.DrawPos + Altitudes.AltIncVect + vector + moveCentreVector, (CurRotation + (float)ArtworkRotation).ToQuat(), new Vector3(turretTopDrawSize, 1f, turretTopDrawSize));
			Graphics.DrawMesh(MeshPool.plane10, matrix, MaterialPool.MatFrom(parentComp.AATurretComps.texPath), 0);
		}
	}
}