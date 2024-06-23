using System;
using System.Collections.Generic;
using Electromagnetic.Core;
using RimWorld;
using Verse;
using Verse.AI;

namespace Electromagnetic.Core
{
    internal class WorkGiver_RWrd_Building_TrainingSpot : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForDef(DefDatabase<ThingDef>.GetNamed("RWrd_Building_TrainingSpot", true));
            }
        }
        public override bool Prioritized
        {
            get
            {
                return true;
            }
        }
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            bool flag = !pawn.IsHaveRoot();
            bool result;
            if (flag)
            {
                result = false;
            }
            else
            {
                Hediff_RWrd_PowerRoot root = pawn.GetRoot();
                Building_Training building_Training = t as Building_Training;
                result = (building_Training != null && pawn.CanReserve(building_Training, 1, -1, null, forced));
            }
            return result;
        }
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return new Job(DefDatabase<JobDef>.GetNamed("RWrd_Specialized_Training", true), t);
        }
    }
}
