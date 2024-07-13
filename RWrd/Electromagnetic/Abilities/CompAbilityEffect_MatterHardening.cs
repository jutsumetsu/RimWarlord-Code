using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Electromagnetic.Core;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Electromagnetic.Abilities
{
    public class CompAbilityEffect_MatterHardening : CompAbilityEffect
    {
        //绑定Properties
        public new CompProperties_AbilityMatterHardening Props
        {
            get
            {
                return (CompProperties_AbilityMatterHardening)this.props;
            }
        }
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target,dest);
            // 获取目标物品
            Thing targetThing = target.Thing;
            if (targetThing == null)
            {
                Log.Error("Target thing is null");
                return;
            }
            Pawn Caster = this.parent.pawn;
            try
            {
                if (Caster.IsHaveRoot())
                {
                    Hediff_RWrd_PowerRoot root = Caster.GetRoot();
                    int level = root.energy.CurrentDef.level + 1;
                    int num = (int)Math.Floor(level / 5f);
                    targetThing.HitPoints = Mathf.RoundToInt(targetThing.MaxHitPoints * Props.HardeningFactor * (num));
                }
                else
                {
                    targetThing.HitPoints = Mathf.RoundToInt(targetThing.MaxHitPoints * Props.HardeningFactor * 1);
                }
                

            }
            catch (Exception ex)
            {
                Log.Error($"Error applying MatterHardening: {ex}");
            }
            //Log.Message("done");


        }
    }
}
