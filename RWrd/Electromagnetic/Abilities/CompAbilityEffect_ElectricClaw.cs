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
    public class CompAbilityEffect_ElectricClaw : CompAbilityEffect_Electromagnetic
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
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Map map = this.parent.pawn.Map;
            Pawn pawn = (Pawn)((Thing)target);
            FleckDef[] effectSet = this.EffectSet;
            //生成技能释放特效
            FleckCreationData dataStatic = FleckMaker.GetDataStatic(this.parent.pawn.DrawPos, map, effectSet[0], 3f);
            IntVec3 position = this.parent.pawn.Position;
            float Rotation = (float)Tools.PointsAngleTool(position, pawn.Position);
            dataStatic.rotation = Rotation;
            map.flecks.CreateFleck(dataStatic);
            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, false);
            //判断电爪抓人方向并设置目标位置
            bool flag = 0 <= Rotation && Rotation <= 22.5;
            bool flag2 = -22.5 <= Rotation && Rotation < 0;
            if (flag || flag2)
            {
                position.z += 1;
            }
            else if (Rotation < -22.5 && Rotation >= -67.5)
            {
                position.z += 1;
                position.x -= 1;
            }
            else if (Rotation < -67.5 && Rotation >= -112.5)
            {
                position.x -= 1;
            }
            else if (Rotation < -112.5 && Rotation >= -157.5)
            {
                position.x -= 1;
                position.z -= 1;
            }
            else if (Rotation > 157.5 || Rotation < -157.5)
            {
                position.z -= 1;
            }
            else if (Rotation > 112.5 && Rotation <= 157.5)
            {
                position.z -= 1;
                position.x += 1;
            }
            else if (Rotation > 67.5 && Rotation <= 112.5)
            {
                position.x += 1;
            }
            else
            {
                position.x += 1;
                position.z += 1;
            }
            pawn.Position = position;
            pawn.Notify_Teleported(true, false);
            //继续击晕并赋予目标电极内力Hediff
            Hediff hediff = HediffMaker.MakeHediff(RWrd_DefOf.RWrd_ElectricInternalEnergy, pawn, null);
            HediffComp_Disappears hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();
            hediffComp_Disappears.ticksToDisappear = 120;
            Hediff_TargetBase hediff1 = hediff as Hediff_TargetBase;
            hediff1.root = this.parent.pawn.GetPowerRoot();
            hediff1.mastery = this.Ability.mastery;
            pawn.health.AddHediff(hediff1, null, null, null);
        }
    }
}
