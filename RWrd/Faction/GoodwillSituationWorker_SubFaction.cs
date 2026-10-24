using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Electromagnetic.ModExtension;

namespace Electromagnetic.Faction
{
    public class GoodwillSituationWorker_SubFaction : GoodwillSituationWorker
    {
        public override int GetNaturalGoodwillOffset(RimWorld.Faction other)
        {
            if (!Apply(other))
            {
                return 0;
            }
            return this.def.naturalGoodwillOffset;
        }
        public bool Apply(RimWorld.Faction other)
        {
            bool result = false;
            if (RimWorld.Faction.OfPlayer.def.HasModExtension<RelationCustom>())
            {
                if (RimWorld.Faction.OfPlayer.def.GetModExtension<RelationCustom>().overlord == other.def)
                {
                    result = true;
                }
            }
            return result;
        }
    }
}
