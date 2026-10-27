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
    public class ThoughtWorker_Precept_EMPowerPresent : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            if (!ModsConfig.IdeologyActive) return ThoughtState.Inactive;
            foreach (Pawn pawn in p.MapHeld.mapPawns.AllPawnsSpawned)
            {
                if (p != pawn && pawn.IsHavePowerRoot() && (p.Faction != null && pawn.Faction != null && p.Faction == pawn.Faction))
                {
                    return ThoughtState.ActiveDefault;
                }
            }
            return ThoughtState.Inactive;
        }
    }
}
