using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electromagnetic.Abilities
{
    public class CompProperties_AbilityHeavyNucleiKick : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityHeavyNucleiKick()
        {
            this.compClass = typeof(CompAbilityEffect_HeavyNucleiKick);
        }
    }
}
