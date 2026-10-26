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
        // 小人类型
        public static PawnKindDef RWrd_Pawn_BaakSide;
        //修炼主干
        public static HediffDef Hediff_RWrd_PowerRoot;
        public static TraitDef RWrd_Gifted;
        //技能树
        public static RWrd_RouteDef Base;
        public static RWrd_RouteDef SixSecret;
        //能力
        public static AbilityDef RWrd_Ability_CellRecombination;
        public static AbilityDef RWrd_Ability_ProtectiveForce;
        public static AbilityDef RWrd_Ability_Flight;
        //特效
        public static FleckDef RWrd_ElectricClawAttractFleck;
        public static FleckDef RWrd_ElectricClawFleck;
        public static FleckDef RWrd_ThunderPunchFleck;
        public static FleckDef RWrd_FragmentsA;
        public static FleckDef RWrd_FragmentsB;
        public static FleckDef RWrd_FragmentsC;
        public static FleckDef RWrd_FragmentsD;
        //衣服
        
        //建筑
        public static ThingDef RWrd_Building_TrainingSpot;

        public static ThingDef HiTechResearchBench;
        [MayRequireIdeology]
        public static ThingDef Altar_Grand;
        [MayRequireIdeology]
        public static ThingDef Pew;
        //派系
        [MayRequire("jm.rimwarlord.baak")]
        public static FactionDef RWrd_Faction_Empire_Baak;
        //颜色
        public static ColorDef BaakRed;
        //文化图标
        [MayRequireIdeology]
        public static IdeoIconDef DownBurst;
        //戒律
        [MayRequireIdeology]
        public static PreceptDef IdeoRole_Leader;
        [MayRequireIdeology]
        public static PreceptDef IdeoRole_Moralist;
        [MayRequireIdeology]
        public static PreceptDef IdeoRitualSeat;
        [MayRequireIdeology]
        public static PreceptDef ApparelDesired_Soft_Subordinate;
        [MayRequireIdeology]
        public static PreceptDef Festival;
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
        [MayRequire("rim.job.world")]
        public static HediffDef Penis;
        [MayRequire("rim.job.world")]
        public static HediffDef Vagina;
        [MayRequire("rim.job.world")]
        public static HediffDef Anus;
        [MayRequire("rim.job.world")]
        public static HediffDef Breasts;

        public static HediffDef HeartAttack;
        public static HediffDef Alzheimers;
        public static HediffDef Asthma;
        public static HediffDef BadBack;
        public static HediffDef Cataract;
        public static HediffDef Frail;
        public static HediffDef HearingLoss;
        public static HediffDef HeartArteryBlockage;
        public static HediffDef Flu;
        public static HediffDef Animal_Flu;
        public static HediffDef Animal_Plague;
        public static HediffDef GutWorms;
        public static HediffDef Malaria;
        public static HediffDef MuscleParasites;
        public static HediffDef SleepingSickness;
        public static HediffDef ChemicalDamageModerate;
        public static HediffDef ChemicalDamageSevere;
        public static HediffDef Cirrhosis;
        public static HediffDef TraumaSavant;
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