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
    public class CompAbilityEffect_MatterHardening : CompAbilityEffect_Electromagnetic
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
            // 目标是否为Pawn
            Pawn targetPawn = targetThing as Pawn;
            if (targetPawn != null)
            {
                RepairPawnEquipment(targetPawn);
                return;
            }
            try
            {
                if (Caster.IsHavePowerRoot())
                {
                    Hediff_RWrd_PowerRoot root = Caster.GetPowerRoot();
                    int masteryOffset = (int)Math.Floor(this.Ability.mastery / 10f);
                    int level = root.energy.AvailableLevel + root.energy.FinalLevelOffset + 1 + masteryOffset;
                    int num = (int)Math.Floor(level / 5f);
                    if (root.energy.IsUltimate)
                    {
                        num += (int)Math.Floor(root.energy.PowerEnergy);
                    }
                    targetThing.HitPoints = Tools.IntRestrict(Mathf.RoundToInt(targetThing.MaxHitPoints * Props.HardeningFactor * num));
                }
                else
                {
                    targetThing.HitPoints = Tools.IntRestrict(Mathf.RoundToInt(targetThing.MaxHitPoints * Props.HardeningFactor * 1));
                }
                

            }
            catch (Exception ex)
            {
                Log.Error($"Error applying MatterHardening: {ex}");
            }
            //Log.Message("done");
        }

        public void RepairPawnEquipment(Pawn pawn)
        {
            if (pawn == null)
                return;

            Pawn caster = this.parent.pawn;

            List<ThingWithComps> toHeal = (from t in pawn.equipment.AllEquipmentListForReading.Concat(pawn.apparel.WornApparel)
                                           where t.def.useHitPoints
                                           select t).ToList();

            foreach (ThingWithComps item in toHeal)
            {
                if (caster.IsHavePowerRoot())
                {
                    Hediff_RWrd_PowerRoot root = caster.GetPowerRoot();
                    int masteryOffset = (int)Math.Floor(this.Ability.mastery / 10f);
                    int level = root.energy.AvailableLevel + root.energy.FinalLevelOffset + 1 + masteryOffset;
                    int num = (int)Math.Floor(level / 5f);
                    item.HitPoints = Tools.IntRestrict(Mathf.RoundToInt(item.MaxHitPoints * Props.HardeningFactor * num));
                }
                else
                {
                    item.HitPoints = Tools.IntRestrict(Mathf.RoundToInt(item.MaxHitPoints * Props.HardeningFactor * 1));
                }
            }
        }
    }
}
