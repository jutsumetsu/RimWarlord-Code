﻿using Electromagnetic.Core;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Electromagnetic.Abilities
{
    public class CompAbilityEffect_GiveTargetHediff : CompAbilityEffect_WithDuration
    {
        //绑定Properties
        public new CompProperties_AbilityGiveTargetHediff Props
        {
            get
            {
                return (CompProperties_AbilityGiveTargetHediff)this.props;
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
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (this.Props.ignoreSelf && target.Pawn == this.parent.pawn)
            {
                return;
            }
            if (!this.Props.onlyApplyToSelf && this.Props.applyToTarget)
            {
                this.ApplyInner(target.Pawn, this.parent.pawn);
            }
            if (this.Props.applyToSelf || this.Props.onlyApplyToSelf)
            {
                this.ApplyInner(this.parent.pawn, target.Pawn);
            }
        }
        protected void ApplyInner(Pawn target, Pawn other)
        {
            if (target != null)
            {
                if (this.TryResist(target))
                {
                    MoteMaker.ThrowText(target.DrawPos, target.Map, "Resisted".Translate(), -1f);
                    return;
                }
                //替换现有Hediff
                if (this.Props.replaceExisting)
                {
                    Hediff firstHediffOfDef = target.health.hediffSet.GetFirstHediffOfDef(this.Props.hediffDef, false);
                    if (firstHediffOfDef != null)
                    {
                        target.health.RemoveHediff(firstHediffOfDef);
                    }
                }
                Hediff hediff = HediffMaker.MakeHediff(this.Props.hediffDef, target, this.Props.onlyBrain ? target.health.hediffSet.GetBrain() : null);
                HediffComp_Disappears hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();
                if (hediffComp_Disappears != null)
                {
                    hediffComp_Disappears.ticksToDisappear = base.GetDurationSeconds(target).SecondsToTicks();
                }
                if (this.Props.severity >= 0f)
                {
                    hediff.Severity = this.Props.severity;
                }
                HediffComp_Link hediffComp_Link = hediff.TryGetComp<HediffComp_Link>();
                if (hediffComp_Link != null)
                {
                    hediffComp_Link.other = other;
                    hediffComp_Link.drawConnection = (target == this.parent.pawn);
                }
                //传入力量之源参数
                Hediff_TargetBase hediff1 = hediff as Hediff_TargetBase;
                hediff1.root = this.parent.pawn.GetPowerRoot();
                hediff1.mastery = this.Ability.mastery;
                target.health.AddHediff(hediff1, null, null, null);
            }
        }
        protected virtual bool TryResist(Pawn pawn)
        {
            return false;
        }
        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            return this.parent.pawn.Faction != Faction.OfPlayer && target.Pawn != null;
        }
    }
}
