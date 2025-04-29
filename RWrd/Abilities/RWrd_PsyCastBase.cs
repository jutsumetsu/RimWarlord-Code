using Electromagnetic.Core;
using Electromagnetic.Electromagnetic.UI;
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
        public RWrd_PsyCastBase(Pawn pawn) : base(pawn)
        {
        }

        public RWrd_PsyCastBase(Pawn pawn, AbilityDef def) : base(pawn, def)
        {
        }

        public override bool CanCast
        {
            get
            {
                bool flag = !base.CanCast;
                return !flag;
            }
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
            else if (!(this.gizmo is Command_Electromagnetic))
            {
                this.gizmo = new Command_Electromagnetic(this, this.pawn);
            }
            yield return this.gizmo;
            yield break;
        }
        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest)
        {
            // 检查是否需要显示灵能效果
            bool showPsycastEffects = this.def.showPsycastEffects;
            if (showPsycastEffects)
            {
                // 检查是否有任何EffectComp具有Psycast属性
                bool flag = base.EffectComps.Any((CompAbilityEffect c) => c.Props.psychic);
                if (flag)
                {
                    //检查是否有区域效果
                    bool hasAreaOfEffect = this.def.HasAreaOfEffect;
                    if (hasAreaOfEffect)
                    {
                        // 如果有区域效果，显示区域效果的粒子效果并播放声音
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
                    // 如果没有灵能效果，但有区域效果并且可以使用区域效果来获取目标
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
    }
}
