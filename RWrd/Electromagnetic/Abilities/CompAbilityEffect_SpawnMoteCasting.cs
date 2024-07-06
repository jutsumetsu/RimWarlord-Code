using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;

namespace Electromagnetic.Abilities
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
