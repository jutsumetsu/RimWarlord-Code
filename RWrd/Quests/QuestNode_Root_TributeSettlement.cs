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
using Electromagnetic.Factions;

namespace Electromagnetic.Quests
{
    public class QuestNode_Root_TributeSettlement : QuestNode
    {
        public SlateRef<FactionDef> receiverFaction;

        protected override bool TestRunInt(Slate slate)
        {
            Map map = slate.Get<Map>("map") ?? Find.CurrentMap;
            Faction receiver = Find.FactionManager.FirstFactionOfDef(receiverFaction.GetValue(slate));
            if (receiver == null || receiver.defeated || receiver.leader == null)
                return false;

            Faction playerOverlord = RWrd_FactionUtility.GetOverlord(Faction.OfPlayer);
            if (playerOverlord != receiver)
                return false;

            return TryFindTargetSettlement(map, receiver, out _);
        }

        protected override void RunInt()
        {
            Slate slate = QuestGen.slate;
            Quest quest = QuestGen.quest;
            Map map = slate.Get<Map>("map") ?? Find.CurrentMap;
            if (map == null)
                return;

            Faction receiver = Find.FactionManager.FirstFactionOfDef(receiverFaction.GetValue(slate));
            if (receiver == null || receiver.defeated || receiver.leader == null)
                return;

            Faction playerOverlord = RWrd_FactionUtility.GetOverlord(Faction.OfPlayer);
            if (playerOverlord != receiver)
                return;

            if (!TryFindTargetSettlement(map, receiver, out Settlement settlement))
                return;

            slate.Set("map", map);
            slate.Set("targetSettlement", settlement);
            slate.Set("targetFaction", settlement.Faction);
            slate.Set("asker", receiver.leader);
            slate.Set("askerFaction", receiver);

            string questTag = QuestGenUtility.HardcodedSignalWithQuestID("targetSettlement");
            QuestUtility.AddQuestTag(settlement, questTag);

            string offerSignal = QuestGenUtility.HardcodedSignalWithQuestID("targetSettlement.Offered");
            string mapRemovedSignal = questTag + ".MapRemoved";

            quest.AddPart(new QuestPart_TributeSettlement(
                settlement, receiver, offerSignal, mapRemovedSignal));

            float points = slate.Get("points", 0f);
            quest.GiveRewards(new RewardsGeneratorParams
            {
                allowGoodwill = true,
                allowRoyalFavor = true,
                giverFaction = receiver,
                rewardValue = Mathf.Max(points, 300f) * 1.5f
            }, offerSignal, null, null, null, null, asker: receiver.leader,
               overridePopulationIntent: 0f);

            quest.End(QuestEndOutcome.Success, 0, null, offerSignal);
        }

        private static bool TryFindTargetSettlement(Map map, Faction receiver, out Settlement settlement)
        {
            if (receiver == null || receiver.defeated || map == null || !map.Tile.Valid)
            {
                settlement = null;
                return false;
            }

            settlement = Find.WorldObjects.SettlementBases
                .Where(s => s.Faction != null && !s.Faction.IsPlayer && !s.Faction.defeated
                    && s.Faction.HostileTo(receiver)
                    && s.Tile.Valid && map.Tile.Valid
                    && Find.WorldGrid.ApproxDistanceInTiles(map.Tile, s.Tile) <= 40f)
                .RandomElementWithFallback();
            return settlement != null;
        }
    }
}
