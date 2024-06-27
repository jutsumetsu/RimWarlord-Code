using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Electromagnetic.Core
{
    public class RWrd_RouteLevel
    {
        public void ResolveReferences()
        {
            bool flag = this.level != 0 || this.levelDef != null;
            if (flag)
            {
                bool flag2 = this.level == 0;
                if (flag2)
                {
                    this.level = this.levelDef.level;
                }
                else
                {
                    bool flag3 = this.levelDef == null;
                    if (flag3)
                    {
                        this.levelDef = DefDatabase<RimWarlordDef>.AllDefsListForReading.Find((RimWarlordDef rwrd) => rwrd.level == this.level);
                    }
                }
            }
            bool flag4 = !this.abilities.Empty<AbilityDef>() || !this.abilityList.NullOrEmpty();
            if (flag4)
            {
                bool flag5 = this.abilities.Empty<AbilityDef>();
                if (flag5)
                {
                    foreach (string defName in this.abilityList.Split(new char[]
                    {
                        ','
                    }))
                    {
                        AbilityDef named = DefDatabase<AbilityDef>.GetNamed(defName, true);
                        bool flag6 = named != null;
                        if (flag6)
                        {
                            this.abilities.Add(named);
                        }
                    }
                }
            }
        }
        public IEnumerable<string> ConfigErrors()
        {
            bool flag = this.abilities == null && this.abilityList.NullOrEmpty();
            if (flag)
            {
                yield return "No Abilities Found For Level " + this.level.ToString();
            }
            yield break;
        }
        public int level;

        public RimWarlordDef levelDef;

        public List<AbilityDef> abilities = new List<AbilityDef>();

        public string abilityList;
    }
}
