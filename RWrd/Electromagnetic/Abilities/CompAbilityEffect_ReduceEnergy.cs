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
        //隐藏按钮
        public override bool ShouldHideGizmo
        {
            get
            {
                return this.disabled;
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
                    ((Hediff_RWrd_PowerRoot)hediff).energy.SetEnergy((float)this.Props.rEnergy);
                    ((Hediff_RWrd_PowerRoot)hediff).energy.SetExp(0.3f * (float)(-(float)this.Props.rEnergy));
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
                this.Initialize(this.props);
            }
            bool flag2 = this.tick % 2000 == 0;
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

        //隐藏技能按钮
        public override bool GizmoDisabled(out string reason)
        {
            bool shouldHideGizmo = this.ShouldHideGizmo;
            bool result;
            bool flag = this.parent.pawn.GetRoot() != null;
            if (shouldHideGizmo)
            {
                reason = "力量不足，无法使用技能";
                result = true;
            }
            else
            {
                reason = null;
                result = false;
            }
            return result;
        }

        public bool disabled = true;
        public Hediff_RWrd_PowerRoot root;
        private int tick = 0;
        private bool Initialized = false;
    }
}
