using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.Sound;

namespace WallShields
{
    public class WallMountedShieldEmitterComp : ThingComp
    {
        private List<IntVec3> shieldedCells;
        private Area selectedArea = null;
        private string selectedAreaString = null;
        private Mesh cubeMesh;
        private Material material;

        private Color shieldColor = new Color(0, 0.5f, 0.5f, 0.35f);
        private readonly int wattPerCell = WallShieldsSettings.shieldPowerPerCell;
        private bool dirtyMesh = true;
        private int shieldActiveTimer = 0;
        private int shieldDelayTicks = 2000;
        private int renderQueue = 3650;

        private int shieldCooldownTicksRemaining;
        private bool IsOnCooldown => shieldCooldownTicksRemaining > 0;

        public static readonly SoundDef HitSoundDef = SoundDef.Named("WallShield_Hit");

        private readonly List<Mesh> meshes = new List<Mesh>();
        private static readonly List<Vector3> verts = new List<Vector3>();
        private static readonly List<int> tris = new List<int>();
        private static readonly List<Color> colors = new List<Color>();

        // ───────────────────────────────
        //  Color Picker
        // ───────────────────────────────
        private void OpenColorPicker()
        {
            Find.WindowStack.Add(new Dialog_ShieldColorPicker(
                shieldColor,
                newColor =>
                {
                    shieldColor = newColor;
                    dirtyMesh = true;
                }));
        }

        // ───────────────────────────────
        //  RimWorld Overrides
        // ───────────────────────────────
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (respawningAfterLoad && selectedAreaString != null)
                selectedArea = parent.Map.areaManager.GetLabeled(selectedAreaString);

            RefreshShieldCells();
        }

        public override void CompTick()
        {
            if (parent.Map == null || !parent.Spawned)
                return;

            // Cooldown countdown
            if (shieldCooldownTicksRemaining > 0)
            {
                shieldCooldownTicksRemaining--;
                SetPowerLevel(0);
                return;
            }

            if (!IsActive())
            {
                SetPowerLevel(0);
                return;
            }

            shieldActiveTimer++;
            ShieldThings();
            SetPowerLevel();
        }

        public override void PostDraw()
        {
            GenerateMesh();
            DrawShieldField();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new Command_Action
            {
                action = SelectRegion,
                defaultLabel = "Select Region to Shield",
                icon = ContentFinder<Texture2D>.Get("UI/SelectRegion", true),
                activateSound = SoundDef.Named("Click")
            };

            yield return new Command_Action
            {
                action = OpenColorPicker,
                defaultLabel = "Change Shield Color",
                defaultDesc = "Change the color of the wall shield’s visual effect.",
                icon = ContentFinder<Texture2D>.Get("UI/Commands/ChangeColor", true),
                activateSound = SoundDef.Named("Click")
            };
        }

        // ───────────────────────────────
        //  Shield Logic
        // ───────────────────────────────
        private bool PowerOn => parent.GetComp<CompPowerTrader>()?.PowerOn ?? false;

        private float PowerUsage =>
            -(float)Math.Pow(shieldedCells.Count, WallShieldsSettings.shieldCellExponent) * wattPerCell;

        public Color Color => shieldColor;

        private void SetPowerLevel() => SetPowerLevel(PowerUsage);

        private void SetPowerLevel(float powerLevel)
        {
            CompPowerTrader comp = parent.GetComp<CompPowerTrader>();
            if (comp != null)
                comp.PowerOutput = powerLevel;
        }

        private bool IsActive()
        {
            if (IsOnCooldown)
                return false;

            bool active = parent.Spawned && PowerOn && IsThereAThreat();
            if (active)
                shieldActiveTimer = 0;
            else
                return !(shieldActiveTimer > shieldDelayTicks);
            return active;
        }

        private bool IsThereAThreat() =>
            GenHostility.AnyHostileActiveThreatToPlayer(parent.Map, true);

        private void ShieldThings()
        {
            if (shieldedCells == null)
                return;

            foreach (IntVec3 cell in shieldedCells)
                BlockProjectiles(cell);
        }

        private void BlockProjectiles(IntVec3 cell)
        {
            if (!cell.InBounds(parent.Map))
                return;

            var thingsHere = parent.Map.thingGrid.ThingsListAt(cell).ToList(); // safe copy

            foreach (Thing thing in thingsHere)
            {
                if (thing is Projectile projectile && !projectile.Destroyed)
                {
                    bool intercept = !WasProjectileFiredByAlly(projectile);
                    if (!intercept) continue;

                    if (ConsumeShieldEnergy(projectile))
                    {
                        projectile.Destroy();
                        FleckMaker.ThrowMicroSparks(projectile.ExactPosition, parent.Map);
                        FleckMaker.ThrowSmoke(projectile.ExactPosition, parent.Map, 1.5f);
                        FleckMaker.ThrowLightningGlow(projectile.ExactPosition, parent.Map, 1.5f);
                        HitSoundDef.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
                    }
                    else
                    {
                        TriggerShieldFailure(); // insufficient energy
                    }
                }
            }
        }

