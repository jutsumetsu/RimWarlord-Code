using Electromagnetic.Abilities;
using Electromagnetic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Electromagnetic.Abilities
{
    public class CompAbilityToggle_ProtectiveForce : CompAbilityToggle_Electromagnetic
    {
        //绑定Properties
        public CompProperties_AbilityProtectiveForce Props
        {
            get
            {
                return (CompProperties_AbilityProtectiveForce)this.props;
            }
        }
        public override void Apply()
        {
            HediffDef pf = RWrd_DefOf.RWrd_ProtectiveForceHediff;
            if (Caster.GetPowerRoot().energy.energy <= 0 && !this.Ability.isActive)
            {
                return;
            }
            this.Ability.isActive = !this.Ability.isActive;
            Hediff hediff = Caster.health.hediffSet.GetFirstHediffOfDef(pf);
            if (this.Ability.isActive && hediff == null)
            {
                Hediff newHediff = HediffMaker.MakeHediff(pf, Caster);
                Caster.health.AddHediff(newHediff);
            }
            else if (!this.Ability.isActive && hediff != null)
            {
                Caster.health.RemoveHediff(hediff);
            }
        }
    }
}
