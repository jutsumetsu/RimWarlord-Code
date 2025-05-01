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
    public class CompAbilityEffect_TigerSharkExplosionPunch : CompAbilityEffect_Electromagnetic
    {
        //绑定Properties
        public new CompProperties_AbilityTigerSharkExplosionPunch Props
        {
            get
            {
                return (CompProperties_AbilityTigerSharkExplosionPunch)this.props;
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
            //伤害计算
            Hediff_RWrd_PowerRoot root = Caster.GetPowerRoot();
            num = Tools.FinalDamage(root, num, masteryOffset);
            num *= Ability.outputPower;
            if (root.SelfDestruction)
            {
                DamageInfo dinfo = new DamageInfo(DamageDefOf.Bomb, (float)num * 10, 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null, true, true, QualityCategory.Normal, true);
                Tools.MillionHp(Caster, pawn, dinfo);
            }
            else
            {

                DamageInfo dinfo = new DamageInfo(DamageDefOf.Blunt, (float)num, 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null, true, true, QualityCategory.Normal, true);
                pawn.TakeDamage(dinfo);
                //赋予目标爆破劲力Hediff
                Hediff_ExplosiveEnergy hediff = (Hediff_ExplosiveEnergy)Tools.MakeEMHediff(RWrd_DefOf.RWrd_ExplosiveEnergy, pawn, this.parent.pawn.GetPowerRoot(), null);
                HediffComp_Disappears hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();
                hediffComp_Disappears.ticksToDisappear = 60;
                hediff.root = this.parent.pawn.GetPowerRoot();
                hediff.outputPower = Ability.outputPower;
                int num2 = (int)Math.Ceiling(this.Ability.mastery / 40f);
                hediff.Severity = 0.1f * num2;
                hediff.damage *= num2;
                pawn.health.AddHediff(hediff, null, null, null);
            }
        }
    }
}
