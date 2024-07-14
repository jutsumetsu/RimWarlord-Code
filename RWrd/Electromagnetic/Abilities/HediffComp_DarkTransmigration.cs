using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse.Sound;
using Verse;
using Electromagnetic.Core;

namespace Electromagnetic.Abilities
{
    public class HediffComp_DarkTransmigration : HediffComp
    {
        public HediffCompProperties_DarkTransmigration Props
        {
            get
            {
                return this.props as HediffCompProperties_DarkTransmigration;
            }
        }
        
        public float EnergyMax
        {
            get
            {
                Pawn pawn = this.Pawn;
                if (pawn.IsHaveRoot())
                {
                    Hediff_RWrd_PowerRoot root = pawn.GetRoot();
                    return root.energy.PowerFlow;
                }

                return this.Props.maxEnergy;
            }
        }
        public float Energy
        {
            get
            {
                Pawn pawn = this.Pawn;
                if (pawn.IsHaveRoot())
                {
                    Hediff_RWrd_PowerRoot root = pawn.GetRoot();
                    return root.energy.energy;
                }

                return this.energy;
            }
        }


        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            this.useEnergy = (this.EnergyMax > 0f);
            bool flag = this.useEnergy;
            if (flag)
            {
                bool fullOnAdd = this.Props.fullOnAdd;
                if (fullOnAdd)
                {

                    this.energy = this.EnergyMax;
                }
                else
                {
                    this.energy = this.Props.energyPctOnReset * this.EnergyMax;
                }
            }
            else
            {
                this.energy = -1f;
            }
        }


        public virtual void PreApplyDamage(ref DamageInfo dinfo, ref bool absorbed)
        {
            bool flag = absorbed;
            if (!flag)
            {
                bool shieldActive = true;
                if (shieldActive)
                {
                    bool flag2 = this.Props.breakOn.Contains(dinfo.Def);
                    if (flag2)
                    {
                        this.Break();
                    }
                    else
                    {
                        bool flag3 = false;
                        switch (this.Props.absorbAttackType)
                        {
                            case AttackType.Melee:
                                flag3 = !dinfo.Def.isRanged;
                                break;
                            case AttackType.Ranged:
                                flag3 = (dinfo.Def.isRanged || dinfo.Def.isExplosive);
                                break;
                            case AttackType.Both:
                                flag3 = true;
                                break;
                        }
                        bool flag4 = flag3 && this.Props.Absorbs(dinfo.Def) && this.AbsorbDamage(ref dinfo);
                        if (flag4)
                        {
                            absorbed = true;
                            this.impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
                            Vector3 loc = base.Pawn.TrueCenter() + this.impactAngleVect.RotatedBy(180f) * 0.5f;
                            float num = Mathf.Min(10f, 2f + dinfo.Amount / 10f);
                            bool flag5 = this.Props.absorbedFleck != null;
                            if (flag5)
                            {
                                FleckMaker.Static(loc, base.Pawn.Map, this.Props.absorbedFleck, num);
                            }
                            bool doDust = this.Props.doDust;
                            if (doDust)
                            {
                                int num2 = (int)num;
                                for (int i = 0; i < num2; i++)
                                {
                                    FleckMaker.ThrowDustPuff(loc, base.Pawn.Map, Rand.Range(0.8f, 1.2f));
                                }
                            }
                        }
                        bool flag6 = false;
                        switch (this.Props.damageOnAttack)
                        {
                            case AttackType.Melee:
                                flag6 = !dinfo.Def.isRanged;
                                break;
                            case AttackType.Ranged:
                                flag6 = (dinfo.Def.isRanged || dinfo.Def.isExplosive);
                                break;
                            case AttackType.Both:
                                flag6 = true;
                                break;
                        }
                        bool flag7 = flag6 && dinfo.Instigator != null;
                        if (flag7)
                        {
                            this.ApplyDamage(dinfo);
                        }
                    }
                }
            }
        }

        protected virtual void ApplyDamage(DamageInfo dinfo)
        {
            dinfo.Instigator.TakeDamage(new DamageInfo(this.Props.damageType, (float)this.Props.damageAmount, this.Props.armorPenetration, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null, true, true, QualityCategory.Normal, true));
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look<float>(ref this.energy, "energy", 0f, false);
            Scribe_Values.Look<bool>(ref this.useEnergy, "useEnergy", false, false);
            Scribe_Values.Look<int>(ref this.ticksTillReset, "ticksTillReset", 0, false);
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            /*severityAdjustment = 0.5f;*/
            bool flag = this.useEnergy;
            if (flag)
            {
                bool flag2 = this.ticksTillReset > 0;
                if (flag2)
                {
                    this.ticksTillReset--;
                    bool flag3 = this.ticksTillReset <= 0;
                    if (flag3)
                    {
                        this.Reset();
                    }
                }
                else
                {
                    bool flag4 = this.energy <= this.EnergyMax;
                    if (flag4)
                    {
                        this.energy += this.Props.energyPerTick;
                    }
                }
            }
            
        }

        

        protected virtual bool AbsorbDamage(ref DamageInfo dinfo)
        {
            bool flag = this.useEnergy;
            bool result;
            if (flag)
            {
                float num = dinfo.Amount * this.Props.energyLossPerDamage;
                bool flag2 = num < this.Energy;
                Log.Message("Damage:" + num.ToString() + ", Energy:" + this.Energy.ToString());
                bool ActiveS = this.ShieldActive;
                
                if (flag2)
                {
                    foreach (Hediff hediff in this.parent.pawn.health.hediffSet.hediffs)
                    {
                        bool flagH = hediff.GetType() == typeof(Hediff_RWrd_PowerRoot);
                        if (flagH)
                        {
                            ((Hediff_RWrd_PowerRoot)hediff).energy.SetEnergy((float)(-num));
                            ((Hediff_RWrd_PowerRoot)hediff).energy.SetExp(0.1f * (float)(-(float)num));
                            break;
                        }
                    }
                    //this.energy -= num;
                    dinfo.SetAmount(0f);
                    result = true;
                }
                else
                {
                    foreach (Hediff hediff in this.parent.pawn.health.hediffSet.hediffs)
                    {
                        bool flagH = hediff.GetType() == typeof(Hediff_RWrd_PowerRoot);
                        if (flagH)
                        {
                            ((Hediff_RWrd_PowerRoot)hediff).energy.SetEnergy((float)(-num));                        
                            break;
                        }
                    }
                    dinfo.SetAmount(dinfo.Amount - this.Energy);
                    this.Break();
                    result = false;
                }
            }
            else
            {
                dinfo.SetAmount(0f);
                result = true;
            }
            return result;
        }

       

        public override void CompPostPostRemoved()
        {

            base.CompPostPostRemoved();
        }

        public virtual bool AllowVerbCast(Verb verb)
        {
            bool result;
            switch (this.Props.cannotUseAttackType)
            {
                case AttackType.Melee:
                    result = (verb is Verb_MeleeAttack);
                    break;
                case AttackType.Ranged:
                    result = (verb is Verb_LaunchProjectile);
                    break;
                case AttackType.Both:
                    result = false;
                    break;
                default:
                    result = true;
                    break;
            }
            return result;
        }

        public float energy;

        public bool useEnergy=true;

        protected int ticksTillReset;

        protected Sustainer sustainer;

        protected Vector3 impactAngleVect;
    }
}
