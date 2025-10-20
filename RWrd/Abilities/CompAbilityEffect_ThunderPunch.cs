using Electromagnetic.Core;
using RimWorld;
using RimWorld.Planet;
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
    public class CompAbilityEffect_ThunderPunch : CompAbilityEffect_Electromagnetic
    {
        //特效组
        public virtual FleckDef[] EffectSet
        {
            get
            {
                return new FleckDef[]
                {
                    RWrd_DefOf.RWrd_ThunderPunchFleck
                };
            }
        }
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Map map = this.parent.pawn.Map;
            Pawn pawn = target.Pawn;
            FleckDef[] effectSet = this.EffectSet;
            Vector3 vector = target.Cell.ToVector3();
            vector.z += 0.5f;
            //生成打击特效
            FleckCreationData dataStatic = FleckMaker.GetDataStatic(vector, map, effectSet[0], 2f);
            dataStatic.rotation = (float)Tools.PointsAngleTool(this.parent.pawn.Position, pawn.Position);
            map.flecks.CreateFleck(dataStatic);
            SoundInfo info = SoundInfo.InMap(new TargetInfo(pawn.Position, map, false), MaintenanceType.None);
            SoundDefOf.Pawn_Melee_Punch_HitPawn.PlayOneShot(info);
            int masteryOffset = (int)Math.Floor(this.Ability.mastery / 10f);
            float num = 40;
            if (Caster.IsHavePowerRoot())
            {
                //伤害计算
                Hediff_RWrd_PowerRoot root = Caster.GetPowerRoot();
                num = Tools.FinalDamage(root, num, masteryOffset);
                num *= Ability.outputPower;
            }
            pawn.TakeDamage(new DamageInfo(DamageDefOf.Flame, num, 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null, true, true, QualityCategory.Normal, true));
        }
    }
}
