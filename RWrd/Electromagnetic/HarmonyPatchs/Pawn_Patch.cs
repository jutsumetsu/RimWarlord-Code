using HarmonyLib;
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
        [HarmonyPatch(typeof(Pawn))]
        [HarmonyPatch(nameof(Pawn.Tick))]
        class Patch1
        {
            [HarmonyPostfix]
            public static void TickPostFix(Pawn __instance)
            {
                if (__instance.RaceProps.Humanlike)
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
        // Pawn伤害patch
        [HarmonyPatch(typeof(Verb_MeleeAttackDamage))]
        [HarmonyPatch("DamageInfosToApply")]
        class Patch2
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
                            int level = root.energy.CurrentDef.level;
                            float cr = root.energy.completerealm;
                            if (acr >= 1 && pff >= 1)
                            {
                                float multiplier = acr + pff;
                                multiplier = (int)Math.Floor(multiplier / 2);
                                float num = __instance.verbProps.AdjustedMeleeDamageAmount(__instance, __instance.CasterPawn);
                                float armorPenetration = __instance.verbProps.AdjustedArmorPenetration(__instance, __instance.CasterPawn);
                                armorPenetration += cr;
                                num = Rand.Range(num * 0.8f, num * 1.2f);
                                num += level;
                                num *= multiplier;
                                if (target.Thing.GetType() == typeof(Pawn))
                                {
                                    Pawn pawn = target.Pawn;
                                    root.energy.damage = num;
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
                    int level = root.energy.CurrentDef.level + 1;
                    int multiplier = pff + level;
                    __result *= multiplier;
                }
            }
        }
        [HarmonyPatch(typeof(StatExtension))]
        [HarmonyPatch(nameof(StatExtension.GetStatValue))]
        class Patch4
        {
            [HarmonyPostfix]
            public static void StatFix(Thing thing, StatDef stat, ref float __result)
            {
                if (thing.GetType() == typeof(Pawn))
                {
                    Pawn pawn = thing as Pawn;
                    if (pawn.IsHaveRoot())
                    {
                        Hediff_RWrd_PowerRoot root = pawn.GetRoot();
                        float cr = root.energy.completerealm;
                        float num = __result + cr;
                        int level = root.energy.CurrentDef.level;
                        int pff = root.energy.PowerFlowFactor();
                        int lf = level + 1;
                        int multiplier = pff + lf;
                        float num2 = __result - lf * 0.1f;
                        float num3 = __result - cr;
                        bool flag = stat.ToString() == nameof(StatDefOf.ShootingAccuracyPawn);
                        bool flag1 = stat.ToString() == nameof(StatDefOf.ShootingAccuracyFactor_Touch);
                        bool flag2 = stat.ToString() == nameof(StatDefOf.ShootingAccuracyFactor_Short);
                        bool flag3 = stat.ToString() == nameof(StatDefOf.ShootingAccuracyFactor_Medium);
                        bool flag4 = stat.ToString() == nameof(StatDefOf.ShootingAccuracyFactor_Long);
                        bool flag5 = stat.ToString() == nameof(StatDefOf.MeleeHitChance);
                        bool flag6 = stat.ToString() == nameof(StatDefOf.CarryingCapacity);
                        bool flag7 = stat.ToString() == nameof(StatDefOf.GlobalLearningFactor);
                        bool flag8 = stat.ToString() == nameof(StatDefOf.ImmunityGainSpeed);
                        bool flag9 = stat.ToString() == nameof(StatDefOf.HuntingStealth);
                        bool flag10 = stat.ToString() == nameof(StatDefOf.MedicalSurgerySuccessChance);
                        bool flag11 = stat.ToString() == nameof(StatDefOf.MedicalTendQuality);
                        bool flag12 = stat.ToString() == nameof(StatDefOf.ArmorRating_Sharp);
                        bool flag13 = stat.ToString() == nameof(StatDefOf.ArmorRating_Blunt);
                        bool flag14 = stat.ToString() == nameof(StatDefOf.ArmorRating_Heat);
                        bool flag15 = stat.ToString() == nameof(StatDefOf.MeleeDodgeChance);
                        bool flag16 = stat.ToString() == nameof(StatDefOf.ComfyTemperatureMax);
                        bool flag17 = stat.ToString() == nameof(StatDefOf.ComfyTemperatureMin);
                        bool flag18 = stat.ToString() == nameof(StatDefOf.MeleeCooldownFactor);
                        bool flag19 = stat.ToString() == nameof(StatDefOf.InjuryHealingFactor);
                        bool flag20 = stat.ToString() == nameof(StatDefOf.ToxicResistance);
                        bool flag21 = stat.ToString() == nameof(StatDefOf.IncomingDamageFactor);
                        bool flag22 = stat.ToString() == nameof(StatDefOf.MaxHitPoints);
                        bool flag23 = stat.ToString() == nameof(StatDefOf.NegotiationAbility);
                        if (flag || flag1 || flag2 || flag3 || flag4 || flag10)
                        {
                            __result = Math.Min(num, 1);
                        }
                        if (flag5 || flag9 || flag11 || flag12 || flag13 || flag14)
                        {
                            __result = num;
                        }
                        if (flag6 || flag7 || flag8 || flag20)
                        {
                            __result *= lf;
                        }
                        if (flag15)
                        {
                            __result += Math.Min(cr, 0.5f);
                            __result = Math.Min(__result, 0.85f);
                        }
                        if (flag16)
                        {
                            __result = 100 + 50 * lf;
                        }
                        if (flag17)
                        {
                            __result = -50 - 50 * lf;
                        }
                        if (flag18)
                        {
                            __result = Math.Max(num2, 0.05f);
                        }
                        if (flag19)
                        {
                            __result = (int)Math.Ceiling(lf / 2f) * 5;
                        }
                        if (flag21)
                        {
                            __result = Math.Max(num3, 0.1f);
                        }
                        if (flag22)
                        {
                            __result *= multiplier;
                        }
                        if (flag23)
                        {
                            if (level >= 10)
                            {
                                __result += 0.8f;
                            }
                        }
                    }
                }
            }
        }
       /* [HarmonyPatch(typeof(CompAbilityEffect_GiveHediff))]
        [HarmonyPatch("ApplyInner")]
        class Patch5
        {
            [HarmonyPrefix]
            public static bool hediffPatch(CompAbilityEffect_GiveHediff __instance, Pawn target, Pawn other)
            {
                Pawn caster = __instance.parent.pawn;

                if (caster.IsHaveRoot() && __instance.Props.hediffDef.hediffClass == typeof(Hediff_TargetBase))
                {
                    Log.Message("patch success");
                    if (target != null)
                    {
                        bool TryResist = Traverse.Create(__instance).Field("TryResist").GetValue<bool>();
                        if (TryResist)
                        {
                            MoteMaker.ThrowText(target.DrawPos, target.Map, "Resisted".Translate(), -1f);
                            return false;
                        }
                        if (__instance.Props.replaceExisting)
                        {
                            Hediff firstHediffOfDef = target.health.hediffSet.GetFirstHediffOfDef(__instance.Props.hediffDef, false);
                            if (firstHediffOfDef != null)
                            {
                                target.health.RemoveHediff(firstHediffOfDef);
                            }
                        }
                        Hediff hediff = HediffMaker.MakeHediff(__instance.Props.hediffDef, target, __instance.Props.onlyBrain ? target.health.hediffSet.GetBrain() : null);
                        HediffComp_Disappears hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();
                        if (hediffComp_Disappears != null)
                        {
                            hediffComp_Disappears.ticksToDisappear = __instance.GetDurationSeconds(target).SecondsToTicks();
                        }
                        if (__instance.Props.severity >= 0f)
                        {
                            hediff.Severity = __instance.Props.severity;
                        }
                        HediffComp_Link hediffComp_Link = hediff.TryGetComp<HediffComp_Link>();
                        if (hediffComp_Link != null)
                        {
                            hediffComp_Link.other = other;
                            hediffComp_Link.drawConnection = (target == __instance.parent.pawn);
                        }
                        if (caster.IsHaveRoot())
                        {
                            Hediff_TargetBase hediff_TargetBase = hediff as Hediff_TargetBase;
                            hediff_TargetBase.root = caster.GetRoot();
                        }
                        else
                        {
                            target.health.AddHediff(hediff, null, null, null);
                        }
                    }
                    return false;
                }

                return true;
            }
        }*/
    }
}
