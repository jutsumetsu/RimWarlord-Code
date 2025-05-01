using System;
using System.Collections.Generic;
using Electromagnetic.Core;
using RimWorld;
using Verse;

namespace Electromagnetic.Abilities
{
    public class CompAbilityEffect_ReduceEnergy : CompAbilityEffect_Electromagnetic
    {
        public new CompProperties_ReduceEnergy Props
        {
            get
            {
                return (CompProperties_ReduceEnergy)this.props;
            }
        }
        //能量消耗
        public float EnergyReduce
        {
            get
            {
                if (Props.reSA)
                {
                    return 0;
                }
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
            Hediff_RWrd_PowerRoot hediff_RWrd_PowerRoot = this.parent.pawn.GetPowerRoot();
            this.root = hediff_RWrd_PowerRoot;
            this.Initialized = true;
        }
        //减少能量并增加经验
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (Caster.IsHavePowerRoot())
            {
                Hediff_RWrd_PowerRoot root = Caster.GetPowerRoot();
                root.energy.SetEnergy((int)Math.Floor(this.EnergyReduce * Ability.outputPower));
                root.energy.SetExp(0.1f * -(float)this.Props.rEnergy);
                if (!Props.masterySA)
                {
                    ((RWrd_PsyCastBase)this.parent).SetMastery(0.2f);
                }
                root.energy.SetCompleteRealm(0.000001f);
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
                bool flag3 = !this.parent.pawn.IsHavePowerRoot();
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
            if (this.parent.pawn.IsHavePowerRoot())
            {
                Hediff_RWrd_PowerRoot root = this.parent.pawn.GetPowerRoot();
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
                try
                {
                    bool flag = false;
                    if (this.root.SelfDestruction)
                    {
                        flag = !this.Props.isEx;
                    }
                    else
                    {
                        if (this.root.abilitySets.Count > 0 && root.abilitysetIndex < this.root.abilitySets.Count)
                        {
                            flag = !this.root.abilitySets[root.abilitysetIndex].Abilities.Contains(this.Ability.def);
                        }
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
                catch (Exception ex)
                {
                    Log.Error($"初始化异常: {ex.Message}\n堆栈跟踪: {ex.StackTrace}");
                    return true;
                }
            }
        }
        public bool disabled = true;
        public Hediff_RWrd_PowerRoot root;
        private int tick = 0;
        private bool Initialized = false;
    }
}
