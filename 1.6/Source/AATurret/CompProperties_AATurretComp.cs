using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using System.Diagnostics;
using UnityEngine;
namespace WallShields
{
    class CompProperties_AATurretComp : CompProperties
    {

        public CompProperties_AATurretComp()
        {
            this.compClass = typeof(AATurretComp);
        }

        /*public int protectionRange;
        public int reloadSpeed;
        public int ammoCount;*/
        public Vector2 turretTopOffset;
        public float turretTopDrawSize;
        public String texPath;

    }
}
