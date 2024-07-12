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
        public new CompProperties_AbilityElectricClaw Props
        {
            get
            {
                return (CompProperties_AbilityElectricClaw)this.props;
            }
        }
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
            FleckCreationData dataStatic = FleckMaker.GetDataStatic(this.parent.pawn.DrawPos, map, effectSet[0], 3f);
            float tick = Find.TickManager.TicksGame;
            dataStatic.rotation = (float)CompAbilityEffect_ElectricClaw.PointsAngleTool(this.parent.pawn.Position, pawn.Position);
            map.flecks.CreateFleck(dataStatic);
            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, false);
            pawn.stances.stunner.StunFor(60, this.parent.pawn, false, false);
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
            pawn.stances.stunner.StunFor(60, this.parent.pawn, false, false);
            Hediff hediff = HediffMaker.MakeHediff(RWrd_DefOf.RWrd_ElectricInternalEnergy, pawn, null);
            HediffComp_Disappears hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();
            hediffComp_Disappears.ticksToDisappear = 120;
            Hediff_TargetBase hediff1 = hediff as Hediff_TargetBase;
            hediff1.root = this.parent.pawn.GetRoot();
            pawn.health.AddHediff(hediff1, null, null, null);
        }
        public static double PointsAngleTool(IntVec3 p1, IntVec3 p2)
        {
            return Math.Atan2((double)(p2.x - p1.x), (double)(p2.z - p1.z)) * 180.0 / 3.141592653589793;
        }
    }
}
