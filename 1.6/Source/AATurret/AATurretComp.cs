using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.Sound;
using UnityEngine;

namespace WallShields
{
    class AATurretComp : ThingComp
    {
        protected AATurretTop top;
        int range = 0;
        int tickCount = 0;
        int ammoRemaining = 0;

        public static readonly SoundDef HitSoundDef = SoundDef.Named("AAGun_Fire");

        private static List<ThingDef> cachedSkyfallerDefs;
        private static readonly Dictionary<ThingDef, bool> defHasContentsCache = new Dictionary<ThingDef, bool>();
        static AATurretComp()
        {
            CacheSkyfallerDefs();
        }

        private static void CacheSkyfallerDefs()
        {
            cachedSkyfallerDefs = DefDatabase<ThingDef>.AllDefs
                .Where(d => typeof(Skyfaller).IsAssignableFrom(d.thingClass))
                .ToList();
            Log.Message($"[AATurret] Cached {cachedSkyfallerDefs.Count} skyfaller defs: {string.Join(", ", cachedSkyfallerDefs.Select(d => d.defName))}");
        }

        public CompProperties_AATurretComp AATurretComps => (CompProperties_AATurretComp)this.props;

        public override void PostDraw()
        {
            base.PostDraw();
            if (top != null)
            {
                top.DrawTurret();
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            range = WallShieldsSettings.protectionRange;
            ammoRemaining = WallShieldsSettings.ammoCount;
            top = new AATurretTop(this);
        }

        public override void CompTick()
        {
            if (parent.Map == null || parent.MapHeld == null || parent.Spawned == false)
            {
                return; // Don't tick while minified or unspawned
            }
            tickCount++;
            if (!IsActive())
            {
                top.TurretTopTick();
                return;
            }

            ShootThings();

            if(tickCount >= WallShieldsSettings.reloadSpeed)
            {
                tickCount = 0;
                Reload();
            }

            top.TurretTopTick();
        }

        private List<Thing> GetTargets()
        {
            var result = new List<Thing>();
            var shuttles = this.parent.Map.listerThings.ThingsOfDef(ThingDefOf.ShuttleIncoming);

            // Only iterate skyfaller defs once per tick
            foreach (var def in cachedSkyfallerDefs)
            {
                foreach (var thing in this.parent.Map.listerThings.ThingsOfDef(def))
                { // Skip shuttle incoming things early
                    if (thing.def.defName.IndexOf("shuttle", StringComparison.OrdinalIgnoreCase) >= 0)
                        continue;

                    if (IsThingAThreat(thing))
                    {
                        result.Add(thing);
                    }
                }
            }

            // Add non-skyfaller threats
            result.AddRange(this.parent.Map.listerThings.ThingsOfDef(ThingDefOf.DefoliatorShipPart));
            result.AddRange(this.parent.Map.listerThings.ThingsOfDef(ThingDefOf.PsychicDronerShipPart));
            result.RemoveAll(thing => shuttles.Contains(thing));


            return result;
        }

        private bool ThingDefHasContents(ThingDef def)
        {
            if (def == null) return false;
            if (defHasContentsCache.TryGetValue(def, out bool cached)) return cached;

            bool has = def.thingClass?.GetProperty("Contents", BindingFlags.Public | BindingFlags.Instance) != null
                       || def.thingClass?.GetField("Contents", BindingFlags.Public | BindingFlags.Instance) != null;

            defHasContentsCache[def] = has;
            return has;
        }

        private bool ThingHasContents(Thing thing)
        {
            if (thing is DropPodIncoming) return true;
            return ThingDefHasContents(thing.def);
        }

        private IEnumerable<Thing> GetInnerContainerThings(Thing thing)
        {
            if (thing is DropPodIncoming pod)
                return pod.Contents?.innerContainer ?? Enumerable.Empty<Thing>();

            try
            {
                var contentsProp = thing.GetType().GetProperty("Contents", BindingFlags.Public | BindingFlags.Instance);
                if (contentsProp == null) return null;
                var contentsObj = contentsProp.GetValue(thing);
                if (contentsObj == null) return null;

                var innerField = contentsObj.GetType().GetField("innerContainer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (innerField != null)
                    return innerField.GetValue(contentsObj) as IEnumerable<Thing>;

                var innerProp = contentsObj.GetType().GetProperty("innerContainer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (innerProp != null)
                    return innerProp.GetValue(contentsObj) as IEnumerable<Thing>;
            }
            catch (Exception ex)
            {
                Log.Message($"[AATurret] Reflection failed for {thing.def.defName}: {ex.Message}");
            }

            return null;
        }
        private bool IsThingAThreat(Thing thing)
        {
            if (ThingHasContents(thing))
            {
                var innerThings = GetInnerContainerThings(thing);
                if (innerThings != null)
                {
                    return innerThings.Any(t => t.Faction != null && t.Faction.HostileTo(Faction.OfPlayer));
                }
                return false; // contents unknown → assume safe
            }

            return thing.Faction != null && thing.Faction.HostileTo(Faction.OfPlayer);
        }

        private void ShootThings()
        {
            if (ammoRemaining <= 0) return;

            List<Thing> targets = GetTargets();
            List<Thing> targetsInRange = targets.Where(t => t.Position.InHorDistOf(this.parent.Position, range)).ToList();

            var targetsToShootAt = targetsInRange
                .Shuffle()
                .Take(Math.Max(ammoRemaining, targetsInRange.Count))
                .ToList();

            foreach (Thing thing in targetsToShootAt)
            {
                bool hasContents = ThingHasContents(thing);

                if (hasContents)
                {
                    if (ShouldDestroyDropPod())
                        DestroyThing(thing);
                    else
                        DamageSkyfallerOrPod(thing);
                }
                else
                {
                    DestroyThing(thing); // always destroy non-pod skyfallers
                }
                SimulateTracerTrail(thing);
            }

            ammoRemaining = Math.Max(ammoRemaining - targetsToShootAt.Count, 0);
            tickCount = 0;
        }

        private void DamageSkyfallerOrPod(Thing thing)
        {
            MakeShrapnelPlaySoundAndAimAtTarget(thing, 1);

            var innerThings = GetInnerContainerThings(thing);
            if (innerThings != null)
            {
                foreach (var occupant in innerThings)
                {
                    InjureOccupant(occupant);
                }
            }
        }

        private void DestroyThing(Thing thing)
        {
            MakeShrapnelPlaySoundAndAimAtTarget(thing, 3);
            if (!thing.Destroyed)
                thing.Destroy(DestroyMode.Vanish);
        }
        private void InjureOccupant(Thing t)
        {
            if (t != null && !t.Destroyed && t.Faction.HostileTo(Faction.OfPlayer))
            {
                for (int i = 0; i < Rand.RangeInclusive(1, WallShieldsSettings.maxShotsAtDropPodOccupant); i++)
                {
                    if (!t.Destroyed)
                        t.TakeDamage(new DamageInfo(DamageDefOf.Bullet, WallShieldsSettings.bulletDamage));
                }
            }
        }
        private void MakeShrapnelPlaySoundAndAimAtTarget(Thing target, int shrapnelCount)
        {
            SkyfallerShrapnelUtility.MakeShrapnel(
                target.Position,
                target.Map,
                Rand.RangeInclusive(0, 360),
                target.def.skyfaller.shrapnelDistanceFactor,
                shrapnelCount,
                0,
                spawnMotes: true
            );
            HitSoundDef.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
            top.CurRotation = (target.Position.ToVector3Shifted() - parent.DrawPos).AngleFlat();
            top.ticksUntilIdleTurn = Rand.RangeInclusive(150, 350);
        }

        private bool ShouldDestroyDropPod()
        {
            return Rand.RangeInclusive(0, 100) <= WallShieldsSettings.chanceOfCompletelyDestroyingDropPod;
        }

        private bool IsThingNotADropPod(Thing thing)
        {
            return !(thing is DropPodIncoming);
        }

        private void Reload()
        {
            ammoRemaining = WallShieldsSettings.ammoCount;
        }

        private bool PowerOn
        {
            get
            {
                CompPowerTrader comp = this.parent.GetComp<CompPowerTrader>();
                return comp != null && comp.PowerOn;
            }
        }

        private bool IsActive()
        {
            bool isActive = this.parent.Spawned && this.PowerOn;
            return isActive;
        }

        private void SimulateTracerTrail(Thing target)
        {
            var map = parent.Map;
            if (map == null) return;

            Vector3 start = parent.DrawPos + new Vector3(0f, 0f, 0f);
            Vector3 end = target.TrueCenter();
            Vector3 delta = end - start;
            float distance = delta.magnitude;
            Vector3 dir = delta.normalized;

            // Randomize the intercept path a little to make it more natural
            end += new Vector3(Rand.Range(-0.4f, 0.4f), 0f, Rand.Range(-0.4f, 0.4f));
            int steps = Mathf.CeilToInt(distance / 0.5f);

            for (int i = 0; i < steps; i++)
            {
                Vector3 pos = start + dir * (i * 0.5f);

                // Thin smoke trail
                if (Rand.Chance(0.5f))
                    FleckMaker.Static(pos, map, FleckDefOf.Smoke, 0.5f);

                // Occasional spark/tracer
                if (Rand.Chance(0.15f))
                    FleckMaker.Static(pos, map, FleckDefOf.MicroSparks, 0.6f);

                // Rare glowing pulse to make the line visible in darkness
                if (Rand.Chance(0.1f))
                    FleckMaker.Static(pos, map, FleckDefOf.LightningGlow, 0.4f);
            }

            // Optional small flash at the hit point
            FleckMaker.ThrowMicroSparks(end, map);
            FleckMaker.ThrowSmoke(end, map, 1.2f);
        }

    }
}
