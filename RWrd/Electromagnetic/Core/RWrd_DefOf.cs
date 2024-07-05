using System;
using System.Collections.Generic;
using RimWorld;
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
        public static RWrd_RouteDef Base;
        public static TraitDef RWrd_Gifted;
        //能力
        public static AbilityDef RWrd_Ability_CellRecombination;
        //特效
        public static FleckDef RWrd_ElectricClawAttractFleck;
        public static FleckDef RWrd_ElectricClawFleck;
        //建筑
        public static ThingDef RWrd_Building_TrainingSpot;
        //Job
        public static JobDef RWrd_General_Training;
        //Hediff
        public static HediffDef RWrd_Regenerating;
    }
}