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
    public class CompAbilityToggle_Flight : CompAbilityToggle_Electromagnetic
    {
        //绑定Properties
        public CompProperties_AbilityFlight Props
        {
            get
            {
                return (CompProperties_AbilityFlight)this.props;
            }
        }
        public override void Apply()
        {
            HediffDef flightDef = RWrd_DefOf.RWrd_Flight;
            HediffDef antigravityDef = RWrd_DefOf.RWrd_Antigravity;
            this.Ability.isActive = !this.Ability.isActive;
            int level = Caster.GetPowerRoot().energy.AvailableLevel;
            HediffDef targetDef = (level >= 50) ? antigravityDef : flightDef;
            Tools.RemoveHediffIfExists(Caster, flightDef);
            Tools.RemoveHediffIfExists(Caster, antigravityDef);
            if (this.Ability.isActive)
            {
                Hediff newHediff = HediffMaker.MakeHediff(targetDef, Caster);
                Caster.health.AddHediff(newHediff);
            }
        }
        public override void CompTick()
        {
            base.CompTick();
            if (Find.TickManager.TicksGame % 60 == 0 && ModDetector.PFIsLoaded && this.Ability.isActive && Caster.GetPowerRoot().energy.AvailableLevel < 50)
            {
                if (Caster.GetPowerRoot().energy.energy >= Caster.GetPowerRoot().energy.FlightConsumptionPerSecond) Caster.GetPowerRoot().energy.SetEnergy(-Caster.GetPowerRoot().energy.FlightConsumptionPerSecond);
                else
                {
                    this.Ability.isActive = false;
                    HediffDef flightDef = RWrd_DefOf.RWrd_Flight;
                    Tools.RemoveHediffIfExists(Caster, flightDef);
                }
            }
        }
    }
}
