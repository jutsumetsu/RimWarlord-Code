using HarmonyLib;
using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Electromagnetic.Core;

namespace Electromagnetic.HarmonyPatchs
{
    public class Pawn_Patch
    {
        [HarmonyPatch(typeof(Pawn))]
        [HarmonyPatch(nameof(Pawn.Tick))]
        class Patch1
        {
            [HarmonyPostfix]
            public static void TickPostFix(Pawn __instance)
            {
                if (__instance.RaceProps.Humanlike)
                {
                    if (Find.TickManager.TicksGame % 600 == 0)
                    {
                        bool flag = __instance.IsHaveRoot();
                        if (!flag)
                        {
                            bool flag2 = __instance.story.traits.HasTrait(RWrd_DefOf.RWrd_Gifted);
                            if (flag2)
                            {
                                Hediff hediff = HediffMaker.MakeHediff(RWrd_DefOf.Hediff_RWrd_PowerRoot, __instance, null);
                                __instance.health.AddHediff(hediff, null, null, null);
                            }
                        }
                    }
                }
            }
        }
        //测试提交
        [HarmonyPatch(typeof(StatWorker_MeleeDamageAmount), "GetMeleeDamage")]
        public static void Postfix(StatRequest req, ref float __result)
        {
            Pawn pawn = req.Thing as Pawn;
            if (pawn != null)
            {
                Hediff_RWrd_PowerRoot root = pawn.GetRoot();
                if (root != null)
                {
                    float completeRealmValue = root.energy.completerealm;
                    int powerFlowValue = root.energy.powerflow;
                    if (completeRealmValue >= 0.1)
                    {
                        float num1 = (int)Math.Floor(completeRealmValue * 10);

                        __result *= completeRealmValue;
                    }
                }
            }
        }

    }
}
