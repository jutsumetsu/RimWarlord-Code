using System;
using System.Collections.Generic;
using Electromagnetic.Core;
using RimWorld;
using Verse;

namespace Electromagnetic.Abilities
{
    public class CompAbilityEffect_ReduceEnergy : CompAbilityEffect
    {
        public new CompProperties_ReduceEnergy Props
        {
            get
            {
                return (CompProperties_ReduceEnergy)this.props;
            }
        }
        //技能接口
        private RWrd_PsyCastBase Ability
        {
            get
            {
                return (RWrd_PsyCastBase)this.parent;
            }
        }
        //能量消耗
        public float EnergyReduce
        {
            get
            {
                float rEnergy = (float)this.Props.rEnergy;
                float mastery = (float)this.Ability.mastery;
                float offset = (float)this.Props.masteryOffset;
                float factor = (float)this.Props.masteryFactor;

                if (offset != 0 && factor == 0)
                {
                    return rEnergy + (float)Math.Floor(mastery * offset);
                }
                else if (offset == 0 && factor != 0)
                {
                    return rEnergy * (float)Math.Floor(mastery * factor);
                }
                else if (offset != 0 || factor != 0)
                {
                    Log.Error("masteryOffset and masteryFactor cannot exist at the same time!");
                }

                return rEnergy;
            }
        }
        //初始化
        public override void Initialize(AbilityCompProperties props)
        {
            base.Initialize(props);
            Hediff_RWrd_PowerRoot hediff_RWrd_PowerRoot = this.parent.pawn.GetRoot();
            this.root = hediff_RWrd_PowerRoot;
            this.Initialized = true;
        }
        //减少能量并增加经验
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            foreach (Hediff hediff in this.parent.pawn.health.hediffSet.hediffs)
            { 
                bool flag = hediff.GetType() == typeof(Hediff_RWrd_PowerRoot);
                if (flag)
                {
                    ((Hediff_RWrd_PowerRoot)hediff).energy.SetEnergy((int)Math.Floor(this.EnergyReduce * Ability.outputPower));
                    ((Hediff_RWrd_PowerRoot)hediff).energy.SetExp(0.1f * -(float)this.Props.rEnergy);
                    ((RWrd_PsyCastBase)this.parent).SetMastery(0.2f);
                    ((Hediff_RWrd_PowerRoot)hediff).energy.SetCompleteRealm(0.000001f);
                    break;
                }
            }
        }


        public override void CompTick()
        {
            this.tick++;
            bool flag = !this.Initialized;
            if (flag)
            {
                //初始化
                this.Initialize(this.props);
            }
            bool flag2 = this.tick % 2000 == 0;
            //在失去力量后移除技能
            if (flag2)
            {
                bool flag3 = this.parent.pawn.GetRoot() == null;
                if (flag3)
                {
                    this.parent.pawn.abilities.RemoveAbility(this.parent.def);
                }
            }
            base.CompTick();
        }

        //禁用技能按钮
        public override bool GizmoDisabled(out string reason)
        {
            bool result;
            if (this.parent.pawn.IsHaveRoot())
            {
                Hediff_RWrd_PowerRoot root = this.parent.pawn.GetRoot();
                if (-(float)this.Props.rEnergy > root.energy.energy)
                {
                    result = true;
                    reason = "RWrd_NoEnergy".Translate() + ", " + "RWrd_CannotUse".Translate();
                }
                else
                {
                    result = false;
                    reason = null;
                }
            }
            else
            {
                result = true;
                reason = "RWrd_NoRoot".Translate() + ", " + "RWrd_CannotUse".Translate();
            }
            return result;
        }
        //隐藏技能按钮
        public override bool ShouldHideGizmo
        {
            get
            {
                bool flag = false;
                if (this.root.abilitySets.Count > 0 && root.abilitysetIndex < this.root.abilitySets.Count)
                {
                    flag = !this.root.abilitySets[root.abilitysetIndex].Abilities.Contains(Ability.def);
                }
                if (this.Ability.mastery < 0 || flag)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool disabled = true;
        public Hediff_RWrd_PowerRoot root;
        private int tick = 0;
        private bool Initialized = false;
    }
}
