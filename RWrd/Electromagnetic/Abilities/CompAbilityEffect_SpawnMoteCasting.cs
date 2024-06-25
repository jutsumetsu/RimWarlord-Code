using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Electromagnetic.Abilities;
using RimWorld;

namespace RWrd.Electromagnetic.Abilities
{
    public class CompAbilityEffect_SpawnMoteCasting : CompAbilityEffect
    {
        public new CompProperties_SpawnMoteCasting Props
        {
            get
            {
                return (CompProperties_SpawnMoteCasting)this.props;
            }
        }
    }
}
