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
    public class CompAbilityEffect_ThunderPunch : CompAbilityEffect
    {
        //绑定Properties
        public new CompProperties_AbilityThunderPunch Props
        {
            get
            {
                return (CompProperties_AbilityThunderPunch)this.props;
            }
        }
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
            int num = 40 + masteryOffset;
            if (Pawn.IsHaveRoot())
            {
                //伤害计算
                Hediff_RWrd_PowerRoot root = Pawn.GetRoot();
                num += root.energy.level;
                if (root.energy.IsUltimate)
                {
                    num += (int)Math.Floor(root.energy.PowerEnergy);
                }
                int acr = root.energy.AvailableCompleteRealm();
                int pff = root.energy.PowerFlowFactor();
                int multiplier = acr + pff;
                multiplier = (int)Math.Floor(multiplier / 2f);
                num *= multiplier;
            }
            pawn.TakeDamage(new DamageInfo(DamageDefOf.Flame, (float)num, 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null, true, true, QualityCategory.Normal, true));
        }
        private HashSet<Faction> affectedFactionCache = new HashSet<Faction>();
    }
}
