using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Electromagnetic.Core;
using Verse;
using static Verse.DamageWorker;

namespace Electromagnetic.HarmonyPatchs
{
    public class DevourPatch
    {
        public static void ApplyBigSmallPatches(Harmony harmony)
        {
            Type engulfHediff = AccessTools.TypeByName("BigAndSmall.EngulfHediff");
            if (engulfHediff == null)
            {
                return;
            }

            Type slimeEngulf = AccessTools.TypeByName("BigAndSmall.CompAbilityEffect_SlimeEngluf_Abstract");
            Type biteDevour = AccessTools.TypeByName("BigAndSmall.DamageWorker_BiteDevourDmg");
            int patched = 0;

            if (TryPatch(harmony, engulfHediff, "Engulf",
                new[] { typeof(Thing) }, "BeforeEngulf"))
            {
                patched++;
            }
            if (slimeEngulf != null && TryPatch(harmony, slimeEngulf, "DoEngulf",
                new[] { typeof(Pawn), typeof(Pawn) }, "BeforeAbility"))
            {
                patched++;
            }
            if (biteDevour != null && TryPatch(harmony, biteDevour, "FinalizeAndAddInjury_Postfix",
                new[]
                {
                    typeof(Pawn), typeof(Hediff_Injury), typeof(DamageInfo),
                    typeof(DamageResult), typeof(DamageWorker_AddInjury)
                }, "BeforeBite"))
            {
                patched++;
            }

            Log.Message(string.Format("[RimWarlord] Big & Small devour compatibility: {0}/3 patched", patched));
        }

        private static bool TryPatch(Harmony harmony, Type type, string methodName,
            Type[] parameters, string prefixName)
        {
            MethodInfo method = AccessTools.DeclaredMethod(type, methodName, parameters);
            if (method == null)
            {
                Log.Warning(string.Format("[RimWarlord] Big & Small method not found: {0}.{1}",
                    type.FullName, methodName));
                return false;
            }
            harmony.Patch(method, prefix: new HarmonyMethod(typeof(DevourPatch), prefixName)
            {
                priority = 800
            });
            return true;
        }

        public static bool BeforeEngulf(Thing __0, ref bool __result)
        {
            Pawn pawn = __0 as Pawn;
            if (pawn == null || !pawn.IsDevourImmune())
            {
                return true;
            }
            __result = false;
            return false;
        }

        public static bool BeforeAbility(Pawn __1)
        {
            return !__1.IsDevourImmune();
        }

        public static bool BeforeBite(Pawn __0)
        {
            return !__0.IsDevourImmune();
        }
    }
}
