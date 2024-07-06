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
                if (this.maxEnergy <= 0f)
                {
                    list = new List<DamageDef>();
                }
                else
                {
                    (list = new List<DamageDef>()).Add(DamageDefOf.EMP);
                }
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
            return this.absorb == null || this.absorb.Contains(def);
        }
        public override void PostLoad()
        {
            base.PostLoad();
            ShieldsSystem_RWrd.ApplyShieldPatches();
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
    }
}
