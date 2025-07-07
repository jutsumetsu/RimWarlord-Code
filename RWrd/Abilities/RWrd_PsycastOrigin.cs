using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace Electromagnetic.Abilities
{
    public class RWrd_PsycastOrigin : RWrd_AbilityBase
    {
        public RWrd_PsycastOrigin(Pawn pawn) : base(pawn)
        {
        }
        public RWrd_PsycastOrigin(Pawn pawn, AbilityDef def) : base(pawn, def) { }

        public override AcceptanceReport CanCast
        {
            get
            {
                bool flag = !base.CanCast;
                return !flag;
            }
        }
        public override IEnumerable<Command> GetGizmos()
        {
            bool flag = this.gizmo == null;
            if (flag)
            {
                this.gizmo = new Command_Ability(this, this.pawn);
            }
            yield return this.gizmo;
            yield break;
        }
        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            float num = base.FinalPsyfocusCost(target);
            bool flag = num > float.Epsilon;
            if (flag)
            {
                this.pawn.psychicEntropy.OffsetPsyfocusDirectly(0f - num);
            }
            bool showPsycastEffects = this.def.showPsycastEffects;
            if (showPsycastEffects)
            {
                bool flag2 = base.EffectComps.Any((CompAbilityEffect c) => c.Props.psychic);
                if (flag2)
                {
                    bool hasAreaOfEffect = this.def.HasAreaOfEffect;
                    if (hasAreaOfEffect)
                    {
                        FleckMaker.Static(target.Cell, this.pawn.Map, FleckDefOf.PsycastAreaEffect, this.def.EffectRadius);
                        SoundDefOf.PsycastPsychicPulse.PlayOneShot(new TargetInfo(target.Cell, this.pawn.Map, false));
                    }
                    else
                    {
                        SoundDefOf.PsycastPsychicEffect.PlayOneShot(new TargetInfo(target.Cell, this.pawn.Map, false));
                    }
                }
                else
                {
                    bool flag3 = this.def.HasAreaOfEffect && this.def.canUseAoeToGetTargets;
                    if (flag3)
                    {
                        SoundDefOf.Psycast_Skip_Pulse.PlayOneShot(new TargetInfo(target.Cell, this.pawn.Map, false));
                    }
                }
            }
            return base.Activate(target, dest);
        }
        public override bool Activate(GlobalTargetInfo target)
        {
            float psyfocusCost = this.def.PsyfocusCost;
            bool flag = psyfocusCost > float.Epsilon;
            if (flag)
            {
                this.pawn.psychicEntropy.OffsetPsyfocusDirectly(0f - psyfocusCost);
            }
            return base.Activate(target);
        }
        protected override void ApplyEffects(IEnumerable<CompAbilityEffect> effects, LocalTargetInfo target, LocalTargetInfo dest)
        {
            bool flag = this.CanApplyPsycastTo(target);
            if (flag)
            {
                foreach (CompAbilityEffect compAbilityEffect in effects)
                {
                    compAbilityEffect.Apply(target, dest);
                }
            }
            else
            {
                MoteMaker.ThrowText(target.CenterVector3, this.pawn.Map, "TextMote_Immune".Translate(), -1f);
            }
        }
        public bool CanApplyPsycastTo(LocalTargetInfo target)
        {
            bool flag = !base.EffectComps.Any((CompAbilityEffect e) => e.Props.psychic);
            bool result;
            if (flag)
            {
                result = true;
            }
            else
            {
                Pawn pawn = target.Pawn;
                bool flag2 = pawn != null;
                if (flag2)
                {
                    bool flag3;
                    if (pawn.Faction != null && pawn.Faction == Faction.OfMechanoids)
                    {
                        flag3 = base.EffectComps.Any((CompAbilityEffect e) => !e.Props.applicableToMechs);
                    }
                    else
                    {
                        flag3 = false;
                    }
                    bool flag4 = flag3;
                    if (flag4)
                    {
                        return false;
                    }
                }
                result = true;
            }
            return result;
        }
        public override bool GizmoDisabled(out string reason)
        {
            bool flag = !this.CanCast;
            if (flag)
            {
                bool canCooldown = base.CanCooldown;
                if (canCooldown)
                {
                    reason = "AbilityOnCooldown".Translate(base.CooldownTicksRemaining.ToStringTicksToPeriod(true, false, true, true, false)).Resolve();
                    return true;
                }
                bool usesCharges = base.UsesCharges;
                if (usesCharges)
                {
                    reason = "AbilityNoCharges".Translate();
                    return true;
                }
            }
            bool flag2 = !this.pawn.Drafted && this.def.disableGizmoWhileUndrafted && this.pawn.GetCaravan() == null && !DebugSettings.ShowDevGizmos;
            bool result;
            if (flag2)
            {
                reason = "AbilityDisabledUndrafted".Translate();
                result = true;
            }
            else
            {
                bool flag3 = this.pawn.DevelopmentalStage.Baby();
                if (flag3)
                {
                    reason = "IsIncapped".Translate(this.pawn.LabelShort, this.pawn);
                    result = true;
                }
                else
                {
                    bool downed = this.pawn.Downed;
                    if (downed)
                    {
                        reason = "CommandDisabledUnconscious".TranslateWithBackup("CommandCallRoyalAidUnconscious").Formatted(this.pawn);
                        result = true;
                    }
                    else
                    {
                        bool deathresting = this.pawn.Deathresting;
                        if (deathresting)
                        {
                            reason = "CommandDisabledDeathresting".Translate(this.pawn);
                            result = true;
                        }
                        else
                        {
                            bool flag4 = this.def.hostile && this.pawn.WorkTagIsDisabled(WorkTags.Violent);
                            if (flag4)
                            {
                                reason = "IsIncapableOfViolence".Translate(this.pawn.LabelShort, this.pawn);
                                result = true;
                            }
                            else
                            {
                                bool flag5 = !this.comps.NullOrEmpty<AbilityComp>();
                                if (flag5)
                                {
                                    for (int i = 0; i < this.comps.Count; i++)
                                    {
                                        bool flag6 = this.comps[i].GizmoDisabled(out reason);
                                        if (flag6)
                                        {
                                            return true;
                                        }
                                    }
                                }
                                Lord lord;
                                bool flag7 = (lord = this.pawn.GetLord()) != null;
                                if (flag7)
                                {
                                    AcceptanceReport report = lord.AbilityAllowed(this);
                                    bool flag8 = !report;
                                    if (flag8)
                                    {
                                        reason = report.Reason;
                                        return true;
                                    }
                                }
                                reason = null;
                                result = false;
                            }
                        }
                    }
                }
            }
            return result;
        }
    }
}
