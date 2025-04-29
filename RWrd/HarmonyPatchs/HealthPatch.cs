using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Electromagnetic.Core;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Electromagnetic.HarmonyPatchs
{
    [HarmonyPatch(typeof(Pawn_HealthTracker))]
    [HarmonyPatch(nameof(Pawn_HealthTracker.ShouldBeDeadFromLethalDamageThreshold))]
    internal class HealthPatch
    {
        [HarmonyPostfix]
		public static void LDTCorrection(Pawn_HealthTracker __instance ,ref bool __result)
        {
            bool flag = false;
            for (int i = 0; i < __instance.hediffSet.hediffs.Count; i++)
            {
                if (__instance.hediffSet.hediffs[i] is Hediff_RWrd_PowerRoot)
                {
                    flag = true;
                }
            }
            if (flag)
            {
                __result = false;
            }
        }
    }
}
