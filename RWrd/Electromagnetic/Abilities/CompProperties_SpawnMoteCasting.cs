using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RWrd.Electromagnetic.Abilities;
using Verse;

namespace Electromagnetic.Abilities
{
    public class CompProperties_SpawnMoteCasting : CompProperties_AbilityEffect
    {
        public CompProperties_SpawnMoteCasting()
        {
            this.compClass = typeof(CompAbilityEffect_SpawnMoteCasting);
        }
        public ThingDef moteCastDef;
    }
}
