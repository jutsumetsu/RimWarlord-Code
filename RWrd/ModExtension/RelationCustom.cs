using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Electromagnetic.ModExtension
{
    public class RelationCustom : DefModExtension
    {
        public List<FactionDef> forcedAllies;
        public List<FactionDef> forcedEnemies;
        public FactionDef overlord;
    }
}
