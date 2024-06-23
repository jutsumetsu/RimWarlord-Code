using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Electromagnetic.Abilities
{
    public class CompProperties_ReduceEnergy : CompProperties_AbilityEffect
    {
        public CompProperties_ReduceEnergy()
        {
            this.compClass = typeof(CompProperties_ReduceEnergy);
        }
        public int rEnergy;
        public int level;
    }
}
