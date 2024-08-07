﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Electromagnetic.Core;
using UnityEngine;
using System.Net.NetworkInformation;
using System.Linq;
using Electromagnetic.Abilities;

namespace Electromagnetic.HarmonyPatchs
{
    public class Pawn_Patch
    {
        [HarmonyPatch(typeof(TraitSet))]
        [HarmonyPatch(nameof(TraitSet.GainTrait))]
        class GainTraitPatch
        {
            [HarmonyPostfix]
            public static void GainTraitPostFix(TraitSet __instance, Trait trait)
            {
                Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
                if (trait.def == RWrd_DefOf.RWrd_Gifted && pawn.RaceProps.Humanlike)
                {
                    if (!pawn.IsHaveRoot())
                    {
                        Hediff hediff = HediffMaker.MakeHediff(RWrd_DefOf.Hediff_RWrd_PowerRoot, pawn);
                        pawn.health.AddHediff(hediff);
                    }
                    /*if (ModLister.BiotechInstalled)
                    {

                    }*/
                }
            }
        }
        // Pawn伤害patch
        [HarmonyPatch(typeof(Verb_MeleeAttackDamage))]
        [HarmonyPatch("DamageInfosToApply")]
        class DamagePatch
        {
            [HarmonyPrefix]
            public static bool DamageInfosToApply_Prefix(Verb_MeleeAttackDamage __instance, LocalTargetInfo target, ref IEnumerable<DamageInfo> __result)
            {
                try
                {
                    if (__instance.CasterIsPawn && __instance.CasterPawn != null)
                    {
                        Pawn casterPawn = __instance.CasterPawn;

                        Hediff_RWrd_PowerRoot root = casterPawn.GetRoot();
                        if (root != null)
                        {
                            int acr = root.energy.AvailableCompleteRealm();
                            int pff = root.energy.PowerFlowFactor();
                            int level = root.energy.level;
                            float cr = root.energy.completerealm;
                            if (acr >= 1 && pff >= 1)
                            {
                                float multiplier = acr + pff;
                                multiplier = (int)Math.Floor(multiplier / 2);
                                float num = __instance.verbProps.AdjustedMeleeDamageAmount(__instance, __instance.CasterPawn);
                                float armorPenetration = __instance.verbProps.AdjustedArmorPenetration(__instance, __instance.CasterPawn);
                                /*armorPenetration += cr;*/
                                num = Rand.Range(num * 0.8f, num * 1.2f);
                                num += level;
                                num *= multiplier;
                                if (target.Thing.GetType() == typeof(Pawn))
                                {
                                    Pawn pawn = target.Pawn;
                                    float num2 = num / 5 * root.energy.ExpCorrectionFactor;
                                    int exp1 = (int)Math.Floor(num2);
                                    int exp2 = exp1 * 10;
                                    if (level == 0)
                                    {
                                        //电推经验获取
                                        root.energy.SetExp(exp2);
                                    }
                                    else
                                    {
                                        //磁场转动经验获取
                                        root.energy.SetExp(exp1);
                                    }
                                }

                                List<DamageInfo> damageInfos = new List<DamageInfo>();
                                DamageDef def = __instance.verbProps.meleeDamageDef;
                                ThingDef source = __instance.EquipmentSource != null ? __instance.EquipmentSource.def : casterPawn.def;
                                Vector3 direction = (target.Thing.Position - casterPawn.Position).ToVector3();
                                DamageInfo damageInfo = new DamageInfo(def, num, armorPenetration, -1f, casterPawn, null, source, DamageInfo.SourceCategory.ThingOrUnknown, null, false, true, QualityCategory.Normal, true);
                                damageInfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
                                damageInfos.Add(damageInfo);

                                if (__instance.tool != null && __instance.tool.extraMeleeDamages != null)
                                {
                                    foreach (ExtraDamage extraDamage in __instance.tool.extraMeleeDamages)
                                    {
                                        if (Rand.Chance(extraDamage.chance))
                                        {
                                            float extraDamageAmount = Rand.Range(extraDamage.amount * 0.8f, extraDamage.amount * 1.2f);
                                            DamageInfo extraDamageInfo = new DamageInfo(extraDamage.def, extraDamageAmount, extraDamage.AdjustedArmorPenetration(__instance, __instance.CasterPawn), -1f, casterPawn, null, source, DamageInfo.SourceCategory.ThingOrUnknown, null, true, true, QualityCategory.Normal, true);
                                            extraDamageInfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
                                            damageInfos.Add(extraDamageInfo);
                                        }
                                    }
                                }
                                bool surpriseAttack = Traverse.Create(__instance).Field("surpriseAttack").GetValue<bool>();
                                if (surpriseAttack)
                                {
                                    IEnumerable<ExtraDamage> surpriseDamages = __instance.verbProps.surpriseAttack?.extraMeleeDamages ?? Enumerable.Empty<ExtraDamage>();
                                    surpriseDamages = surpriseDamages.Concat(__instance.tool?.surpriseAttack?.extraMeleeDamages ?? Enumerable.Empty<ExtraDamage>());

                                    foreach (ExtraDamage extraDamage in surpriseDamages)
                                    {
                                        int surpriseDamageAmount = GenMath.RoundRandom(extraDamage.AdjustedDamageAmount(__instance, __instance.CasterPawn));
                                        DamageInfo surpriseDamageInfo = new DamageInfo(extraDamage.def, surpriseDamageAmount, extraDamage.AdjustedArmorPenetration(__instance, __instance.CasterPawn), -1f, casterPawn, null, source, DamageInfo.SourceCategory.ThingOrUnknown, null, true, true, QualityCategory.Normal, true);
                                        surpriseDamageInfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
                                        damageInfos.Add(surpriseDamageInfo);
                                    }
                                }


                                __result = damageInfos;
                                return false;
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Exception in Patch_Verb_MeleeAttackDamage: {ex}");
                }

                return true;
            }
        }
        //部位血量加成
        [HarmonyPatch(typeof(BodyPartDef))]
        [HarmonyPatch(nameof(BodyPartDef.GetMaxHealth))]
        class Patch3
        {
            [HarmonyPostfix]
            public static void BodyPartFix(Pawn pawn, ref float __result)
            {
                if (pawn.IsHaveRoot())
                {
                    Hediff_RWrd_PowerRoot root = pawn.GetRoot();
                    int pff = root.energy.PowerFlowFactor();
                    int level = root.energy.level + 1;
                    int multiplier = pff + level;
                    __result *= multiplier;
                }
            }
        }
        [HarmonyPatch(typeof(StatExtension))]
        [HarmonyPatch(nameof(StatExtension.GetStatValue))]
        class PawnStatPatch
        {
            [HarmonyPostfix]
            public static void StatFix(Thing thing, StatDef stat, ref float __result)
            {
                if (thing.GetType() == typeof(Pawn))
                {
                    Pawn pawn = thing as Pawn;
                    if (pawn.IsHaveRoot())
                    {
                        bool flag15 = stat.ToString() == nameof(StatDefOf.MeleeDodgeChance);
                        bool flag18 = stat.ToString() == nameof(StatDefOf.MeleeCooldownFactor);
                        bool flag21 = stat.ToString() == nameof(StatDefOf.IncomingDamageFactor);
                        if (flag15)
                        {
                            __result = Math.Min(__result, 0.85f);
                        }
                        if (flag18)
                        {
                            __result = Math.Max(__result, 0.05f);
                        }
                        if (flag21)
                        {
                            __result = Math.Max(__result, 0.01f);
                        }
                    }
                }
            }
        }
        [HarmonyPatch(typeof(RoofCollapserImmediate))]
        [HarmonyPatch("DropRoofInCellPhaseOne")]
        class ThickRockRoofPatch
        {
            [HarmonyPrefix]
            public static bool NoThickRockRoofDead_Prefix(IntVec3 c, Map map, List<Thing> outCrushedThings)
            {
                try
                {
                    List<Thing> thingList = c.GetThingList(map);

                    thingList.RemoveAll(thing =>
                    {
                        if (thing is Pawn pawn)
                        {
                            return pawn.IsHaveRoot();
                        }
                        return false;
                    });

                    return true;
                }
                catch (Exception ex)
                {
                    Log.Error($"Exception in Patch5: {ex}");
                    return true;
                }
            }
        }
    }
}
