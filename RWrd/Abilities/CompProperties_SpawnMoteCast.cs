using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Electromagnetic.Abilities
{
    public class CompProperties_SpawnMoteCast : CompProperties_AbilityEffect
    {
        public CompProperties_SpawnMoteCast()
        {
            this.compClass = typeof(CompAbilityEffect_SpawnMoteCast);
        }
        public ThingDef moteCastDef;
    }
}
