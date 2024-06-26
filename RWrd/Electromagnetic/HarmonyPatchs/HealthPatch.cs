using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Electromagnetic.HarmonyPatchs
{
    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(Pawn_HealthTracker))]
    [HarmonyPatch(nameof(Pawn_HealthTracker.ShouldBeDeadFromLethalDamageThreshold))]
    internal class HealthPatch
    {
        [HarmonyPostfix]
		public static void LDTCorrection(ref bool __result)
        {
            __result = false;
        }
    }
}
