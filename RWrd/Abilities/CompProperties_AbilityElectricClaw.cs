using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electromagnetic.Abilities
{
    public class CompProperties_AbilityElectricClaw : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityElectricClaw()
        {
            this.compClass = typeof(CompAbilityEffect_ElectricClaw);
        }
        public int damage;
    }
}
