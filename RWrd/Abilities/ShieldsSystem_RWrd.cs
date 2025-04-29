using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using Verse;
using Electromagnetic.HarmonyPatchs;

namespace Electromagnetic.Abilities
{
    public static class ShieldsSystem_RWrd
    {
        private static HarmonyMethod ShieldMethod(this string name)
        {
            return new HarmonyMethod(typeof(ShieldsSystem_RWrd), name, null);
        }
        public static void ApplyDrawPatches()
        {
            bool flag = ShieldsSystem_RWrd.drawPatchesApplied;
            if (!flag)
            {
                ShieldsSystem_RWrd.drawPatchesApplied = true;
                HarmonyInit.PatchMain.instance.Patch(AccessTools.Method(typeof(Pawn), "SpawnSetup", null, null), null, "OnPawnSpawn".ShieldMethod(), null, null);
                HarmonyInit.PatchMain.instance.Patch(AccessTools.Method(typeof(Pawn), "DeSpawn", null, null), null, "OnPawnDespawn".ShieldMethod(), null, null);
                HarmonyInit.PatchMain.instance.Patch(AccessTools.Method(typeof(Pawn), "DrawAt", null, null), null, "PawnPostDrawAt".ShieldMethod(), null, null);
            }
        }
        public static void ApplyShieldPatches()
        {
            bool flag = ShieldsSystem_RWrd.shieldPatchesApplied;
            if (!flag)
            {
                ShieldsSystem_RWrd.shieldPatchesApplied = true;
                HarmonyInit.PatchMain.instance.Patch(AccessTools.Method(typeof(ThingWithComps), "PreApplyDamage", null, null), null, "PostPreApplyDamage".ShieldMethod(), null, null);
                HarmonyInit.PatchMain.instance.Patch(AccessTools.Method(typeof(Verb), "CanHitTarget", null, null), null, "CanHitTargetFrom_Postfix".ShieldMethod(), null, null);
            }
        }
        public static void OnPawnSpawn(Pawn __instance)
        {
            ShieldsSystem_RWrd.HediffDrawsByPawn.Add(__instance, __instance.health.hediffSet.hediffs.OfType<HediffWithComps>().SelectMany((HediffWithComps hediff) => hediff.comps).OfType<HediffComp_Draw_RWrd>().ToList<HediffComp_Draw_RWrd>());
        }
        public static void OnPawnDespawn(Pawn __instance)
        {
            ShieldsSystem_RWrd.HediffDrawsByPawn.Remove(__instance);
        }
        public static void PawnPostDrawAt(Pawn __instance, Vector3 drawLoc)
        {
            List<HediffComp_Draw_RWrd> list;
            bool flag = ShieldsSystem_RWrd.HediffDrawsByPawn.TryGetValue(__instance, out list);
            if (flag)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].DrawAt(drawLoc);
                }
            }
        }
        public static void PostPreApplyDamage(ThingWithComps __instance, ref DamageInfo dinfo, ref bool absorbed)
        {
            Pawn pawn;
            bool flag;
            if (!absorbed)
            {
                pawn = (__instance as Pawn);
                flag = (pawn == null);
            }
            else
            {
                flag = true;
            }
            bool flag2 = flag;
            if (!flag2)
            {
                pawn = (__instance as Pawn);
                foreach (HediffComp_Shield_RWrd hediffComp_Shield in pawn.health.hediffSet.hediffs.OfType<HediffWithComps>().SelectMany((HediffWithComps hediff) => hediff.comps).OfType<HediffComp_Shield_RWrd>())
                {
                    hediffComp_Shield.PreApplyDamage(ref dinfo, ref absorbed);
                    bool flag3 = absorbed;
                    if (flag3)
                    {
                        break;
                    }
                }
            }
        }
        public static void CanHitTargetFrom_Postfix(Verb __instance, ref bool __result)
        {
            bool flag;
            if (__result && __instance.CasterIsPawn)
            {
                Pawn casterPawn = __instance.CasterPawn;
                if (casterPawn != null)
                {
                    flag = casterPawn.health.hediffSet.hediffs.OfType<HediffWithComps>().SelectMany((HediffWithComps hediff) => hediff.comps).OfType<HediffComp_Shield_RWrd>().Any((HediffComp_Shield_RWrd shield) => !shield.AllowVerbCast(__instance));
                    goto IL_80;
                }
            }
            flag = false;
        IL_80:
            bool flag2 = flag;
            if (flag2)
            {
                __result = false;
            }
        }
        public static Dictionary<Pawn, List<HediffComp_Draw_RWrd>> HediffDrawsByPawn = new Dictionary<Pawn, List<HediffComp_Draw_RWrd>>();
        private static bool drawPatchesApplied;
        private static bool shieldPatchesApplied;
    }
}
