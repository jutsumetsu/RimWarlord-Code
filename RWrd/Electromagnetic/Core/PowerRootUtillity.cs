using System;
using System.Collections.Generic;
using System.Linq;
using Electromagnetic.Abilities;
using Electromagnetic.Setting;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Electromagnetic.Core
{
    public static class PowerRootUtillity
    {
        /// <summary>
        /// 是否拥有力量之源
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public static bool IsHavePowerRoot(this Pawn pawn)
        {
            if (pawn == null)
            {
                return false;
            }
            else
            {
                return pawn.health.hediffSet.HasHediff(RWrd_DefOf.Hediff_RWrd_PowerRoot, false);
            }
        }
        /// <summary>
        /// 获取角色力量之源数据
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public static Hediff_RWrd_PowerRoot GetPowerRoot(this Pawn pawn)
        {
            return pawn.IsHavePowerRoot() ? ((Hediff_RWrd_PowerRoot)pawn.health.hediffSet.GetFirstHediffOfDef(RWrd_DefOf.Hediff_RWrd_PowerRoot, false)) : null;
        }
        /// <summary>
        /// 判断是否被锁
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public static bool IsLockedByEMPower(this Pawn pawn)
        {
            if (pawn == null)
            {
                return false;
            }
            else
            {
                return pawn.health.hediffSet.HasHediff(RWrd_DefOf.RWrd_HeavenLock, false);
            }
        }
        public static Hediff_HeavenLock GetHeavenLock(this Pawn pawn)
        {
            return pawn.IsLockedByEMPower() ? ((Hediff_HeavenLock)pawn.health.hediffSet.GetFirstHediffOfDef(RWrd_DefOf.RWrd_HeavenLock)) : null;
        }
        /// <summary>
        /// 获取心脏血量百分比
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public static float GetHeartHealthPercent(this Pawn pawn)
        {
            if (pawn?.health?.hediffSet == null)
            {
                return 0;
            }

            BodyPartRecord heart = pawn.health.hediffSet.GetNotMissingParts().FirstOrDefault(part => part.def.defName == "Heart");

            if (heart == null)
            {
                return 0;
            }

            float maxHP = heart.def.GetMaxHealth(pawn);

            float currentHP = maxHP;
            foreach (var injury in pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>())
            {
                if (injury.Part == heart)
                {
                    currentHP -= injury.Severity;
                }
            }

            return (currentHP / maxHP);
        }
        /// <summary>
        /// 检查技能树解锁
        /// </summary>
        /// <param name="pawn"></param>
        public static void CheckRouteUnlock(this Pawn pawn, Hediff_RWrd_PowerRoot root = null)
        {
            bool flag = pawn.IsHavePowerRoot();
            if (flag || root != null)
            {
                if (root == null)
                {
                    root = pawn.GetPowerRoot();
                }
                IEnumerable<RWrd_RouteDef> paths = DefDatabase<RWrd_RouteDef>.AllDefs;
                foreach (var path in paths)
                {
                    if (!root.routes.Contains(path))
                    {
                        if (path.CanPawnUnlock(pawn))
                        {
                            root.UnlockRoute(path);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 检查等级技能限制
        /// </summary>
        /// <param name="pawn"></param>
        public static void CheckEMAbilityLimiting(this Pawn pawn, Hediff_RWrd_PowerRoot root = null)
        {
            bool flag = pawn.IsHavePowerRoot();
            if (flag || root != null)
            {
                if (root == null)
                {
                    root = pawn.GetPowerRoot();
                }
                int level = root.energy.availableLevel;
                List<Ability> list = new List<Ability>();
                List<Ability> list2 = new List<Ability>();
                bool flag2 = root.routes.Count != 0;
                if (flag2)
                {
                    foreach (RWrd_RouteDef route in root.routes)
                    {
                        list2.AddRange(pawn.CheckEMAbilityTree(route));
                    }
                }
                foreach (Ability ability in pawn.abilities.abilities)
                {
                    CompAbilityEffect_ReduceEnergy compAbilityEffect_ReduceEnergy = ability.CompOfType<CompAbilityEffect_ReduceEnergy>();
                    bool flag3 = compAbilityEffect_ReduceEnergy == null;
                    if (!flag3)
                    {
                        if (compAbilityEffect_ReduceEnergy.Props.isCommon || compAbilityEffect_ReduceEnergy.Props.isAshura)
                        {
                            list2.Add(ability);
                        }
                        bool flag4 = list2.Contains(ability);
                        if (!flag4)
                        {
                            list.Add(ability);
                        }
                    }
                }
                list.RemoveDuplicates(null);
                list.ForEach(delegate (Ability a)
                {
                    pawn.abilities.RemoveAbility(a.def);
                });
            }
        }
        /// <summary>
        /// 初始等级
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public static int InitialLevel(this Hediff_RWrd_PowerRoot root)
        {
            int num = 0;
            Pawn pawn = root.pawn;
            if (pawn.story.Childhood is WarlordBackstoryDef warlordChildhood)
            {
                num = UnityEngine.Random.Range(warlordChildhood.minLevel, warlordChildhood.maxLevel);
                Log.Message(pawn.Name.ToStringShort + "'s warlordChildhood:" + pawn.story.Childhood.title + "[" + warlordChildhood.minLevel + "," + warlordChildhood.maxLevel + ")");
            }
            if (pawn.story.Adulthood is WarlordBackstoryDef warlordAdulthood)
            {
                if (num < warlordAdulthood.maxLevel)
                {
                    int adultMin = Math.Max(num, warlordAdulthood.minLevel);
                    num = UnityEngine.Random.Range(adultMin, warlordAdulthood.maxLevel);
                }
                Log.Message(pawn.Name.ToStringShort + "'s warlordAdulthood:" + pawn.story.Adulthood.title + "[" + warlordAdulthood.minLevel + "," + warlordAdulthood.maxLevel + ")");
            }
            if (num == 0)
            {
                int cP = (int)(pawn.kindDef.combatPower / 100f);
                bool flag = cP < 5;
                if (flag)
                {
                    cP = UnityEngine.Random.Range(0, 3);
                }
                else
                {
                    bool flag2 = cP < 8;
                    if (flag2)
                    {
                        cP = UnityEngine.Random.Range(3, 7);
                    }
                    else
                    {
                        bool flag3 = cP < 10;
                        if (flag3)
                        {
                            cP = UnityEngine.Random.Range(8, 10);
                        }
                    }
                }
                bool flag4 = cP > 10;
                if (flag4)
                {
                    cP = 10;
                }
                num = cP;
            }
            return num;
        }
        /// <summary>
        /// 检查技能树
        /// </summary>
        /// <param name="pawn">小人实例</param>
        /// <param name="route">技能树</param>
        /// <returns></returns>
        public static List<Ability> CheckEMAbilityTree(this Pawn pawn, RWrd_RouteDef route)
        {
            List<Ability> list = new List<Ability>();
            foreach (RWrd_RouteNode rwrd_RouteNode in route.routeNodes)
            {
                string text = route.defName + "-Node" + rwrd_RouteNode.number.ToString() + "：";
                bool flag;
                flag = rwrd_RouteNode.CanPawnUnlock(pawn);
                text += (flag ? "(Enabled)" : "(Disabled)");
                foreach (AbilityDef abilityDef in rwrd_RouteNode.abilities)
                {
                    text = text + abilityDef.defName + "，";
                    Ability ability = pawn.abilities.GetAbility(abilityDef, false);
                    bool flag2 = ability == null;
                    if (flag)
                    {
                        if (!flag2)
                        {
                            list.Add(ability);
                        }
                        else
                        {
                            pawn.abilities.GainAbility(abilityDef);
                            ability = pawn.abilities.GetAbility(abilityDef, false);
                            list.Add(ability);
                        }
                    }
                }
                bool godMode = DebugSettings.godMode;
                if (godMode)
                {
                    Log.Message(text);
                }
            }
            return list;
        }
        /// <summary>
        /// 移除技能技能树
        /// </summary>
        /// <param name="pawn">小人实例</param>
        /// <param name="route">技能树</param>
        public static void RemoveEMAbilityTree(this Pawn pawn, RWrd_RouteDef route)
        {
            foreach (var node in route.routeNodes)
            {
                foreach (var abilityDef in node.abilities)
                {
                    Ability ability = pawn.abilities.GetAbility(abilityDef, false);
                    if (ability != null)
                    {
                        pawn.abilities.RemoveAbility(abilityDef);
                    }
                }
            }
        }
        /// <summary>
        /// 检查是否能满足长期需求
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public static bool LordPreventsGettingTraining(Pawn pawn)
        {
            Lord lord = pawn.GetLord();
            return lord != null && !lord.CurLordToil.AllowSatisfyLongNeeds;
        }
        /// <summary>
        /// 更新Stage信息
        /// </summary>
        /// <param name="cr">完全境界设置识别</param>
        public static void UpdatePowerRootStageInfo(this Pawn pawn, bool cr = false)
        {
            Hediff_RWrd_PowerRoot root = pawn.GetPowerRoot();
            float num1;
            if (root.energy.level == 0)
            {
                num1 = 0.1f;
            }
            else
            {
                num1 = 0;
            }
            float num2;
            if (RWrdSettings.NoFoodDrinkRequired)
            {
                num2 = Math.Max(1 - root.energy.level * 0.05f, 0f);
            }
            else
            {
                num2 = 1;
            }
            root.stage.painFactor = num1;
            root.stage.hungerRateFactor = num2;
            if (cr)
            {
                root.stage.statOffsets = root.GetStatOffsetList.ToList();
                foreach (var capMod in root.stage.capMods)
                {
                    if (capMod.capacity == PawnCapacityDefOf.Manipulation)
                    {
                        capMod.offset = root.energy.completerealm;
                    }
                }
            }
            else
            {
                root.stage.statOffsets = root.GetStatOffsetList.ToList();
                root.stage.statFactors = root.GetStatFactorList.ToList();
                root.stage.capMods = root.GetPCMList.ToList();
            }
            pawn.health.Notify_HediffChanged(root);
        }
    }
}