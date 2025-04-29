using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Electromagnetic.Abilities
{
    public class CompProperties_SpawnMoteSinging : CompProperties_AbilityEffect
    {
        public CompProperties_SpawnMoteSinging()
        {
            this.compClass = typeof(CompAbilityEffect_SpawnMoteSinging);
        }
        public ThingDef moteCastDef;
    }
}
