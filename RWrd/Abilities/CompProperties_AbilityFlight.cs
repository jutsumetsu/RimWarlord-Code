using Electromagnetic.Abilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electromagnetic.Abilities
{
    public class CompProperties_AbilityFlight : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityFlight()
        {
            this.compClass = typeof(CompAbilityToggle_Flight);
        }
    }
}
