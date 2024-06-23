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
        //建筑
        public static ThingDef RWrd_Building_TrainingSpot;
    }
}