using Electromagnetic.Core;
using Electromagnetic.Quests;
using HarmonyLib;
using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Electromagnetic.HarmonyPatchs
{
    public class QuestPatch
    {
        [HarmonyPatch(typeof(SettlementDefeatUtility), nameof(SettlementDefeatUtility.CheckDefeated))]
        public static class SettlementDefeatUtility_BaakOffering_Patch
        {
            private static Map mapForPostfix;
            private static PlanetTile tileForPostfix;

            [HarmonyPrefix]
            private static bool Prefix(Settlement factionBase)
            {
                if (factionBase == null || factionBase.Faction == null ||
                    factionBase.Faction == RimWorld.Faction.OfPlayer)
                    return true;

                QuestPart_BaakOfferSettlement part = Find.QuestManager.QuestsListForReading
                    .Where(q => q.State == QuestState.Ongoing)
                    .SelectMany(q => q.PartsListForReading)
                    .OfType<QuestPart_BaakOfferSettlement>()
                    .FirstOrDefault(p => p.settlement == factionBase);
                if (part == null)
                    return true;

                if (part.defeated)
                    return false;

                Map map = factionBase.Map;
                if (map == null || !SettlementDefeatUtility.IsDefeated(map, factionBase.Faction))
                    return true;

                RimWorld.Faction baak = Find.FactionManager.FirstFactionOfDef(RWrd_DefOf.RWrd_Faction_Empire_Baak);
                if (baak == null || baak.defeated)
                    return true;

                part.tile = factionBase.Tile;
                part.settlementName = factionBase.Label;
                part.defeated = true;
                return false;
            }
        }
    }
}
