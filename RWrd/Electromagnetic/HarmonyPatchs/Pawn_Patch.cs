using HarmonyLib;
using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Electromagnetic.Core;
using UnityEngine;
using System.Net.NetworkInformation;

namespace Electromagnetic.HarmonyPatchs
{
    public class Pawn_Patch
    {
        [HarmonyPatch(typeof(Pawn))]
        [HarmonyPatch(nameof(Pawn.Tick))]
        class Patch1
        {
            [HarmonyPostfix]
            public static void TickPostFix(Pawn __instance)
            {
                if (__instance.RaceProps.Humanlike)
                {
                    if (Find.TickManager.TicksGame % 600 == 0)
                    {
                        bool flag = __instance.IsHaveRoot();
                        if (!flag)
                        {
                            bool flag2 = __instance.story.traits.HasTrait(RWrd_DefOf.RWrd_Gifted);
                            if (flag2)
                            {
                                Hediff hediff = HediffMaker.MakeHediff(RWrd_DefOf.Hediff_RWrd_PowerRoot, __instance, null);
                                __instance.health.AddHediff(hediff, null, null, null);
                            }
                        }
                    }
                }
            }
        }
        //Thing伤害patch
        [HarmonyPatch(typeof(DamageWorker))]
        [HarmonyPatch("Apply")]
        class Patch2
        {
            [HarmonyPrefix]
            public static bool GetThingMeleeDamagePreFix(DamageInfo dinfo, Thing victim, ref DamageWorker.DamageResult __result)
            {
                /*if (victim.SpawnedOrAnyParentSpawned)
                {
                    ImpactSoundUtility.PlayImpactSound(victim, dinfo.Def.impactSoundType, victim.MapHeld);
                }*/
                Pawn pawn = dinfo.Instigator as Pawn;
                if (pawn != null)
                {
                    if (victim.def.useHitPoints && dinfo.Def.harmsHealth)
                    {
                        Hediff_RWrd_PowerRoot root = pawn.GetRoot();
                        if (root != null)
                        {
                            int acr = root.energy.AvailableCompleteRealm();
                            int pff = root.energy.PowerFlowFactor();
                            int level = root.energy.CurrentDef.level;
                            if (acr >= 1 && pff >= 1)
                            {
                                float num = dinfo.Amount;
                                if (victim.def.category == ThingCategory.Building)
                                {
                                    num *= dinfo.Def.buildingDamageFactor;
                                    if (victim.def.passability == Traversability.Impassable)
                                    {
                                        num *= dinfo.Def.buildingDamageFactorImpassable;
                                    }
                                    else
                                    {
                                        num *= dinfo.Def.buildingDamageFactorPassable;
                                    }
                                    if (dinfo.Def.scaleDamageToBuildingsBasedOnFlammability)
                                    {
                                        num *= Mathf.Max(0.05f, victim.GetStatValue(StatDefOf.Flammability, true, -1));
                                    }
                                    if ((pawn = (dinfo.Instigator as Pawn)) != null && pawn.IsShambler)
                                    {
                                        num *= 1.5f;
                                    }
                                    if (ModsConfig.BiotechActive && dinfo.Instigator != null && (dinfo.WeaponBodyPartGroup != null || (dinfo.Weapon != null && dinfo.Weapon.IsMeleeWeapon)) && victim.def.IsDoor)
                                    {
                                        num *= dinfo.Instigator.GetStatValue(StatDefOf.MeleeDoorDamageFactor, true, -1);
                                    }
                                }
                                if (victim.def.category == ThingCategory.Plant)
                                {
                                    num *= dinfo.Def.plantDamageFactor;
                                }
                                else if (victim.def.IsCorpse)
                                {
                                    num *= dinfo.Def.corpseDamageFactor;
                                }
                                num += level;
                                num *= acr * pff;
                                __result = new DamageWorker.DamageResult();
                                __result.totalDamageDealt = Mathf.Min(victim.HitPoints, GenMath.RoundRandom(num));
                                victim.HitPoints -= Mathf.RoundToInt(__result.totalDamageDealt);
                                if (victim.HitPoints <= 0)
                                {
                                    victim.HitPoints = 0;
                                    victim.Kill(new DamageInfo?(dinfo), null);
                                }
                                root.energy.damage = __result.totalDamageDealt;

                                return false;
                            }
                        }
                    }
                }
                return true;
            }
        }
        //Pawn伤害patch
        [HarmonyPatch(typeof(DamageWorker_AddInjury))]
        [HarmonyPatch("Apply")]
        class Patch3
        {
            [HarmonyPrefix]
            public static void GetPawnMeleeDamagePreFix(DamageInfo dinfo, Thing thing, ref DamageWorker.DamageResult __result)
            {
                if (dinfo.Instigator != null && thing != null)
                {
                    Pawn instigator = dinfo.Instigator as Pawn;
                    Hediff_RWrd_PowerRoot root = instigator.GetRoot();
                    if (root != null)
                    {
                        int acr = root.energy.AvailableCompleteRealm();
                        int pff = root.energy.PowerFlowFactor();
                        int level = root.energy.CurrentDef.level;
                        if (acr >= 1 && pff >= 1)
                        {
                            float multiplier = acr * pff;
                            float num_cache = dinfo.Amount + level;
                            dinfo.SetAmount(num_cache * multiplier);
                            __result.totalDamageDealt *= multiplier;
                        }
                    }
                }
            }
        }
    }
}
