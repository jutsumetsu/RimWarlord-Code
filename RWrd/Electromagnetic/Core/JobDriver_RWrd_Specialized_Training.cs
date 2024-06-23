using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Electromagnetic.Core
{
    internal class JobDriver_RWrd_Specialized_Training : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = this.pawn;
            LocalTargetInfo targetA = this.job.targetA;
            Job job = this.job;
            return pawn.Reserve(targetA, job, 1, -1, null, errorOnFailed, false);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Hediff_RWrd_PowerRoot root = this.pawn.GetRoot();
            int tick = this.ticktime;
            int counter = this.TrainingCounter;
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            this.FailOnThingHavingDesignation(TargetIndex.A, DesignationDefOf.Uninstall);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell, false);
            Toil work = new Toil();
            work.tickAction = delegate ()
            {
                bool canFunction = this.CanFunction;
                if (canFunction)
                {
                    Pawn actor = work.actor;
                    tick--;
                    if (tick <= 0)
                    {
                        if (root != null)
                        {
                            root.energy.SetExp(100);
                            root.energy.SetCompleteRealm(0.001f);
                            counter++;
                            if (counter >= 10)
                            {
                                canFunction = false;
                            }
                            else
                            {
                                canFunction = true;
                            }
                        }
                        tick = this.ticktime;
                    }
                }
                else
                {
                    work.actor.jobs.EndCurrentJob(JobCondition.Succeeded, true, true);
                }
            };
            work.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            work.defaultCompleteMode = ToilCompleteMode.Delay;
            work.defaultDuration = 4000;
            yield return work;
            yield break;
        }

        private int ticktime = 300;
        private bool CanFunction = true;
        private int TrainingCounter = 0;
    }
}
