using System;
using System.Collections.Generic;
using Electromagnetic.Abilities;
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
        public static bool IsHaveRoot(this Pawn pawn)
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
        public static Hediff_RWrd_PowerRoot GetRoot(this Pawn pawn)
        {
            return pawn.IsHaveRoot() ? ((Hediff_RWrd_PowerRoot)pawn.health.hediffSet.GetFirstHediffOfDef(RWrd_DefOf.Hediff_RWrd_PowerRoot, false)) : null;
        }
        /// <summary>
        /// 检查等级技能限制
        /// </summary>
        /// <param name="pawn"></param>
        public static void CheckAbilityLimiting(this Pawn pawn)
        {
            bool flag = pawn.IsHaveRoot();
            if (flag)
            {
                Hediff_RWrd_PowerRoot root = pawn.GetRoot();
                int level = root.energy.level;
                List<Ability> list = new List<Ability>();
                List<Ability> list2 = new List<Ability>();
                bool flag2 = root.routes.Count != 0;
                if (flag2)
                {
                    foreach (RWrd_RouteDef route in root.routes)
                    {
                        list2.AddRange(pawn.CheckAbilities(route));
                    }
                }
                else
                {
                    root.UnlockRoute(RWrd_DefOf.Base);
                    foreach (RWrd_RouteDef route in root.routes)
                    {
                        list2.AddRange(pawn.CheckAbilities(route));
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
        /// 随机生成力量之源
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="root">力量之源</param>
        /// <param name="currentRWrd">等级Def</param>
        /// <param name="route">技能树</param>
        public static void RandomPowerRootSpawn(Pawn pawn, Hediff_RWrd_PowerRoot root)
        {
            int num = (int)(pawn.kindDef.combatPower / 100f);
            bool flag = num < 5;
            if (flag)
            {
                num = UnityEngine.Random.Range(0, 3);
            }
            else
            {
                bool flag2 = num < 8;
                if (flag2)
                {
                    num = UnityEngine.Random.Range(3, 7);
                }
                else
                {
                    bool flag3 = num < 10;
                    if (flag3)
                    {
                        num = UnityEngine.Random.Range(8, 10);
                    }
                }
            }
            bool flag4 = num > 10;
            if (flag4)
            {
                num = 10;
            }
            root.energy.level = num;
            /*List<RWrd_RouteDef> allDefListForReading = DefDatabase<RWrd_RouteDef>.AllDefsListForReading;
            allDefListForReading.Remove(RWrd_DefOf.Base);
            RWrd_RouteDef route =  allDefListForReading.RandomElement<RWrd_RouteDef>();*/
            /*root.UnlockRoute(route);*/
        }
        /// <summary>
        /// 检查技能树
        /// </summary>
        /// <param name="pawn">小人实例</param>
        /// <param name="route">技能树</param>
        /// <returns></returns>
        public static List<Ability> CheckAbilities(this Pawn pawn, RWrd_RouteDef route)
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
        public static void RemoveAbilities(this Pawn pawn, RWrd_RouteDef route)
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
    }
}