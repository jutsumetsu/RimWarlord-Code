using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Electromagnetic.Abilities
{
    public class CompAbilityEffect_SpawnMoteCast : CompAbilityEffect
    {
        //绑定Properties
        public new CompProperties_SpawnMoteCast Props
        {
            get
            {
                return (CompProperties_SpawnMoteCast)this.props;
            }
        }
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            //生成技能触发特效
            Pawn pawn = this.parent.pawn;
            if (pawn.Dead) { return; }
            ThingDef moteCastDef = this.Props.moteCastDef;
            MoteMaker.MakeAttachedOverlay(pawn, moteCastDef, CompAbilityEffect_SpawnMoteCast.MoteCastOffset, CompAbilityEffect_SpawnMoteCast.MoteCastScale, this.parent.verb.verbProps.warmupTime - CompAbilityEffect_SpawnMoteCast.MoteCastFadeTime);
        }
        private static float MoteCastFadeTime = 0.4f;
        private static float MoteCastScale = 2f;
        private static Vector3 MoteCastOffset = new Vector3(0f, 0f, 1f);
        private int tick = 0;
        private bool happend = false;
    }
}
