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
    public class RJWPatches
    {
        public class RJWPregnancyPatch
        {
            public static void Postfix(Pawn mother, Pawn father, Pawn baby)
            {
                if (baby is Pawn babyPawn)
                {
                    if (DebugSettings.godMode) Log.Message("RimWarlord RJW Patch Success");
                    bool hasRootParent = (mother != null && mother.IsHavePowerRoot() == true) ||
                                         (father != null && father.IsHavePowerRoot() == true);

                    if (hasRootParent)
                    {
                        Hediff root = Tools.MakePowerRoot(RWrd_DefOf.Hediff_RWrd_PowerRoot, babyPawn, false, true);
                        babyPawn.health.AddHediff(root);
                        if (!babyPawn.story.traits.HasTrait(RWrd_DefOf.RWrd_Gifted)) babyPawn.story.traits.GainTrait(new Trait(RWrd_DefOf.RWrd_Gifted));
                    }
                }
            }
        }
    }
}
