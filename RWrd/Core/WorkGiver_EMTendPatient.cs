using RimWorld;
using Verse;
using Verse.AI;

namespace Electromagnetic.Core
{
    public class WorkGiver_EMTendPatient : WorkGiver_TendOther_Humanlike
    {
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Pawn patient = t as Pawn;
            if (patient == null || patient == pawn) return false;
            if (!base.HasJobOnThing(pawn, t, forced)) return false;
            return pawn.GetRWrdTendAbility(patient) != null;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Pawn patient = t as Pawn;
            Job job = patient != null ? pawn.TryMakeRWrdTendJob(patient) : null;
            if (job != null) return job;
            return base.JobOnThing(pawn, t, forced);
        }
    }
}