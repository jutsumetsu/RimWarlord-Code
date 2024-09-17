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
            this.compClass = typeof(CompAbilityEffect_ReduceEnergy);
        }
        public int rEnergy;
        public int level;
        public float masteryFactor = 0;
        public float masteryOffset = 0;
        public bool isCommon = false;
        public bool isAshura = false;
        public bool isAttack = false;
    }
}
