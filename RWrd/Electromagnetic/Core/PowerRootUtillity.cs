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
        //是否拥有力量之源
        public static bool IsHaveRoot(this Pawn pawn)
        {
            return pawn.health.hediffSet.HasHediff(RWrd_DefOf.Hediff_RWrd_PowerRoot, false);
        }
        //获取角色力量之源数据
        public static Hediff_RWrd_PowerRoot GetRoot(this Pawn pawn)
        {
            return pawn.IsHaveRoot() ? ((Hediff_RWrd_PowerRoot)pawn.health.hediffSet.GetFirstHediffOfDef(RWrd_DefOf.Hediff_RWrd_PowerRoot, false)) : null;
        }
        //检查等级技能限制
        public static void CheckLevelAndLimitingAbility(this Pawn pawn)
        {
            bool flag = pawn.IsHaveRoot();
            if (flag)
            {
                Hediff_RWrd_PowerRoot firstHediff = pawn.GetRoot();
                int level = firstHediff.energy.currentRWrd.def.level;
                string route = firstHediff.Route;
                List<Ability> list = new List<Ability>();
                List<Ability> list2 = new List<Ability>();
                list2.AddRange(pawn.CheckAbilities(RWrd_DefOf.Base));
                bool flag2 = firstHediff.route != null;
                if (flag2)
                {
                    list2.AddRange(pawn.CheckAbilities(firstHediff.route));
                }
                foreach (Ability ability in pawn.abilities.abilities)
                {
                    CompAbilityEffect_ReduceEnergy compAbilityEffect_ReduceEnergy = ability.CompOfType<CompAbilityEffect_ReduceEnergy>();
                    bool flag3 = compAbilityEffect_ReduceEnergy == null;
                    if (!flag3)
                    {
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
        //随机生成力量之源
        public static void RandomPowerRootSpawn(Pawn pawn, Hediff_RWrd_PowerRoot root, out RimWarlordDef currentRWrd, out RWrd_RouteDef route)
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
            currentRWrd = RimWarlordDef.Dict.GetValueOrDefault(num);
            List<RWrd_RouteDef> allDefListForReading = DefDatabase<RWrd_RouteDef>.AllDefsListForReading;
            route =  allDefListForReading.RandomElement<RWrd_RouteDef>();
            root.route = route;
            foreach (AbilityDef def in route.AllAbilities)
            {
                pawn.abilities.GainAbility(def);
            }
        }
        //检查技能
        public static List<Ability> CheckAbilities(this Pawn pawn, RWrd_RouteDef route)
        {
            List<Ability> list = new List<Ability>();
            Hediff_RWrd_PowerRoot root = pawn.GetRoot();
            int level = root.energy.CurrentDef.level;
            foreach (RWrd_RouteLevel rwrd_RouteLevel in route.routeLevels)
            {
                string text = rwrd_RouteLevel.levelDef.defName + "：";
                bool flag;
                if (rwrd_RouteLevel.level > level)
                {
                    flag = true;
                }
                else
                {
                    flag = false;
                }
                foreach (AbilityDef abilityDef in rwrd_RouteLevel.abilities)
                {
                    Ability ability = pawn.abilities.GetAbility(abilityDef, false);
                    bool flag2 = ability == null;
                    if (!flag2)
                    {
                        list.Add(ability);
                        CompAbilityEffect_ReduceEnergy compAbilityEffect_ReduceEnergy = ability.CompOfType<CompAbilityEffect_ReduceEnergy>();
                        if (compAbilityEffect_ReduceEnergy != null && -compAbilityEffect_ReduceEnergy.Props.rEnergy > root.energy.energy)
                        {
                            flag = true;
                        }
                        ability.CompOfType<CompAbilityEffect_ReduceEnergy>().disabled = flag;
                        text += (flag ? "(Disabled)" : "(Enabled)");
                        text = text + abilityDef.defName + "，";
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
        //检查是否能满足长期需求
        public static bool LordPreventsGettingTraining(Pawn pawn)
        {
            Lord lord = pawn.GetLord();
            return lord != null && !lord.CurLordToil.AllowSatisfyLongNeeds;
        }
    }
}