        // ───────────────────────────────
        //  Battery Drain + Overload Logic
        // ───────────────────────────────
        private bool ConsumeShieldEnergy(Projectile proj)
        {
            try
            {
                float costWd = ComputeInterceptEnergyCostWd(proj);

                var powerComp = parent.GetComp<CompPowerTrader>();
                if (powerComp?.PowerNet == null)
                    return false;

                float available = powerComp.PowerNet.batteryComps.Sum(b => b.StoredEnergy);
                if (available < costWd)
                    return false;

                float remaining = costWd;
                foreach (var bat in powerComp.PowerNet.batteryComps)
                {
                    float draw = Mathf.Min(bat.StoredEnergy, remaining);
                    bat.DrawPower(draw);
                    remaining -= draw;
                    if (remaining <= 0f) break;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[WallShields] ConsumeShieldEnergy exception: {ex}");
                return false;
            }
            return true;
        }

        private float ComputeInterceptEnergyCostWd(Projectile proj)
        {
            float baseCostWd = WallShieldsSettings.shieldInterceptCostWd;
            float percentageOfDamageDrained = WallShieldsSettings.percentageOfDamageDrained / 100f;

            int dmg = 0;
            try { dmg = proj.DamageAmount; } catch { }

            float dmgFactorWd = dmg * percentageOfDamageDrained;
            return Mathf.Max(0f, baseCostWd + dmgFactorWd);
        }

        private void TriggerShieldFailure()
        {
            if (WallShieldsSettings.shieldCooldownTicks == 0)
                return; // disabled

            try
            {
                shieldCooldownTicksRemaining = WallShieldsSettings.shieldCooldownTicks;
                shieldActiveTimer = 0;

                // visual overload effect
                if (!shieldedCells.NullOrEmpty())
                {
                    foreach (var cell in shieldedCells)
                    {
                        if (!cell.InBounds(parent.Map)) continue;
                        FleckMaker.ThrowMicroSparks(cell.ToVector3Shifted(), parent.Map);
                        FleckMaker.ThrowSmoke(cell.ToVector3Shifted(), parent.Map, 1.0f);
                        FleckMaker.ThrowLightningGlow(cell.ToVector3Shifted(), parent.Map, 0.8f);
                    }
                }

                // slight EMP-like explosion
                GenExplosion.DoExplosion(
                    parent.Position,
                    parent.Map,
                    Math.Min(79, shieldedCells.Count),
                    DamageDefOf.EMP,
                    parent,
                    damAmount: 20,
                    armorPenetration: 0,
                    explosionSound: HitSoundDef
                );

                Messages.Message("Shield Generator overloaded and shut down!", parent, MessageTypeDefOf.NegativeEvent);
            }
            catch (Exception ex)
            {
                Log.Error($"[WallShields] TriggerShieldFailure exception: {ex}");
            }
        }

        // ───────────────────────────────
        //  Drawing + Mesh
        // ───────────────────────────────
        private void GenerateMesh()
        {
            if (cubeMesh == null)
                cubeMesh = GraphicsUtil.CreateCuboidMesh();
        }

        private void DrawShieldField()
        {
            if (!IsActive() || selectedArea == null)
                return;

            if (dirtyMesh)
                RegenerateMesh();

            if (parent.Map == Find.CurrentMap)
            {
                for (int i = 0; i < meshes.Count; i++)
                    Graphics.DrawMesh(meshes[i], Vector3.zero, Quaternion.identity, material, 0);
            }
        }

        private void RefreshShieldCells()
        {
            if (selectedArea == null)
            {
                shieldedCells = new List<IntVec3>();
                return;
            }

            dirtyMesh = true;
            shieldedCells = selectedArea.ActiveCells.ToList();
        }

        private bool WasProjectileFiredByAlly(Projectile projectile)
        {
            Thing launcher = GetInstanceField(typeof(Projectile), projectile, "launcher") as Thing;
            return launcher?.Faction != null && launcher.Faction.IsPlayer;
        }

        // ───────────────────────────────
        //  UI + Save
        // ───────────────────────────────
        private void SelectRegion()
        {
            AreaUtility.MakeAllowedAreaListFloatMenu(delegate (Area a)
            {
                selectedArea = a;
                selectedAreaString = selectedArea.Label;
                RefreshShieldCells();
            }, addNullAreaOption: false, addManageOption: true, parent.Map);
        }

        public static object GetInstanceField(Type type, object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public |
                                     BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            return field.GetValue(instance);
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref selectedAreaString, "selectedAreaString");
            Scribe_Values.Look(ref shieldActiveTimer, "shieldActiveTimer");
            Scribe_Values.Look(ref shieldColor, "shieldColor", new Color(0, 0.5f, 0.5f, 0.35f));
            Scribe_Values.Look(ref shieldCooldownTicksRemaining, "shieldCooldownTicksRemaining", 0);
        }

