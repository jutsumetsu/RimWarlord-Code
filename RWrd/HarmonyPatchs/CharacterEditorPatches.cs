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

namespace Electromagnetic.HarmonyPatchs
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
                return AccessTools.Method("CharacterEditor.TraitTool:AddTrait");
            }
            [HarmonyPostfix]
            public static void Postfix(Pawn pawn, TraitDef traitDef)
            {
                if (traitDef == RWrd_DefOf.RWrd_Gifted && pawn.RaceProps.Humanlike)
                {
                    if (!pawn.IsHavePowerRoot())
                    {
                        Hediff hediff = HediffMaker.MakeHediff(RWrd_DefOf.Hediff_RWrd_PowerRoot, pawn);
                        pawn.health.AddHediff(hediff);
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
                return AccessTools.Method("CharacterEditor.HealthTool:RemoveHediff");
            }
            [HarmonyPostfix]
            public static void Postfix(Pawn pawn, Hediff hediff)
            {
                bool flag = pawn == null || hediff == null;
                if (!flag)
                {
                    if (hediff is Hediff_RWrd_PowerRoot) hediff.PostRemoved();
                }
            }
        }
    }
}
