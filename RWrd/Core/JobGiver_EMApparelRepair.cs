using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;
using Electromagnetic.Abilities;

namespace Electromagnetic.Core
{
    public class JobGiver_EMApparelRepair : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn == null || !pawn.Spawned || pawn.Map == null) return null;
            if (pawn.Downed || pawn.InMentalState || pawn.Drafted) return null;
            if (pawn.mindState?.enemyTarget != null) return null;
            if (!pawn.IsHavePowerRoot()) return null;
            if (!PowerRootUtillity.HasDamagedWornApparel(pawn)) return null;

            RWrd_AbilityBase ability = pawn.GetRWrdMatterHardeningAbility() as RWrd_AbilityBase;
            if (ability == null) return null;

            Hediff_RWrd_PowerRoot root = pawn.GetPowerRoot();
            int masteryOffset = (int)Math.Floor(ability.mastery / 10f);
            int level = root.energy.AvailableLevel + root.energy.FinalLevelOffset + 1 + masteryOffset;
            int num = (int)Math.Floor(level / 5f);
            if (root.energy.IsUltimate) num += (int)Math.Floor(root.energy.PowerEnergy);

            return ability.GetJob(new LocalTargetInfo(pawn), new LocalTargetInfo(pawn));
        }
    }
}
