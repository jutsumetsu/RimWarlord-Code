using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Electromagnetic.Core;
using Verse;

namespace Electromagnetic.Core
{
    public class ThoughtWorker_Precept_EMPowerPresentAlter : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            if (!ModsConfig.IdeologyActive) return ThoughtState.Inactive;
            foreach (Pawn pawn in p.MapHeld.mapPawns.AllPawnsSpawned)
            {
                if (pawn.IsHavePowerRoot() && !pawn.IsPrisonerOfColony && !pawn.IsSlaveOfColony && !pawn.IsColonist)
                {
                    return ThoughtState.ActiveDefault;
                }
            }
            return ThoughtState.Inactive;
        }
    }
}
