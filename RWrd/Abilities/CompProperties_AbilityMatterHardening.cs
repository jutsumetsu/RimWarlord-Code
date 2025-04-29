using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Electromagnetic.Abilities;
using RimWorld;

namespace Electromagnetic.Abilities
{
    public class CompProperties_AbilityMatterHardening : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityMatterHardening()
        {
            this.compClass = typeof(CompAbilityEffect_MatterHardening);
        }
        public int HardeningFactor = 10;
    }
}
