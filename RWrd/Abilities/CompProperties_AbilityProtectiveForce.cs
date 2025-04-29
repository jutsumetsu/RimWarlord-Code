using Electromagnetic.Abilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electromagnetic.Abilities
{
    public class CompProperties_AbilityProtectiveForce : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityProtectiveForce()
        {
            this.compClass = typeof(CompAbilityToggle_ProtectiveForce);
        }
    }
}
