using Electromagnetic.Core;
using RimWorld.Planet;
using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Electromagnetic.Quests
{
    public class QuestNode_Root_BaakBounty : QuestNode
    {
        protected override bool TestRunInt(Slate slate)
        {
            Map map = slate.Get<Map>("map") ?? Find.CurrentMap;
            return map != null && map.IsPlayerHome && TryFindTargetFaction(out _);
        }

        protected override void RunInt()
        {
            Slate slate = QuestGen.slate;
            Quest quest = QuestGen.quest;
            Map map = slate.Get<Map>("map") ?? Find.CurrentMap;
            if (map == null)
            {
                return;
            }
            slate.Set("map", map);
            float points = slate.Get("points", 0f);
            RimWorld.Faction baak = Find.FactionManager.FirstFactionOfDef(RWrd_DefOf.RWrd_Faction_Empire_Baak);
            if (baak == null || baak.defeated)
            {
                return;
            }

            if (!TryFindTargetFaction(out RimWorld.Faction faction))
            {
                return;
            }

            PawnKindDef kind = faction.RandomPawnKind();
            if (kind == null || baak.leader == null)
            {
                return;
            }

            Pawn target = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
                kind, faction,
                PawnGenerationContext.NonPlayer, map.Tile,
                forceGenerateNewPawn: true, allowDead: false, allowDowned: false,
                canGeneratePawnRelations: true, mustBeCapableOfViolence: true));

            slate.Set("target", target);
            slate.Set("targetFaction", faction);
            slate.Set("asker", baak.leader);
            slate.Set("askerFaction", baak);

            if (!TileFinder.TryFindNewSiteTile(out PlanetTile siteTile, map.Tile, 12, 40))
            {
                return;
            }

            Site site = QuestGen_Sites.GenerateSite(
                new[] { new SitePartDefWithParams(SitePartDefOf.BanditCamp, new SitePartParams
                {
                    threatPoints = Mathf.Max(points, 300f)
                }) },
                siteTile,
                faction);

            slate.Set("site", site);
            quest.SpawnWorldObject(site);

            string siteMapGenerated = QuestGenUtility.HardcodedSignalWithQuestID("site.MapGenerated");
            string targetDestroyed = QuestGenUtility.HardcodedSignalWithQuestID("target.Destroyed");
            string siteMapRemoved = QuestGenUtility.HardcodedSignalWithQuestID("site.MapRemoved");

            quest.AddPart(new QuestPart_BaakSpawnTarget(target, site, siteMapGenerated));

            quest.GiveRewards(new RewardsGeneratorParams
            {
                allowGoodwill = true,
                allowRoyalFavor = true,
                giverFaction = baak,
                rewardValue = points * 1.5f
            }, targetDestroyed, null, null, null, null, asker: baak.leader, overridePopulationIntent: 0f);
            quest.End(QuestEndOutcome.Success, 0, null, targetDestroyed);
            quest.End(QuestEndOutcome.Fail, 0, null, siteMapRemoved);
        }

        private static bool TryFindTargetFaction(out RimWorld.Faction faction)
        {
            RimWorld.Faction baak = Find.FactionManager.FirstFactionOfDef(RWrd_DefOf.RWrd_Faction_Empire_Baak);
            if (baak == null || baak.defeated)
            {
                faction = null;
                return false;
            }
            faction = Find.FactionManager.AllFactions
                .Where(f => f != null && !f.IsPlayer && !f.Hidden && !f.temporary
                    && f.def.humanlikeFaction && !f.defeated
                    && f != baak && f.HostileTo(baak)
                    && f.RandomPawnKind() != null)
                .RandomElementWithFallback();
            return faction != null;
        }
    }
}
