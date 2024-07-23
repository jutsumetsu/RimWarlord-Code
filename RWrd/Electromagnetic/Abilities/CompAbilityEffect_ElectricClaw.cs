using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using System.Text;
using System.Threading.Tasks;
using Electromagnetic.Core;
using Verse.AI;

namespace Electromagnetic.Abilities
{
    public class CompAbilityEffect_ElectricClaw : CompAbilityEffect
    {
        //绑定Properties
        public new CompProperties_AbilityElectricClaw Props
        {
            get
            {
                return (CompProperties_AbilityElectricClaw)this.props;
            }
        }
        //特效组
        public virtual FleckDef[] EffectSet
        {
            get
            {
                return new FleckDef[]
                {
                    RWrd_DefOf.RWrd_ElectricClawAttractFleck
                };
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
            Pawn pawn = (Pawn)((Thing)target);
            FleckDef[] effectSet = this.EffectSet;
            //生成技能释放特效
            FleckCreationData dataStatic = FleckMaker.GetDataStatic(this.parent.pawn.DrawPos, map, effectSet[0], 3f);
            dataStatic.rotation = (float)Tools.PointsAngleTool(this.parent.pawn.Position, pawn.Position);
            map.flecks.CreateFleck(dataStatic);
            //暂停目标动作并击晕
            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, false);
            pawn.stances.stunner.StunFor(60, this.parent.pawn, false, false);
            //判断电爪抓人方向并设置目标位置
            if (this.parent.pawn.Rotation == Rot4.North)
            {
                IntVec3 position = this.parent.pawn.Position;
                position.z += 1;
                pawn.Position = position;
            }
            else if (this.parent.pawn.Rotation == Rot4.West)
            {
                IntVec3 position = this.parent.pawn.Position;
                position.x -= 1;
                pawn.Position = position;
            }
            else if (this.parent.pawn.Rotation == Rot4.South)
            {
                IntVec3 position = this.parent.pawn.Position;
                position.z -= 1;
                pawn.Position = position;
            }
            else if (this.parent.pawn.Rotation == Rot4.West)
            {
                IntVec3 position = this.parent.pawn.Position;
                position.x += 1;
                pawn.Position = position;
            }
            else
            {
                IntVec3 position = this.parent.pawn.Position;
                pawn.Position = position;
            }
            //继续击晕并赋予目标电极内力Hediff
            pawn.stances.stunner.StunFor(60, this.parent.pawn, false, false);
            Hediff hediff = HediffMaker.MakeHediff(RWrd_DefOf.RWrd_ElectricInternalEnergy, pawn, null);
            HediffComp_Disappears hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();
            hediffComp_Disappears.ticksToDisappear = 120;
            Hediff_TargetBase hediff1 = hediff as Hediff_TargetBase;
            hediff1.root = this.parent.pawn.GetRoot();
            hediff1.mastery = this.Ability.mastery;
            pawn.health.AddHediff(hediff1, null, null, null);
        }
    }
}
