using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Electromagnetic.Core;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Electromagnetic.Abilities
{
    public class TeslaProjectile : Bullet
    {
        public Thing Holder
        {
            get
            {
                bool flag = this.holder == null;
                Thing launcher;
                if (flag)
                {
                    launcher = this.launcher;
                }
                else
                {
                    launcher = this.holder;
                }
                return launcher;
            }
        }
        protected virtual int GetDamageAmount
        {
            get
            {
                return this.def.projectile.GetDamageAmount(1f, null);
            }
        }
        protected virtual DamageInfo GetDamageInfo(Thing hitThing)
        {
            return new DamageInfo(this.Props.damageDef, (float)this.GetDamageAmount, this.def.projectile.GetArmorPenetration(this.launcher, null), this.Holder.DrawPos.AngleToFlat(hitThing.DrawPos), base.Launcher, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null, true, true, QualityCategory.Normal, true);
        }
        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            bool isRanged = this.def.projectile.damageDef.isRanged;
            this.def.projectile.damageDef.isRanged = true;
            base.Impact(hitThing, false);
            this.def.projectile.damageDef.isRanged = isRanged;
            bool flag = this.mainLauncher == null;
            if (flag)
            {
                this.mainLauncher = this.launcher;
            }
            bool flag2 = this.equipmentDef == null;
            if (flag2)
            {
                this.equipmentDef = ThingDef.Named("Gun_Autopistol");
            }
            bool flag3 = TeslaProjectile.wasDeflected;
            if (flag3)
            {
                TeslaProjectile.wasDeflected = false;
                bool flag4 = Rand.Chance(0.3f);
                if (flag4)
                {
                    this.DestroyAll();
                }
            }
            else
            {
                bool flag5 = hitThing == null && !this.shotAnything;
                if (flag5)
                {
                    this.shotAnything = true;
                }
                else
                {
                    bool flag6 = hitThing != null && !this.shotAnything;
                    if (flag6)
                    {
                        BattleLogEntry_RangedImpact battleLogEntry_RangedImpact = new BattleLogEntry_RangedImpact(this.launcher, hitThing, this.intendedTarget.Thing, this.equipmentDef, this.def, this.targetCoverDef);
                        Find.BattleLog.Add(battleLogEntry_RangedImpact);
                        DamageInfo damageInfo = this.GetDamageInfo(hitThing);
                        Pawn pawn = (Pawn)base.Launcher;
                        if (pawn.IsHaveRoot())
                        {
                            Hediff_RWrd_PowerRoot root = pawn.GetRoot();
                            int level = root.energy.CurrentDef.level;
                            damageInfo.SetAmount(damageInfo.Amount + level);
                        }
                        hitThing.TakeDamage(damageInfo).AssociateWithLog(battleLogEntry_RangedImpact);
                        bool flag7 = this.Props.addFire && hitThing.TryGetComp<CompAttachBase>() != null && hitThing.Map != null;
                        if (flag7)
                        {
                            Fire fire = (Fire)GenSpawn.Spawn(ThingDefOf.Fire, hitThing.Position, hitThing.Map, WipeMode.Vanish);
                            fire.AttachTo(hitThing);
                        }
                        bool flag8 = this.Props.impactRadius > 0f;
                        if (flag8)
                        {
                            GenExplosion.DoExplosion(hitThing.Position, base.Map, this.Props.impactRadius, this.Props.explosionDamageDef, base.Launcher, this.def.projectile.GetDamageAmount(1f, null), -1f, null, null, null, null, null, 0f, 1, null, false, null, 0f, 1, 0f, false, null, null, null, true, 1f, 0f, true, null, 1f, null, null);
                        }
                        SoundDef impactSound = this.Props.impactSound;
                        if (impactSound != null)
                        {
                            impactSound.PlayOneShot(hitThing);
                        }
                        this.RegisterHit(hitThing);
                        bool flag9 = this.numBounces < this.MaxBounceCount;
                        if (flag9)
                        {
                            Thing thing = this.NextTarget(hitThing);
                            bool flag10 = thing != null;
                            if (flag10)
                            {
                                this.FireAt(thing);
                            }
                        }
                        this.shotAnything = true;
                    }
                }
            }
        }
        protected virtual int MaxBounceCount
        {
            get
            {
                return this.Props.maxBounceCount;
            }
        }
        private void RegisterHit(Thing hitThing)
        {
            this.RegisterHit(this, hitThing);
            foreach (TeslaProjectile projectile in this.allProjectiles)
            {
                this.RegisterHit(projectile, hitThing);
            }
        }
        private void RegisterHit(TeslaProjectile projectile, Thing hitThing)
        {
            bool flag = !projectile.prevTargets.Contains(hitThing);
            if (flag)
            {
                projectile.prevTargets.Add(hitThing);
            }
            projectile.curLifetime = 0;
        }
        public TeslaChainingProps Props
        {
            get
            {
                return this.def.GetModExtension<TeslaChainingProps>();
            }
        }
        public Thing PrimaryEquipment
        {
            get
            {
                Thing primaryLauncher = this.PrimaryLauncher;
                Building_TurretGun building_TurretGun = primaryLauncher as Building_TurretGun;
                bool flag = building_TurretGun != null;
                Thing result;
                if (flag)
                {
                    result = building_TurretGun.gun;
                }
                else
                {
                    result = null;
                }
                return result;
            }
        }
        public Verb PrimaryVerb
        {
            get
            {
                Thing primaryLauncher = this.PrimaryLauncher;
                Building_TurretGun building_TurretGun = primaryLauncher as Building_TurretGun;
                bool flag = building_TurretGun != null;
                Verb result;
                if (flag)
                {
                    result = building_TurretGun.AttackVerb;
                }
                else
                {
                    result = null;
                }
                return result;
            }
        }
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            Vector3 vector = this.Holder.TrueCenter();
            Vector3 vector2 = this.DrawPos;
            bool flag = vector2.magnitude > vector.magnitude;
            if (flag)
            {
                Vector3 vector3 = vector;
                vector = vector2;
                vector2 = vector3;
            }
            Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(vector2 + (vector - vector2) / 2f, Quaternion.AngleAxis(vector.AngleToFlat(vector2) + 90f, Vector3.up), new Vector3(1f, 1f, (vector - vector2).magnitude)), this.Graphic.MatSingle, 0);
        }
        public void FireAt(Thing target)
        {
            TeslaProjectile teslaProjectile = (TeslaProjectile)GenSpawn.Spawn(this.def, base.Position, base.Map, WipeMode.Vanish);
            teslaProjectile.Launch(this.launcher, target, target, base.HitFlags, false, this.PrimaryEquipment);
            teslaProjectile.holder = this;
            bool flag = this.mainLauncher != null;
            if (flag)
            {
                teslaProjectile.mainLauncher = this.mainLauncher;
            }
            this.allProjectiles.Add(teslaProjectile);
            this.prevTargets.Add(target);
            bool flag2 = teslaProjectile.prevTargets == null;
            if (flag2)
            {
                teslaProjectile.prevTargets = new List<Thing>();
            }
            teslaProjectile.prevTargets.AddRange(this.prevTargets);
            this.numBounces++;
            teslaProjectile.numBounces = this.numBounces;
            teslaProjectile.curLifetime = this.curLifetime;
        }
        private bool IsValidTarget(Thing thing)
        {
            Thing primaryLauncher = this.PrimaryLauncher;
            Building_TurretGun building_TurretGun = primaryLauncher as Building_TurretGun;
            bool flag = building_TurretGun != null && !TeslaProjectile.isValidTarget(building_TurretGun, thing);
            bool result;
            if (flag)
            {
                result = false;
            }
            else
            {
                Verb primaryVerb = this.PrimaryVerb;
                bool flag2 = primaryVerb != null && !primaryVerb.targetParams.CanTarget(thing, null);
                result = !flag2;
            }
            return result;
        }
        private Thing NextTarget(Thing currentTarget)
        {
            IEnumerable<Thing> enumerable = (from t in GenRadial.RadialDistinctThingsAround(currentTarget.PositionHeld, base.Map, this.Props.bounceRange, false)
                                             where (this.Props.targetFriendly || t.HostileTo(this.launcher)) && this.IsValidTarget(t)
                                             select t).Except(new Thing[]
            {
                this,
                this.usedTarget.Thing
            });
            enumerable = enumerable.Except(this.prevTargets);
            enumerable = from t in enumerable
                         orderby t.Position.DistanceTo(this.Holder.Position)
                         select t;
            return enumerable.FirstOrDefault<Thing>();
        }
        private Thing PrimaryLauncher
        {
            get
            {
                bool flag = this.mainLauncher != null;
                Thing result;
                if (flag)
                {
                    result = this.mainLauncher;
                }
                else
                {
                    foreach (TeslaProjectile teslaProjectile in this.allProjectiles)
                    {
                        bool flag2 = teslaProjectile.mainLauncher != null;
                        if (flag2)
                        {
                            return teslaProjectile.mainLauncher;
                        }
                    }
                    result = null;
                }
                return result;
            }
        }
        public override void Tick()
        {
            base.Tick();
            bool flag = this.shotAnything;
            if (flag)
            {
                this.curLifetime++;
            }
            bool flag2 = this.curLifetime > this.Props.maxLifetime;
            if (flag2)
            {
                this.DestroyAll();
            }
            else
            {
                bool destroyed = this.Holder.Destroyed;
                if (destroyed)
                {
                    this.DestroyAll();
                }
                else
                {
                    bool flag3 = this.allProjectiles.Any((TeslaProjectile x) => x.Destroyed);
                    if (flag3)
                    {
                        this.DestroyAll();
                    }
                }
            }
        }
        public void DestroyAll()
        {
            TeslaProjectile.destroyAll = true;
            for (int i = this.allProjectiles.Count - 1; i >= 0; i--)
            {
                bool flag = !this.allProjectiles[i].Destroyed;
                if (flag)
                {
                    this.allProjectiles[i].Destroy(DestroyMode.Vanish);
                }
            }
            this.Destroy(DestroyMode.Vanish);
            TeslaProjectile.destroyAll = false;
        }
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            bool flag = TeslaProjectile.destroyAll;
            if (flag)
            {
                base.Destroy(mode);
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Thing>(ref this.mainLauncher, "mainLauncher", false);
            Scribe_References.Look<Thing>(ref this.holder, "holder", false);
            Scribe_Values.Look<int>(ref this.numBounces, "numBounces", 0, false);
            Scribe_Values.Look<int>(ref this.curLifetime, "curLifetime", 0, false);
            Scribe_Values.Look<bool>(ref this.shotAnything, "firedOnce", false, false);
            Scribe_Collections.Look<TeslaProjectile>(ref this.allProjectiles, "allProjectiles", LookMode.Reference, Array.Empty<object>());
            Scribe_Collections.Look<Thing>(ref this.prevTargets, "prevTargets", LookMode.Reference, Array.Empty<object>());
        }
        public int curLifetime;
        protected int numBounces;
        protected List<TeslaProjectile> allProjectiles = new List<TeslaProjectile>();
        protected List<Thing> prevTargets = new List<Thing>();
        private Thing holder;
        private Thing mainLauncher;
        private bool shotAnything;
        public static bool wasDeflected;
        private static readonly Func<Building_TurretGun, Thing, bool> isValidTarget = (Func<Building_TurretGun, Thing, bool>)Delegate.CreateDelegate(typeof(Func<Building_TurretGun, Thing, bool>), AccessTools.Method(typeof(Building_TurretGun), "IsValidTarget", null, null));
        public static bool destroyAll;
        [HarmonyPatch]
        public static class ProjectilePatches
        {
            [HarmonyTargetMethods]
            public static IEnumerable<MethodBase> GetMethods()
            {
                yield return AccessTools.Method(typeof(Projectile), "ImpactSomething", null, null);
                yield return AccessTools.Method(typeof(Projectile), "CheckForFreeIntercept", null, null);
                yield break;
            }
            public static void Postfix()
            {
                TeslaProjectile.wasDeflected = false;
            }
        }
    }
}
