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
            if (patient == null || patient == pawn)
            {
                return false;
            }
            if (def.emergency && !HealthAIUtility.ShouldBeTendedNowByPlayerUrgent(patient))
            {
                return false;
            }
            if (!WorkGiver_Tend.GoodLayingStatusForTend(patient, pawn))
            {
                return false;
            }
            if (!HealthAIUtility.ShouldEverReceiveMedicalCareFromPlayer(patient))
            {
                return false;
            }
            if (!pawn.CanReserve(patient, 1, -1, null, forced))
            {
                return false;
            }
            if (patient.IsMutant && !patient.mutant.Def.entitledToMedicalCare)
            {
                return false;
            }
            if (patient.InAggroMentalState && !patient.health.hediffSet.HasHediff(HediffDefOf.Scaria))
            {
                return false;
            }
            if (!patient.HasRWrdTreatableCondition())
            {
                return false;
            }
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