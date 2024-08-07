﻿using System;
using System.Collections.Generic;
using HarmonyLib;
using System.Reflection;
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
            JitterHandler jitterer = this.fieldJitterer.GetValue(this.pawn.Drawer) as JitterHandler;
            Hediff_RWrd_PowerRoot root = this.pawn.GetRoot();
            Need need = this.pawn.needs.TryGetNeed<Need_Training>();
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
                            int currentLevel = root.energy.level;
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
                            root.energy.SetCompleteRealm(0.0003f);
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
            work.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
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
