using Electromagnetic.Core;
using RimWorld.Planet;
using RimWorld.QuestGen;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;

namespace Electromagnetic.Quest
{
    public class QuestNode_Root_BaakTribute : QuestNode
    {
        protected override bool TestRunInt(Slate slate)
        {
            Map map = slate.Get<Map>("map") ?? Find.CurrentMap;
            return TryFindTargetSettlement(map, out _);
        }

        protected override void RunInt()
        {
            Slate slate = QuestGen.slate;
            RimWorld.Quest quest = QuestGen.quest;
            Map map = slate.Get<Map>("map") ?? Find.CurrentMap;
            if (map == null)
            {
                return;
            }
            slate.Set("map", map);
            RimWorld.Faction baak = Find.FactionManager.FirstFactionOfDef(RWrd_DefOf.RWrd_Faction_Empire_Baak);
            if (baak == null || baak.defeated)
            {
                return;
            }

            if (!TryFindTargetSettlement(map, baak, out Settlement settlement))
            {
                return;
            }

            slate.Set("targetSettlement", settlement);
            slate.Set("targetFaction", settlement.Faction);
            slate.Set("asker", baak.leader);
            slate.Set("askerFaction", baak);

            // 信号按 quest 唯一化，避免多个上供任务同时被同一个全局信号结束
            string questTag = QuestGenUtility.HardcodedSignalWithQuestID("targetSettlement");
            QuestUtility.AddQuestTag(settlement, questTag);

            string offerSignal = QuestGenUtility.HardcodedSignalWithQuestID("targetSettlement.Offered");
            string mapRemovedSignal = questTag + ".MapRemoved";
            quest.AddPart(new QuestPart_BaakOfferSettlement(settlement, offerSignal, mapRemovedSignal));

            float points = slate.Get("points", 0f);

            quest.GiveRewards(new RewardsGeneratorParams
            {
                allowGoodwill = true,
                allowRoyalFavor = true,
                giverFaction = baak,
                rewardValue = Mathf.Max(points, 300f) * 1.5f
            }, offerSignal, null, null, null, null, asker: baak.leader,
               overridePopulationIntent: 0f);

            quest.End(QuestEndOutcome.Success, 0, null, offerSignal);
        }

        private static bool TryFindTargetSettlement(Map map, out Settlement settlement)
        {
            RimWorld.Faction baak = Find.FactionManager.FirstFactionOfDef(RWrd_DefOf.RWrd_Faction_Empire_Baak);
            return TryFindTargetSettlement(map, baak, out settlement);
        }

        private static bool TryFindTargetSettlement(Map map, RimWorld.Faction baak, out Settlement settlement)
        {
            if (baak == null || baak.defeated || map == null || !map.Tile.Valid)
            {
                settlement = null;
                return false;
            }

            settlement = Find.WorldObjects.SettlementBases
                .Where(s => s.Faction != null && !s.Faction.IsPlayer && !s.Faction.defeated
                    && s.Faction.HostileTo(baak)
                    && s.Tile.Valid && map.Tile.Valid
                    && Find.WorldGrid.ApproxDistanceInTiles(map.Tile, s.Tile) <= 40f)
                .RandomElementWithFallback();
            return settlement != null;
        }
    }
}
