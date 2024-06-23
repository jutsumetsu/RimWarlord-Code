using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Electromagnetic.Core
{
    public class RWrd_RouteDef : Def
    {
        //所有技能
        public List<AbilityDef> AllAbilities
        {
            get
            {
                bool flag = this.cachedAbilities == null;
                if (flag)
                {
                    this.cachedAbilities = new List<AbilityDef>();
                    foreach (List<AbilityDef> collection in this.levels.Values)
                    {
                        this.cachedAbilities.AddRange(collection);
                    }
                }
                return this.cachedAbilities;
            }
        }
        public List<RWrd_RouteLevel> routeLevels = new List<RWrd_RouteLevel>();
        private List<AbilityDef> cachedAbilities;
        private Dictionary<int, List<AbilityDef>> levels = new Dictionary<int, List<AbilityDef>>();
    }
}
