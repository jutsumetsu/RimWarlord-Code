using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Electromagnetic.Core
{
    public class RWrd_RouteDef : Def
    {
        public override void ResolveReferences()
        {
            base.ResolveReferences();
            foreach (RWrd_RouteLevel ri_RouteLevel in this.routeLevels)
            {
                ri_RouteLevel.ResolveReferences();
                this.levels[ri_RouteLevel.level] = ri_RouteLevel.abilities;
            }
        }
        public override IEnumerable<string> ConfigErrors()
        {
            bool flag = this.routeLevels.Count <= 0;
            if (flag)
            {
                yield return "No route level found for route " + this.defName;
            }
            foreach (RWrd_RouteLevel level in this.routeLevels)
            {
                foreach (string error in level.ConfigErrors())
                {
                    yield return error;
                }
            }
            yield break;
        }
        public bool HasAbility(AbilityDef def)
        {
            return this.AllAbilities.Contains(def);
        }
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
        public List<AbilityDef> this[int lv]
        {
            get
            {
                List<AbilityDef> list;
                bool flag = this.levels.TryGetValue(lv, out list);
                List<AbilityDef> result;
                if (flag)
                {
                    result = list;
                }
                else
                {
                    result = null;
                }
                return result;
            }
        }
        public List<RWrd_RouteLevel> routeLevels = new List<RWrd_RouteLevel>();
        private List<AbilityDef> cachedAbilities;
        private Dictionary<int, List<AbilityDef>> levels = new Dictionary<int, List<AbilityDef>>();
    }
}
