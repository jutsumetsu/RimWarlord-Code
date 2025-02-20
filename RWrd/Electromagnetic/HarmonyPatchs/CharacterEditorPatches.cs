using Electromagnetic.Core;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Electromagnetic.Electromagnetic.HarmonyPatchs
{
    public class CharacterEditorPatches
    {
        [HarmonyPatch]
        public class AddTraitPatch
        {
            public static bool Prepare()
            {
                return ModDetector.CEIsLoaded;
            }
            public static MethodBase TargetMethod()
            {
                return AccessTools.Method("ay:a", new[]
                {
                    typeof(Pawn),
                    typeof(TraitDef),
                    typeof(TraitDegreeData),
                    typeof(bool),
                    typeof(bool),
                    typeof(Trait)
                });
            }
            [HarmonyPostfix]
            public static void Postfix(Pawn A_0, TraitDef A_1)
            {
                if (A_1 == RWrd_DefOf.RWrd_Gifted && A_0.RaceProps.Humanlike)
                {
                    if (!A_0.IsHavePowerRoot())
                    {
                        Hediff hediff = HediffMaker.MakeHediff(RWrd_DefOf.Hediff_RWrd_PowerRoot, A_0);
                        A_0.health.AddHediff(hediff);
                    }
                }
            }
        }
        [HarmonyPatch]
        public class RemoveHediffPatch
        {
            public static bool Prepare()
            {
                return ModDetector.CEIsLoaded;
            }
            public static MethodBase TargetMethod()
            {
                return AccessTools.Method("a2:a", new[] { typeof(Pawn), typeof(Hediff) });
            }
            [HarmonyPostfix]
            public static void Postfix(Pawn A_0, Hediff A_1)
            {
                bool flag = A_0 == null || A_1 == null;
                if (!flag)
                {
                    if (A_1 is Hediff_RWrd_PowerRoot) A_1.PostRemoved();
                }
            }
        }
    }
}