        // ───────────────────────────────
        //  Mesh Regeneration
        // ───────────────────────────────
        public void RegenerateMesh()
        {
            for (int i = 0; i < meshes.Count; i++)
                meshes[i].Clear();

            int meshIndex = 0;
            int vertexCount = 0;

            if (meshes.Count < 1)
                meshes.Add(new Mesh { name = "WallShieldMesh" });

            Mesh mesh = meshes[meshIndex];
            CellRect rect = new CellRect(0, 0, parent.Map.Size.x, parent.Map.Size.z);
            float y = AltitudeLayer.MapDataOverlay.AltitudeFor();
            bool careAboutVertexColors = false;

            for (int x = rect.minX; x <= rect.maxX; x++)
            {
                for (int z = rect.minZ; z <= rect.maxZ; z++)
                {
                    int idx = CellIndicesUtility.CellToIndex(x, z, parent.Map.Size.x);
                    if (!GetCellBool(idx)) continue;

                    verts.Add(new Vector3(x, y, z));
                    verts.Add(new Vector3(x, y, z + 1));
                    verts.Add(new Vector3(x + 1, y, z + 1));
                    verts.Add(new Vector3(x + 1, y, z));
                    Color c = GetCellExtraColor(idx);
                    colors.AddRange(new[] { c, c, c, c });
                    if (c != Color.white)
                        careAboutVertexColors = true;

                    int count = verts.Count;
                    tris.Add(count - 4);
                    tris.Add(count - 3);
                    tris.Add(count - 2);
                    tris.Add(count - 4);
                    tris.Add(count - 2);
                    tris.Add(count - 1);

                    vertexCount++;
                    if (vertexCount >= 16383)
                    {
                        FinalizeWorkingDataIntoMesh(mesh);
                        meshIndex++;
                        if (meshes.Count < meshIndex + 1)
                            meshes.Add(new Mesh { name = "WallShieldMesh" });
                        mesh = meshes[meshIndex];
                        vertexCount = 0;
                    }
                }
            }

            FinalizeWorkingDataIntoMesh(mesh);
            CreateMaterialIfNeeded(careAboutVertexColors);
            dirtyMesh = false;
        }

        private void FinalizeWorkingDataIntoMesh(Mesh mesh)
        {
            if (verts.Count == 0) return;
            mesh.SetVertices(verts);
            verts.Clear();
            mesh.SetTriangles(tris, 0);
            tris.Clear();
            mesh.SetColors(colors);
            colors.Clear();
        }

        private void CreateMaterialIfNeeded(bool careAboutVertexColors)
        {
            if (material == null)
                material = SolidColorMaterials.NewSolidColorMaterial(shieldColor, ShaderDatabase.MetaOverlay);
            else
                material.color = shieldColor;

            material.renderQueue = renderQueue;
        }

        public bool GetCellBool(int index) =>
            selectedArea != null && ((ICellBoolGiver)selectedArea).GetCellBool(index);

        public Color GetCellExtraColor(int index) =>
            selectedArea != null ? ((ICellBoolGiver)selectedArea).GetCellExtraColor(index) : Color.white;

        public override string CompInspectStringExtra()
        {
            string status;

            if (IsOnCooldown)
            {
                float secondsLeft = shieldCooldownTicksRemaining / 60f; // ticks → seconds
                status = $"<color=#FF8800>COOLDOWN</color> ({secondsLeft:F1}s remaining)";
            }
            else if (!PowerOn)
            {
                status = "<color=#FF5555>Offline (No Power)</color>";
            }
            else if (!IsThereAThreat())
            {
                status = "<color=#BBBBBB>Idle</color>";
            }
            else
            {
                status = "<color=#00FFAA>Active</color>";
            }

            string regionInfo = selectedArea != null
                ? $"Area: {selectedArea.Label} ({shieldedCells?.Count ?? 0} cells)"
                : "Area: <color=#FF5555>Not selected</color>";

            string powerInfo = PowerOn
                ? $"Power Use: {Math.Abs(PowerUsage):F0} W"
                : "Power: Off";

            string cooldownInfo = WallShieldsSettings.shieldCooldownTicks == 0
                ? "Overload: <color=#88FF88>Disabled</color>"
                : $"Overload Cooldown: {WallShieldsSettings.shieldCooldownTicks / 60f:F1}s";

            return
                $"Status: {status}\n" +
                $"{regionInfo}\n" +
                $"{powerInfo}\n" +
                $"{cooldownInfo}";
        }
    }
}
