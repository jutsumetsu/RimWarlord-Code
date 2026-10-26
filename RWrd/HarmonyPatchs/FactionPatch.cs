using Electromagnetic.Core;
using Electromagnetic.ModExtension;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Electromagnetic.HarmonyPatchs
{
    public class FactionPatch
    {
        //派系文化图标补丁-Def
        [HarmonyPatch(typeof(IdeoFoundation))]
        [HarmonyPatch(nameof(IdeoFoundation.GetRandomIconDef))]
        class FactionIconDefPatch
        {
            [HarmonyPostfix]
            public static void Postfix(Ideo ideo, ref IdeoIconDef __result)
            {
                if (ModsConfig.IdeologyActive)
                {
                    string IdeoName = ideo.name;
                    if (ModDetector.BaakIsLoaded)
                    {
                        if (IdeoName == RWrd_DefOf.RWrd_Faction_Empire_Baak.ideoName)
                        {
                            __result = RWrd_DefOf.DownBurst;
                        }
                    }
                }
            }

        }
        //派系文化图标补丁-颜色
        [HarmonyPatch(typeof(IdeoFoundation))]
        [HarmonyPatch(nameof(IdeoFoundation.GetRandomColorDef))]
        class FactionIconColorPatch
        {
            [HarmonyPostfix]
            public static void Postfix(Ideo ideo, ref ColorDef __result)
            {
                if (ModsConfig.IdeologyActive)
                {
                    string IdeoName = ideo.name;
                    if (ModDetector.BaakIsLoaded)
                    {
                        if (IdeoName == RWrd_DefOf.RWrd_Faction_Empire_Baak.ideoName) 
                        {
                            __result = RWrd_DefOf.BaakRed;
                        }
                    }
                }
            } 

        }
        //派系文化修正补丁
        [HarmonyPatch(typeof(IdeoGenerator), nameof(IdeoGenerator.MakeFixedIdeo))]
        class FactionIdeoPatch
        {
            [HarmonyPostfix]
            public static void Postfix(Ideo __result, IdeoGenerationParms parms)
            {
                if (ModsConfig.IdeologyActive)
                {
                    string IdeoName = __result.name;
                    if (ModDetector.BaakIsLoaded)
                    {
                        if (IdeoName == RWrd_DefOf.RWrd_Faction_Empire_Baak.ideoName)
                        {
                            List<Precept_Ritual> rituals = __result.GetAllPreceptsOfType<Precept_Ritual>().ToList();
                            for (int i = 0; i < rituals.Count() ; i++)
                            {
                                if (rituals[i].def == RWrd_DefOf.Festival)
                                {
                                    __result.RemovePrecept(rituals[i]);
                                }
                            }
                            if (__result.GetPrecept(RWrd_DefOf.IdeoRole_Leader) is Precept_RoleSingle leader)
                            {
                                leader.ApparelRequirements.Clear();
                            }
                            if (__result.GetPrecept(RWrd_DefOf.IdeoRole_Moralist) is Precept_RoleSingle moralist)
                            {
                                moralist.ApparelRequirements.Clear();
                                moralist.SetName("RWrd_BaakMoralist".Translate());
                            }
                            if (__result.GetPrecept(PreceptDefOf.Funeral) is Precept_Ritual funeral)
                            {
                                funeral.SetName("RWrd_Funeral".Translate());
                            }
                            if (__result.GetPrecept(PreceptDefOf.IdeoBuilding) is Precept_Building building)
                            {
                                building.ThingDef = RWrd_DefOf.Altar_Grand;
                                building.SetName(RWrd_DefOf.Altar_Grand.label);
                            }
                            if (__result.GetPrecept(RWrd_DefOf.IdeoRitualSeat) is Precept_RitualSeat ritualSeat)
                            {
                                ritualSeat.ThingDef = RWrd_DefOf.Pew;
                            }

                            __result.SetIcon(RWrd_DefOf.DownBurst, RWrd_DefOf.BaakRed);
                        }
                    }
                    if (parms.forFaction != null)
                    {
                        if (parms.forFaction.GetModExtension<IdeoCustom>() is IdeoCustom custom) __result.resolveRWrdIdeoCustom(custom);
                    }
                }
            }

        }
        //派系初始关系补丁
        [HarmonyPatch(typeof(RimWorld.Faction), nameof(RimWorld.Faction.TryMakeInitialRelationsWith))]
        class FactionInitialRelationPatch
        {
            [HarmonyPrefix]
            public static bool Prefix(RimWorld.Faction __instance, RimWorld.Faction other)
            {
                bool result = true;
                if (RimWorld.Faction.OfPlayerSilentFail == null)
                {
                    if (__instance.def.isPlayer && __instance.def.HasModExtension<RelationCustom>())
                    {
                        FactionDef overlord = __instance.def.GetModExtension<RelationCustom>().overlord;
                        if (overlord != null)
                        {
                            Log.Message($"Player faction's overlord: {overlord.defName}");
                            if (__instance.RelationWith(other, true) != null)
                            {
                                return result;
                            }
                            if (other.def == overlord) Tools.MakeAllyRelation(__instance, other);
                        }
                    }
                }
                /*if (__instance.def.HasModExtension<RelationCustom>())
                {
                    List<FactionDef> forcedAllies = __instance.def.GetModExtension<RelationCustom>().forcedAllies;
                    for (int i = 0; i < forcedAllies.Count; i++)
                    {
                        if (other.def == forcedAllies[i]) Tools.MakeAllyRelation(__instance, other);
                    }
                }*/
                return result;

            }
        }
        //通讯台好感度花费补丁
        [HarmonyPatch(typeof(RimWorld.Faction), nameof(RimWorld.Faction.CalculateAdjustedGoodwillChange))]
        class CommsConsolePatch
        {
            [HarmonyPostfix]
            public static void Postfix(RimWorld.Faction __instance, RimWorld.Faction other, ref int __result, int goodwillChange)
            {
                if (goodwillChange >= 0) return;
                if (__instance.IsPlayer)
                {
                    if (__instance.def.HasModExtension<RelationCustom>())
                    {
                        if (other.def == __instance.def.GetModExtension<RelationCustom>().overlord) __result = 0;
                    }
                }
            }
        }
        //宗主锁定好感补丁
        [HarmonyPatch(typeof(RimWorld.Faction), nameof(RimWorld.Faction.CanChangeGoodwillFor))]
        class OverlordFactionLockGoodwillPatch
        {
            [HarmonyPostfix]
            public static void Postfix(RimWorld.Faction __instance, ref bool __result, RimWorld.Faction other)
            {
                if (RimWorld.Faction.OfPlayerSilentFail == null) return;
                if (!RimWorld.Faction.OfPlayer.def.HasModExtension<RelationCustom>()) return;
                FactionDef overlord = RimWorld.Faction.OfPlayer.def.GetModExtension<RelationCustom>().overlord;
                if (overlord == null) return;
                if ((__instance.IsPlayer && other.def == overlord) || (other.IsPlayer && __instance.def == overlord)) __result = false;
            }
        }
        //宗主免疫模因负面
        [HarmonyPatch(typeof(GoodwillSituationWorker_MemeCompatibility), nameof(GoodwillSituationWorker_MemeCompatibility.GetNaturalGoodwillOffset))]
        class OverlordNoMemeNegative
        {
            [HarmonyPostfix]
            public static void Postfix(ref int __result, RimWorld.Faction other)
            {
                if (RimWorld.Faction.OfPlayerSilentFail == null) return;
                if (!RimWorld.Faction.OfPlayer.def.HasModExtension<RelationCustom>()) return;
                FactionDef overlord = RimWorld.Faction.OfPlayer.def.GetModExtension<RelationCustom>().overlord;
                if (overlord == null) return;
                if (__result < 0 || other.def == overlord) __result = 0;
            }
        }
        //宗主免疫定居点过近判定
        [HarmonyPatch(typeof(SettlementProximityGoodwillUtility), nameof(SettlementProximityGoodwillUtility.AppendProximityGoodwillOffsets))]
        class OverlordIsNeighborPatch
        {
            [HarmonyPostfix]
            public static void Postfix(List<Pair<Settlement, int>> outOffsets)
            {
                if (RimWorld.Faction.OfPlayerSilentFail == null) return;
                if (!RimWorld.Faction.OfPlayer.def.HasModExtension<RelationCustom>()) return;
                FactionDef overlord = RimWorld.Faction.OfPlayer.def.GetModExtension<RelationCustom>().overlord;
                if (overlord == null) return;
                for (int i = 0; i < outOffsets.Count; i++)
                {
                    if (outOffsets[i].First.Faction.def == overlord) outOffsets.Remove(outOffsets[i]);
                }
            }
        }
        /*[HarmonyPatch(typeof(Precept_Ritual), nameof(Precept_Ritual.GenerateNameRaw))]
        class IdeoRitualNameTracker
        {
            [HarmonyPostfix]
            public static void Postfix(Precept_Ritual __instance)
            {
                string IdeoName = __instance.ideo.name;
                Log.Message($"Now make ritual name for {IdeoName}");
                switch (IdeoName)
                {
                    case "Millennial Throne":
                        Log.Message($"RitualDef: {__instance.def.defName}");
                        break;
                    case "千年帝位":
                        Log.Message($"RitualDef: {__instance.def.defName}");
                        break;
                }
            }
        }*/
        [HarmonyPatch(typeof(Ideo), nameof(Ideo.SetIcon))]
        class IdeoIconSetTracker
        {
            [HarmonyPostfix]
            public static void Postfix(Ideo __instance, IdeoIconDef iconDef)
            {
                string IdeoName = __instance.name;
                if (IdeoName == RWrd_DefOf.RWrd_Faction_Empire_Baak.ideoName)
                {
                    Log.Message($"Set Icon Name: {iconDef.defName}");
                }
            }
        }
    }
}
