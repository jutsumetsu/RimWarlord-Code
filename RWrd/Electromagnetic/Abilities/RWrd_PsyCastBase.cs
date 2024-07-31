using Electromagnetic.Core;
using Electromagnetic.UI;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Electromagnetic.Abilities
{
    public class RWrd_PsyCastBase : RWrd_PsycastOrigin
    {
        public override bool CanCast
        {
            get
            {
                bool flag = !base.CanCast;
                return !flag;
            }
        }
        public RWrd_PsyCastBase(Pawn pawn) : base(pawn)
        {
        }
        public RWrd_PsyCastBase(Pawn pawn, AbilityDef def) : base(pawn, def)
        {
        }
        /// <summary>
        /// 获取Gizmo
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<Command> GetGizmos()
        {
            bool flag = this.gizmo == null;
            if (flag)
            {
                this.gizmo = new Command_Electromagnetic(this, this.pawn);
            }
            yield return this.gizmo;
            yield break;
        }
        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            bool showPsycastEffects = this.def.showPsycastEffects;
            if (showPsycastEffects)
            {
                bool flag = base.EffectComps.Any((CompAbilityEffect c) => c.Props.psychic);
                if (flag)
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
                    bool flag2 = this.def.HasAreaOfEffect && this.def.canUseAoeToGetTargets;
                    if (flag2)
                    {
                        SoundDefOf.Psycast_Skip_Pulse.PlayOneShot(new TargetInfo(target.Cell, this.pawn.Map, false));
                    }
                }
            }
            return base.Activate(target, dest);
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
            //修罗道
            if (target.Pawn !=  null)
            {
                //检测黑暗轮回类Hediff
                if (target.Pawn.health.hediffSet.GetFirstHediff<Hediff_DarkReincarnation>() != null)
                {
                    Log.Message("Target has Dark Reincarnation Hediff");
                    if (!target.Pawn.abilities.abilities.Any(a => a.def == this.def))
                    {
                        target.Pawn.abilities.GainAbility(this.def);
                        RWrd_PsyCastBase ability = (RWrd_PsyCastBase)target.Pawn.abilities.GetAbility(this.def);
                        ability.mastery = -100;
                        CompAbilityEffect_ReduceEnergy compAbilityEffect_ReduceEnergy = ability.CompOfType<CompAbilityEffect_ReduceEnergy>();
                        compAbilityEffect_ReduceEnergy.Props.isAshura = true;
                    }
                    else
                    {
                        Hediff_DarkReincarnation hediff = target.Pawn.health.hediffSet.GetFirstHediff<Hediff_DarkReincarnation>();
                        RWrd_PsyCastBase ability = (RWrd_PsyCastBase)target.Pawn.abilities.GetAbility(this.def);
                        float num = 0.1f * (float)Math.Ceiling(hediff.mastery / 10f);
                        ability.mastery += num;
                    }
                }
            }
        }
        public new bool CanApplyPsycastTo(LocalTargetInfo target)
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
        public new void AbilityTick()
        {
            this.AbilityTick();
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref this.mastery, "mastery", 0f, false);
        }
        /// <summary>
        /// 设置精通值
        /// </summary>
        /// <param name="num">数值</param>
        public void SetMastery(float num)
        {
            if (num > 0)
            {
                float num2 = this.mastery + num;
                this.mastery = (num2 > this.MaxMastery ? this.MaxMastery : num2);
            }
        }
        public float mastery = 0;
        public float MaxMastery = 100;
    }
}
