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
        public static FleckDef RWrd_FragmentsA;
        public static FleckDef RWrd_FragmentsB;
        public static FleckDef RWrd_FragmentsC;
        public static FleckDef RWrd_FragmentsD;
        //建筑
        public static ThingDef RWrd_Building_TrainingSpot;

        public static ThingDef HiTechResearchBench;
        //Job
        public static JobDef RWrd_General_Training;
        public static JobDef RWrd_ResearchDisc;
        public static JobDef RWrd_StudyBook;
        //Hediff
        public static HediffDef RWrd_Regenerating;
        public static HediffDef RWrd_ElectricInternalEnergy;
        public static HediffDef RWrd_HeavenLock;
        public static HediffDef RWrd_ExplosiveEnergy;
        public static HediffDef RWrd_AsuraFormula;
        public static HediffDef RWrd_ProtectiveForceHediff;
        [MayRequire("pathfinding.framework")]
        public static HediffDef RWrd_Flight;
        [MayRequire("pathfinding.framework")]
        public static HediffDef RWrd_Antigravity;

        public static HediffDef Asthma;
        public static HediffDef BadBack;
        public static HediffDef Flu;
        public static HediffDef Frail;
        //伤害类型
        public static DamageDef RWrd_PowerfulWave;
        public static DamageDef RWrd_LifeDrain;
        //人物属性
        public static StatDef MeleeArmorPenetration;

        [MayRequire("DubsBadHygiene")]
        public static StatDef ThirstRateMultiplier;
        [MayRequire("DubsBadHygiene")]
        public static StatDef HygieneRateMultiplier;
        [MayRequire("DubsBadHygiene")]
        public static StatDef BladderRateMultiplier;

        [MayRequire("saveourship2")]
        public static StatDef HypoxiaResistance;
        [MayRequire("saveourship2")]
        public static StatDef DecompressionResistance;
    }
}