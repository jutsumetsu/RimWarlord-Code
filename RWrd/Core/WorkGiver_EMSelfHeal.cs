using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;

namespace Electromagnetic.Core
{
    public class WorkGiver_EMSelfHeal : WorkGiver_TendSelf
    {
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (pawn != t || pawn.playerSettings == null || !pawn.playerSettings.selfTend)
            {
                return false;
            }
            if (!HealthAIUtility.ShouldEverReceiveMedicalCareFromPlayer(pawn))
            {
                return false;
            }
            if (!pawn.CanReserve(pawn, 1, -1, null, forced))
            {
                return false;
            }
            if (!pawn.HasRWrdTreatableCondition())
            {
                return false;
            }
            return pawn.GetRWrdSelfHealAbility() != null;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Job job = pawn.TryMakeRWrdSelfHealJob();
            if (job != null) return job;
            return base.JobOnThing(pawn, t, forced);
        }
    }
}
