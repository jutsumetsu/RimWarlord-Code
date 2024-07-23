using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Electromagnetic.Core
{
    [DefOf]
    public static class RWrd_DefOf
    {
        static RWrd_DefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(RWrd_DefOf));
        }
        //修炼主干
        public static HediffDef Hediff_RWrd_PowerRoot;
        public static TraitDef RWrd_Gifted;
        //技能树
        public static RWrd_RouteDef Base;
        public static RWrd_RouteDef SixSecret;
        //能力
        public static AbilityDef RWrd_Ability_CellRecombination;
        //特效
        public static FleckDef RWrd_ElectricClawAttractFleck;
        public static FleckDef RWrd_ElectricClawFleck;
        public static FleckDef RWrd_ThunderPunchFleck;
        //建筑
        public static ThingDef RWrd_Building_TrainingSpot;

        public static ThingDef HiTechResearchBench;
        //Job
        public static JobDef RWrd_General_Training;
        public static JobDef RWrd_ResearchDisc;
        //Hediff
        public static HediffDef RWrd_Regenerating;
        public static HediffDef RWrd_ElectricInternalEnergy;
        public static HediffDef RWrd_HeavenLock;
        public static HediffDef RWrd_ExplosiveEnergy;
        public static HediffDef RWrd_AsuraFormula;

        public static HediffDef Asthma;
        public static HediffDef BadBack;
        public static HediffDef Flu;
        public static HediffDef Frail;
        //人物属性
        public static StatDef MeleeArmorPenetration;

        public static StatDef ThirstRateMultiplier;
        public static StatDef HygieneRateMultiplier;
        public static StatDef BladderRateMultiplier;
    }
}