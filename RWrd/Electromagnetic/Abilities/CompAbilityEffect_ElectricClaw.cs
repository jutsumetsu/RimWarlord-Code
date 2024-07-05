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
                    RWrd_DefOf.RWrd_ElectricClawAttractFleck,
                    RWrd_DefOf.RWrd_ElectricClawFleck
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
            pawn.stances.stunner.StunFor(120, this.parent.pawn, false, false);
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
            Hediff_RWrd_PowerRoot root = this.parent.pawn.GetRoot();
            float num = this.Props.damage + root.energy.CurrentDef.level;
            int acr = root.energy.AvailableCompleteRealm();
            int pff = root.energy.PowerFlowFactor();
            float multiplier = acr + pff;
            multiplier = (int)Math.Floor(multiplier / 2);
            num *= multiplier;
            FleckCreationData dataStatic2 = FleckMaker.GetDataStatic(target.Cell.ToVector3(), map, effectSet[1], 1f);
            map.flecks.CreateFleck(dataStatic2);
            pawn.TakeDamage(new DamageInfo(DamageDefOf.Flame, num, 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null, true, true, QualityCategory.Normal, true));
        }
        public static double PointsAngleTool(IntVec3 p1, IntVec3 p2)
        {
            return Math.Atan2((double)(p2.x - p1.x), (double)(p2.z - p1.z)) * 180.0 / 3.141592653589793;
        }
    }
}
