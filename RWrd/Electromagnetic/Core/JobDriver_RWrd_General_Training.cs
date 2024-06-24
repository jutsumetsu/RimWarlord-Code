using System;
using System.Collections.Generic;
using HarmonyLib;
using System.Reflection;
using RimWorld;
using Verse;
using Verse.AI;

namespace Electromagnetic.Core
{
    internal class JobDriver_RWrd_General_Training : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = this.pawn;
            LocalTargetInfo target = this.job.GetTarget(TargetIndex.A);
            Job job = this.job;
            return ReservationUtility.Reserve(pawn, target, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            JitterHandler jitterer = this.fieldJitterer.GetValue(this.pawn.Drawer) as JitterHandler;
            Hediff_RWrd_PowerRoot root = this.pawn.GetRoot();
            Need need = this.pawn.needs.TryGetNeed<Need_Training>();
            int tick = this.ticktime;
            int counter = this.TrainingCounter;
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
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
                            int currentLevel = root.energy.currentRWrd.def.level;
                            if (currentLevel == 0)
                            {
                                root.energy.SetExp(200);
                                counter += 1;
                            }
                            else
                            {
                                root.energy.SetExp(25);
                                counter += 1;
                            }
                            root.energy.SetCompleteRealm(0.001f);
                            need.CurLevel += 0.1f;
                        }
                        tick = this.ticktime;
                        if (counter >= 10)
                        {
                            this.CanFunction = false;
                        }
                        else
                        {
                            this.CanFunction = true;
                        }
                    }
                    if (this.pawn.IsHashIntervalTick(100))
                    {
                        if (jitterer != null)
                        {
                            jitterer.AddOffset(Rand.Range(0.25f, 0.75f), this.pawn.Rotation.AsAngle);
                            if (Rand.Value > 0.7f)
                            {
                                work.handlingFacing = true;
                                this.pawn.Rotation = Rot4.Random;
                            }
                        }
                    }
                }
                else
                {
                    work.actor.jobs.EndCurrentJob(JobCondition.Succeeded, true, true);
                }
            };
            work.defaultCompleteMode = ToilCompleteMode.Delay;
            work.defaultDuration = 4000;
            yield return work;
            yield break;
        }

        private int ticktime = 2500;
        private bool CanFunction = true;
        private int TrainingCounter = 0;
        private FieldInfo fieldJitterer = AccessTools.Field(typeof(Pawn_DrawTracker), "jitterer");
    }
}
