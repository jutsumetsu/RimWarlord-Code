using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
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
        // 获取道途最大等级
        public int MaxLevel
        {
            get
            {
                if (routeNodes != null && routeNodes.Count > 0)
                {
                    return routeNodes.Max(node => node.requiredLevel);
                }
                return 0; 
            }
        }

        public override void PostLoad()
        {
            base.PostLoad();
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                bool flag = !this.background.NullOrEmpty();
                if (flag)
                {
                    this.backgroundImage = ContentFinder<Texture2D>.Get(this.background, true);
                }               
                bool flag3 = this.width > 0 && this.height > 0;
                if (flag3)
                {
                    Texture2D texture2D = new Texture2D(this.width, this.height);
                    texture2D.Apply();
                    bool flag4 = this.backgroundImage == null;
                    if (flag4)
                    {
                        this.backgroundImage = texture2D;
                    }                    
                }
            });
        }

        public List<RWrd_RouteNode> routeNodes = new List<RWrd_RouteNode>();
        private List<AbilityDef> cachedAbilities;
        private Dictionary<int, List<AbilityDef>> nodes = new Dictionary<int, List<AbilityDef>>();
        public string background;
        [Unsaved(false)]
        public Texture2D backgroundImage;
        public int width;
        public int height;
    }
}
