using Electromagnetic.Abilities;
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
    public class JobGiver_EMApparelRestock : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn == null || !pawn.Spawned || pawn.Map == null) return null;
            if (pawn.Downed || pawn.InMentalState || pawn.Drafted) return null;
            if (pawn.mindState?.enemyTarget != null) return null;
            if (!pawn.IsHavePowerRoot()) return null;

            Hediff_RWrd_PowerRoot root = pawn.GetPowerRoot();
            if (root.energy.AvailableLevel < 75) return null;
            Ability atomSplit = pawn.abilities.GetAbility(RWrd_DefOf.RWrd_Ability_AtomSplit);
            if (atomSplit == null) return null;

            HediffComp_ApparelRestock comp = root.GetComp<HediffComp_ApparelRestock>();
            if (comp == null || comp.pending.Count == 0) return null;
            if (!comp.HasAffordable(pawn, atomSplit as RWrd_AbilityBase)) return null;

            return new Job(DefDatabase<JobDef>.GetNamed("RWrd_ApparelRestock", true));
        }
    }
}
