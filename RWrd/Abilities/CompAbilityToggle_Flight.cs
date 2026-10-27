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
                ownsFlight = false;
                if (Caster.flight == null) return;
                Hediff newHediff = HediffMaker.MakeHediff(targetDef, Caster);
                Caster.health.AddHediff(newHediff);
                if (Caster.Flying) return;
                if (Caster.flight.CanFlyNow)
                {
                    Caster.flight.StartFlying();
                    ownsFlight = Caster.Flying;
                }
            }
            else
            {
                bool wasOwned = ownsFlight;
                ownsFlight = false;
                if (wasOwned) Caster.flight?.ForceLand();
            }
        }
        public override void CompTick()
        {
            base.CompTick();
            if (!Ability.isActive || Caster.flight == null) return;
            if (!ownsFlight && !Caster.Flying && Caster.flight.CanFlyNow)
            {
                Caster.flight.StartFlying();
                ownsFlight = Caster.Flying;
            }
            if (!ownsFlight) return;
            if (Caster.GetPowerRoot().energy.AvailableLevel >= 50) return;
            if (!Caster.Flying || Find.TickManager.TicksGame % 60 != 0) return;
            var energy = Caster.GetPowerRoot().energy;
            if (energy.energy >= energy.FlightConsumptionPerSecond)
                energy.SetEnergy(-energy.FlightConsumptionPerSecond);
            else
            {
                this.Ability.isActive = false;
                ownsFlight = false;
                Tools.RemoveHediffIfExists(Caster, RWrd_DefOf.RWrd_Flight);
            }
        }
        private bool ownsFlight;
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ownsFlight, "rwrd_ownsFlight", false);
        }
    }
}
