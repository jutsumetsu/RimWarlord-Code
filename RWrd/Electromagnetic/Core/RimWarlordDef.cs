using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Electromagnetic.Core
{
    public class RimWarlordDef : Def
    {
        public static Dictionary<int, RimWarlordDef> Dict
        {
            get
            {
                bool flag = RimWarlordDef.cache.NullOrEmpty<int, RimWarlordDef>();
                if (flag)
                {
                    foreach (RimWarlordDef rimWarlordDef in DefDatabase<RimWarlordDef>.AllDefsListForReading)
                    {
                        RimWarlordDef.cache[rimWarlordDef.level] = rimWarlordDef;
                    }
                }
                return RimWarlordDef.cache;
            }
        }

        public int level = 0;

        [NoTranslate]
        public List<string> tags;
        //能量上下限
        public float MaxEnergy = 100f;
        public float MinEnergy = 1f;
        //经验需求
        public float EXP = 0f;
        //完全境界和力量流量上限
        public int MaxPowerFlow = 100000000;
        public float MaxCompleteRealm = 10000;

        public HediffDef PowerBuff;

        public List<StatModifier> statOffsets;
        public List<StatModifier> statFactors;

        private static readonly Dictionary<int, RimWarlordDef> cache = new Dictionary<int, RimWarlordDef> ();
    }
}
