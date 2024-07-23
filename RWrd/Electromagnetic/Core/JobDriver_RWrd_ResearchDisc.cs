using Electromagnetic.Things;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Electromagnetic.Core
{
    public class JobDriver_RWrd_ResearchDisc : JobDriver_StudyItem
    {
        protected Thing StudyThing
        {
            get
            {
                return base.TargetThingA;
            }
        }
        protected CompThingStudiable StudyComp
        {
            get
            {
                return this.StudyThing.TryGetComp<CompThingStudiable>();
            }
        }
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            bool flag = this.pawn.Reserve(base.TargetThingA, this.job, 1, -1, null, errorOnFailed, false) && this.pawn.Reserve(base.TargetB, this.job, 1, -1, null, errorOnFailed, false);
            return flag && this.pawn.Reserve(base.TargetC, this.job, 1, -1, null, errorOnFailed, false);
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.B);
            this.FailOnBurningImmobile(TargetIndex.B);
            yield return Toils_General.DoAtomic(delegate
            {
                this.job.count = 1;
            });
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch, false).FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, true, false, true, false).FailOnDestroyedNullOrForbidden(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.InteractionCell, false);
            yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, null, false, false);
            yield return this.BookStudyToil();
            yield break;
        }
        private Toil BookStudyToil()
        {
            Toil study = ToilMaker.MakeToil("StudyToil");
            study.tickAction = delegate ()
            {
                Pawn actor = study.actor;
                float num = 0.08f;
                bool flag = !actor.WorkTypeIsDisabled(WorkTypeDefOf.Research);
                if (flag)
                {
                    num = actor.GetStatValue(StatDefOf.ResearchSpeed, true, -1);
                }
                num *= this.TargetThingA.GetStatValue(StatDefOf.ResearchSpeedFactor, true, -1);
                this.StudyComp.Study(num, actor);
                bool completed = this.StudyComp.Completed;
                if (completed)
                {
                    this.StudyComp.LearnAbilities(this.pawn);
                    if (this.StudyComp.Props.route != null)
                    {
                        Hediff_RWrd_PowerRoot root = this.pawn.GetRoot();
                        root.UnlockRoute(this.StudyComp.Props.route);
                    }
                    this.pawn.CheckAbilityLimiting();
                    this.pawn.jobs.curDriver.ReadyForNextToil();
                }
            };
            study.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            study.WithProgressBar(TargetIndex.A, () => this.StudyComp.ProgressPercent, false, -0.5f, false);
            study.defaultCompleteMode = ToilCompleteMode.Delay;
            study.defaultDuration = 4000;
            study.activeSkill = (() => SkillDefOf.Intellectual);
            return study;
        }
        private new const TargetIndex ResearchBenchInd = TargetIndex.B;
        private new const TargetIndex HaulCell = TargetIndex.C;
        protected const TargetIndex StudiableInd = TargetIndex.A;
        private const float DefaultResearchSpeed = 0.08f;
        private const int JobEndInterval = 4000;
    }
}
