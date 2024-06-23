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
        public static HediffDef Hediff_RWrd_PowerRoot;
        public static RWrd_RouteDef Base;
        public static TraitDef RWrd_Gifted;
    }
}