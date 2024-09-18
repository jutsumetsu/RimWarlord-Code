using Electromagnetic.Core;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;
using Verse.Sound;

namespace Electromagnetic.Abilities
{
    public class CompAbilityEffect_TigerSharkExplosionPunch : CompAbilityEffect
    {
        //绑定Properties
        public new CompProperties_AbilityTigerSharkExplosionPunch Props
        {
            get
            {
                return (CompProperties_AbilityTigerSharkExplosionPunch)this.props;
            }
        }
        private Pawn Pawn
        {
            get
            {
                return this.parent.pawn;
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
            Map map = this.parent.pawn.Map;
            Pawn pawn = target.Pawn;
            SoundInfo info = SoundInfo.InMap(new TargetInfo(pawn.Position, map, false), MaintenanceType.None);
            SoundDefOf.Pawn_Melee_Punch_HitPawn.PlayOneShot(info);
            int masteryOffset = (int)Math.Floor(this.Ability.mastery / 10f);
            float num = 20;
            if (Pawn.IsHavePowerRoot())
            {
                //伤害计算
                Hediff_RWrd_PowerRoot root = Pawn.GetPowerRoot();
                num = Tools.FinalDamage(root, num, masteryOffset);
                num *= Ability.outputPower;
            }
            pawn.TakeDamage(new DamageInfo(DamageDefOf.Blunt, (float)num, 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null, true, true, QualityCategory.Normal, true));
            //赋予目标爆破劲力Hediff
            Hediff hediff = HediffMaker.MakeHediff(RWrd_DefOf.RWrd_ExplosiveEnergy, pawn, null);
            HediffComp_Disappears hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();
            hediffComp_Disappears.ticksToDisappear = 60;
            Hediff_ExplosiveEnergy hediff1 = hediff as Hediff_ExplosiveEnergy;
            hediff1.root = this.parent.pawn.GetPowerRoot();
            hediff1.outputPower = Ability.outputPower;
            int num2 = (int)Math.Ceiling(this.Ability.mastery / 40f);
            hediff1.Severity = 0.1f * num2;
            hediff1.damage *= num2;
            pawn.health.AddHediff(hediff1, null, null, null);
        }
    }
}
