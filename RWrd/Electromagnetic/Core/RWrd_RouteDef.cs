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
            foreach (RWrd_RouteNode rwrd_RouteNode in this.routeNodes)
            {
                this.nodes[rwrd_RouteNode.number] = rwrd_RouteNode.abilities;
            }
        }
        public override IEnumerable<string> ConfigErrors()
        {
            bool flag = this.routeNodes.Count <= 0;
            if (flag)
            {
                yield return "No route node found for route " + this.defName;
            }
            foreach (RWrd_RouteNode node in this.routeNodes)
            {
                foreach (string error in node.ConfigErrors())
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
                    foreach (List<AbilityDef> collection in this.nodes.Values)
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
                bool flag = this.nodes.TryGetValue(lv, out list);
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
        public List<RWrd_RouteNode> routeNodes = new List<RWrd_RouteNode>();
        private List<AbilityDef> cachedAbilities;
        private Dictionary<int, List<AbilityDef>> nodes = new Dictionary<int, List<AbilityDef>>();
    }
}
