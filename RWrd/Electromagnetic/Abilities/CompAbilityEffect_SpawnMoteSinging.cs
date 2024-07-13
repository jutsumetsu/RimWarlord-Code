using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Electromagnetic.Abilities
{
    public class CompAbilityEffect_SpawnMoteSinging : CompAbilityEffect
    {
        public new CompProperties_SpawnMoteSinging Props
        {
            get
            {
                return (CompProperties_SpawnMoteSinging)this.props;
            }
        }
        public override void CompTick()
        {
            Pawn pawn = this.parent.pawn;
            ThingDef moteCastDef = this.Props.moteCastDef;
            bool casting = this.parent.Casting;
            //生成读条特效
            if (casting)
            {
                bool flag = this.Props.moteCastDef != null;
                if (!flag)
                {
                    return;
                }
                MoteMaker.MakeAttachedOverlay(pawn, moteCastDef, CompAbilityEffect_SpawnMoteSinging.MoteCastOffset, CompAbilityEffect_SpawnMoteSinging.MoteCastScale, this.parent.verb.verbProps.warmupTime - CompAbilityEffect_SpawnMoteSinging.MoteCastFadeTime);
            }
            base.CompTick();
        }
        private static float MoteCastFadeTime = 0f;
        private static float MoteCastScale = 5f;
        private static Vector3 MoteCastOffset = new Vector3(0f, 0f, 0f);
    }
}
