using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Electromagnetic.Core
{
    public class JobGiver_EMTraining : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            Need need = pawn.needs.TryGetNeed<Need_Training>();
            IntVec3 intVec = AIUtility.FindRandomSpotOutsideColony(pawn, 16f, false, false);
            if (!intVec.IsValid)
            {
                return null;
            }
            IntVec3 invalid = IntVec3.Invalid;
            if (!AIUtility.FindAroundSpotFromTarget(pawn, intVec, 4f, 3f, true, true, true).TryRandomElement(out invalid))
            {
                return null;
            }
            if (!pawn.IsHaveRoot())
            {
                return null;
            }
            if (need.CurLevel >= 0.99f)
            {
                return null;
            }
            else
            {
                return new Job(DefDatabase<JobDef>.GetNamed("RWrd_General_Training", true), invalid);
            }
        }
    }
}
