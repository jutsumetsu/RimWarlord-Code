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
using Electromagnetic.Setting;

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
                    if (!pawn.IsHavePowerRoot())
                    {
                        Hediff hediff = HediffMaker.MakeHediff(RWrd_DefOf.Hediff_RWrd_PowerRoot, pawn);
                        pawn.health.AddHediff(hediff);
                    }
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

                        Hediff_RWrd_PowerRoot root = casterPawn.GetPowerRoot();
                        if (root != null)
                        {
                            int acr = root.energy.AvailableCompleteRealm();
                            int pff = root.energy.PowerFlowFactor(true);
                            int level = root.energy.level;
                            float cr = root.energy.completerealm;
                            if (acr >= 1 && pff >= 1)
                            {
                                float num = __instance.verbProps.AdjustedMeleeDamageAmount(__instance, __instance.CasterPawn);
                                float armorPenetration = __instance.verbProps.AdjustedArmorPenetration(__instance, __instance.CasterPawn);
                                armorPenetration += cr * 4;
                                num = Rand.Range(num * 0.8f, num * 1.2f);
                                num = Tools.FinalDamage(root, num);
                                num *= root.energy.outputPower;
                                bool ReachLimit = root.energy.level == root.energy.LevelMax && root.energy.exp == root.energy.MaxExp;
                                if (target.Thing.GetType() == typeof(Pawn) && !ReachLimit)
                                {
                                    Pawn pawn = target.Pawn;
                                    float num2 = num / 5;
                                    if (RWrdSettings.EnableExpCorrectionFactor)
                                    {
                                        num2 *= root.energy.ExpCorrectionFactor;
                                    }
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

                                Need need = casterPawn.needs.TryGetNeed<Need_Training>();
                                need.CurLevel += 0.001f;

                                List<DamageInfo> damageInfos = new List<DamageInfo>();
                                DamageDef def = __instance.verbProps.meleeDamageDef;
                                ThingDef source = __instance.EquipmentSource != null ? __instance.EquipmentSource.def : casterPawn.def;
                                Vector3 direction = (target.Thing.Position - casterPawn.Position).ToVector3();
                                DamageInfo damageInfo = new DamageInfo(def, num, armorPenetration, -1f, casterPawn, null, source, DamageInfo.SourceCategory.ThingOrUnknown, null, false, true, QualityCategory.Normal, true);
                                damageInfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
                                damageInfos.Add(damageInfo);

                                float radius = 0.7f + (float)Math.Ceiling(level / 20f) * 0.5f;
                                if (root.energy.IsUltimate)
                                {
                                    radius += 1.8f;
                                }
                                List<Thing> list = new List<Thing>
                                {
                                    target.Thing
                                };
                                foreach (Pawn pawn2 in casterPawn.MapHeld.mapPawns.AllPawns)
                                {
                                    bool flag = pawn2.Faction == casterPawn.Faction;
                                    if (flag)
                                    {
                                        list.Add(pawn2);
                                    }
                                }
                                radius *= RWrdSettings.WaveRangeFactor * root.energy.waveRange;
                                radius = Math.Min(radius, 56);
                                int waveDamage = (int)Math.Floor((root.energy.DamageImmunityThreshold - 5) * root.energy.wavePower);
                                if (radius > 1f && RWrdSettings.PowerfulEnergyWave && root.energy.personalEnergyWave)
                                {
                                    GenExplosion.DoExplosion(target.Thing.PositionHeld, target.Thing.MapHeld, radius, RWrd_DefOf.RWrd_PowerfulWave, casterPawn, waveDamage, 0, null, null, null, null, null, 0, 1, null, false, null, 0, 1, 0, false, null, list, null, RWrdSettings.DoVisualWaveEffect, 1, 0, false, null, 0);
                                }

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
                if (pawn.IsHavePowerRoot())
                {
                    Hediff_RWrd_PowerRoot root = pawn.GetPowerRoot();
                    int pff = root.energy.PowerFlowFactor();
                    int level = root.energy.level + 1;
                    int multiplier = pff + level;
                    if (root.energy.IsUltimate)
                    {
                        multiplier += (int)Math.Floor(root.energy.PowerEnergy);
                    }
                    __result *= multiplier;
                }
            }
        }
        //属性加成
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
                    if (pawn.IsHavePowerRoot())
                    {
                        Hediff_RWrd_PowerRoot root = pawn.GetPowerRoot();
                        bool flag = stat.ToString() == nameof(StatDefOf.ArmorRating_Sharp);
                        bool flag1 = stat.ToString() == nameof(StatDefOf.ArmorRating_Blunt);
                        bool flag2 = stat.ToString() == nameof(StatDefOf.ArmorRating_Heat);
                        bool flag15 = stat.ToString() == nameof(StatDefOf.MeleeDodgeChance);
                        bool flag18 = stat.ToString() == nameof(StatDefOf.MeleeCooldownFactor);
                        bool flag21 = stat.ToString() == nameof(StatDefOf.IncomingDamageFactor);
                        if (flag || flag1 || flag2)
                        {
                            float cr = root.energy.completerealm;
                            __result += cr * 4;
                        }
                        if (flag15)
                        {
                            __result = Math.Min(__result, 0.85f);
                        }
                        if (flag18)
                        {
                            int lf = root.energy.availableLevel + 1;
                            __result += -lf * 0.02f;
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
        //强者不死于厚岩顶
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
                            return pawn.IsHavePowerRoot();
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
        //强者不死于杂鱼
        [HarmonyPatch(typeof(Thing))]
        [HarmonyPatch("TakeDamage")]
        public class Patch_Thing_TakeDamage
        {
            [HarmonyPrefix]
            public static bool Prefix(ref DamageInfo dinfo, Thing __instance)
            {
                if (__instance is Pawn pawn)
                {
                    if (pawn.IsHavePowerRoot())
                    {
                        Hediff_RWrd_PowerRoot root = pawn.GetPowerRoot();
                        float num = root.energy.DamageImmunityThreshold;
                        Log.Message(pawn.Name.ToStringShort + "'s Damage Immunity Threshold: " + num.ToString() + " Current damage: " + dinfo.Amount.ToString());
                        if (dinfo.Amount <= num)
                        {
                            dinfo.SetAmount(0f);
                        }
                        else if (dinfo.Amount <= num * 1.5f)
                        {
                            dinfo.SetAmount(num);
                        }
                        else if ( dinfo.Amount <= num * 2)
                        {
                            dinfo.SetAmount(num * 1.25f);
                        }
                    }
                }
                return true;
            }
        }
        //强者免疫疾病事件
        [HarmonyPatch(typeof(IncidentWorker_Disease))]
        [HarmonyPatch("PotentialVictims")]
        public class Patch_IncidentWorker_Disease_PotentialVictims
        {
            [HarmonyPostfix]
            public static void Postfix(ref IEnumerable<Pawn> __result, IncidentWorker_Disease __instance)
            {
                __result = __result.Where(pawn => !pawn.IsHavePowerRoot());
            }
        }
        //强者免疫地块污染
        [HarmonyPatch(typeof(PollutionUtility))]
        [HarmonyPatch("PawnPollutionTick")]
        public static class Patch_PollutionUtility_PawnPollutionTick
        {
            [HarmonyPrefix]
            public static bool Prefix(Pawn pawn)
            {
                if (pawn.IsHavePowerRoot())
                {

                    return false;
                }

                return true;
            }
        }
        //强者免疫毒气
        [HarmonyPatch(typeof(GasUtility))]
        [HarmonyPatch("PawnGasEffectsTick")]
        public static class Patch_GasUtility_PawnGasEffectsTick
        {
            [HarmonyPrefix]
            public static bool Prefix(Pawn pawn)
            {
                if (pawn.IsHavePowerRoot())
                {

                    return false;
                }

                return true;
            }
        }
        //磁场力量遗传
        [HarmonyPatch(typeof(QuestManager))]
        [HarmonyPatch("Notify_PawnBorn")]
        public static class Patch_QuestManager_Notify_PawnBorn
        {
            [HarmonyPrefix]
            public static void Prefix(Thing baby, Thing birther, Pawn mother, Pawn father)
            {
                if (baby is Pawn babyPawn)
                {
                    bool hasRootParent = (mother != null && mother.IsHavePowerRoot() == true) ||
                                         (father != null && father.IsHavePowerRoot() == true);

                    if (hasRootParent)
                    {
                        Hediff root = HediffMaker.MakeHediff(RWrd_DefOf.Hediff_RWrd_PowerRoot, babyPawn);
                        babyPawn.health.AddHediff(root);
                        babyPawn.story.traits.GainTrait(new Trait(RWrd_DefOf.RWrd_Gifted));
                    }
                }
            }
        }
    }
}
