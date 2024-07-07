using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Electromagnetic.Abilities
{
    public class CompProperties_AbilityWindBlade : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityWindBlade()
        {
            this.compClass = typeof(CompAbilityEffect_WindBlade);
        }
        public float range;
        public float lineWidthEnd = 13f;
        public FleckDef SpawnFleck;
    }
}
