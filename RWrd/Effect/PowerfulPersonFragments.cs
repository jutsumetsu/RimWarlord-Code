using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;
using Electromagnetic.Core;

namespace Electromagnetic.Effect
{
    public class PowerfulPersonFragments : FragmentsWorker
    {
        public override void DoPatches()
        {
            base.Patch(AccessTools.Method(typeof(Pawn), "Kill", null, null), AccessTools.Method(typeof(PowerfulPersonFragments), "Prefix", null, null), AccessTools.Method(typeof(PowerfulPersonFragments), "Postfix", null, null), null);
        }

        public static void Prefix(DamageInfo? dinfo, Pawn __instance, ref (Map map, IntVec3 pos) __state)
        {
            __state = (__instance.Map, __instance.Position);
        }


        public static void Postfix(DamageInfo? dinfo, Pawn __instance, (Map map, IntVec3 pos) __state)
        {
            bool flag = __state.map != null;
            if (flag)
            {
                bool hasHediffA = __instance.IsHavePowerRoot();
                if (hasHediffA)
                {
                    FleckMaker.Static(__state.pos, __state.map, PowerfulPersonFragments.Fragments(Rand.RangeInclusive(0, 3)), 1.5f);
                    FleckMaker.Static(__state.pos, __state.map, PowerfulPersonFragments.Fragments(Rand.RangeInclusive(0, 3)), 2f);
                    // 移除 Pawn 的尸体
                    if (__instance.Corpse != null)
                    {
                        __instance.Corpse.Destroy();
                    }
                }
            }
        }

        public static FleckDef Fragments(int index)
        {
            FleckDef result;
            switch (index)
            {
                case 0:
                    result = RWrd_DefOf.RWrd_FragmentsA;
                    break;
                case 1:
                    result = RWrd_DefOf.RWrd_FragmentsB;
                    break;
                case 2:
                    result = RWrd_DefOf.RWrd_FragmentsC;
                    break;
                case 3:
                    result = RWrd_DefOf.RWrd_FragmentsD;
                    break;
                default:
                    result = RWrd_DefOf.RWrd_FragmentsA;
                    break;
            }
            return result;
        }
    }
}
