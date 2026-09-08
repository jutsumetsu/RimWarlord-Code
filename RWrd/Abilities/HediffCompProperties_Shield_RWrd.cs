using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace Electromagnetic.Abilities
{
    public class HediffCompProperties_Shield_RWrd : HediffCompProperties_Draw_RWrd
    {
        public override void ResolveReferences(HediffDef parent)
        {
            base.ResolveReferences(parent);
            bool flag = this.breakOn == null;
            if (flag)
            {
                List<DamageDef> list;
                list = new List<DamageDef>();
                this.breakOn = list;
            }
            bool flag2 = this.graphic == null;
            if (flag2)
            {
                this.graphic = new GraphicData
                {
                    graphicClass = typeof(Graphic_Single),
                    texPath = "Other/ShieldBubble",
                    shaderType = ShaderTypeDefOf.Transparent
                };
            }
        }
        public bool Absorbs(DamageDef def)
        {
            return this.absorb == null || this.absorb.Contains(def) || IsProjectileDamage(def);
        }

        public static bool IsProjectileDamage(DamageDef def)
        {
            return def != null && projectileDamageDefs.Contains(def);
        }
        public override void PostLoad()
        {
            base.PostLoad();
            InitializeProjectileDamageDefs();
            ShieldsSystem_RWrd.ApplyShieldPatches();
        }

        private static void InitializeProjectileDamageDefs()
        {
            if (projectileDamageDefsInitialized)
            {
                return;
            }
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefsListForReading)//可能会在开技能的时候卡一下
            {
                if (thingDef.projectile != null && thingDef.projectile.damageDef != null)
                {
                    projectileDamageDefs.Add(thingDef.projectile.damageDef);
                }
            }
            projectileDamageDefsInitialized = true;
        }
        public List<DamageDef> breakOn;
        public List<DamageDef> absorb;
        public AttackType absorbAttackType = AttackType.Ranged;
        public AttackType cannotUseAttackType = AttackType.Ranged;
        public float maxEnergy = -1f;
        public float energyPerTick = -1f;
        public int rechargeDelay = 3000;
        public float energyLossPerDamage = 0.033f;
        public bool fullOnAdd = true;
        public float energyPctOnReset = 0.2f;
        public SoundDef sustainer;
        public SoundDef soundBroken;
        public SoundDef soundRecharge;
        public SoundDef soundEnded;
        public FleckDef absorbedFleck;
        public FleckDef brokenFleck;
        public bool doDust = true;
        public AttackType damageOnAttack = AttackType.None;
        public DamageDef damageType;
        public int damageAmount = -1;
        public float armorPenetration = -1f;
        public bool doRandomRotation = true;
        private static readonly HashSet<DamageDef> projectileDamageDefs = new HashSet<DamageDef>();
        private static bool projectileDamageDefsInitialized;
    }
}
