using System;
using System.Collections.Generic;
using System.Reflection;
using Electromagnetic.Core;
using HarmonyLib;
using Verse;

namespace Electromagnetic.HarmonyPatchs
{
    [HarmonyPatch]
    public class PostFix_EBFEndpoints_GetMaxHealthWithEBF
    {
        public static bool Prepare()
        {
            return ModDetector.EBFIsLoaded;
        }
        public static MethodBase TargetMethod()
        {
            return AccessTools.Method("EBF.EBFEndpoints:GetMaxHealthWithEBF", null, null);
        }
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, ref float __result)
        {
            if (pawn.IsHaveRoot())
            {
                Hediff_RWrd_PowerRoot root = pawn.GetRoot();
                int pff = root.energy.PowerFlowFactor();
                int level = root.energy.level + 1;
                int multiplier = pff + level;
                if (root.energy.IsUltimate)
                {
                    multiplier += (int)Math.Floor(root.energy.PowerEnergy);
                }
                __result *= multiplier;
            }
        }
    }
}
