using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Electromagnetic.Abilities
{
    public class CompProperties_AbilityGiveTargetHediff : CompProperties_AbilityEffectWithDuration
    {
        public CompProperties_AbilityGiveTargetHediff()
        {
            this.compClass = typeof(CompAbilityEffect_GiveTargetHediff);
        }
        public HediffDef hediffDef;
        public bool onlyBrain;
        public bool applyToSelf;
        public bool onlyApplyToSelf;
        public bool applyToTarget = true;
        public bool replaceExisting;
        public float severity = -1f;
        public bool ignoreSelf;
    }
}
