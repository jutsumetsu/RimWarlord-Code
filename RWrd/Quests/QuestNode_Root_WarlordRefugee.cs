using Electromagnetic.Core;
using Electromagnetic.ModExtension;
using RimWorld.QuestGen;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Electromagnetic.Quests
{
    public class QuestNode_Root_WarlordRefugee : QuestNode
    {
        public FloatRange durationDays = new FloatRange(10f, 20f);
        public FloatRange bountyTimePct = new FloatRange(0.2f, 0.4f);
        public float eliteChance = 0.25f;

        protected override bool TestRunInt(Slate slate)
        {
            Map map = slate.Get<Map>("map") ?? Find.CurrentMap;
            if (map == null)
                return false;
            return TryPickIssuer(out _);
        }

        protected override void RunInt()
        {
            Slate slate = QuestGen.slate;
            Quest quest = QuestGen.quest;
            Map map = slate.Get<Map>("map") ?? Find.CurrentMap;
            if (map == null)
                return;

            int durationTicks = Mathf.RoundToInt(durationDays.RandomInRange * 60000f);
            bool eliteTier = Rand.Chance(eliteChance);
            PawnKindDef kind = eliteTier
                ? RWrd_DefOf.RWrd_Pawn_WanderingWarlordElite
                : RWrd_DefOf.RWrd_Pawn_WanderingWarlord;

            List<FactionRelation> relations = new List<FactionRelation>();
            foreach (Faction other in Find.FactionManager.AllFactionsListForReading)
            {
                if (other == null || other.defeated || other.temporary || other.Hidden)
                    continue;
                FactionRelation relation = new FactionRelation(other, FactionRelationKind.Neutral);
                relation.baseGoodwill = 0;
                relations.Add(relation);
            }

            Faction hostFaction = FactionGenerator.NewGeneratedFactionWithRelations(
                RWrd_DefOf.RWrd_Faction_Wanderers, relations, hidden: true);
            hostFaction.temporary = true;
            Find.FactionManager.Add(hostFaction);

            Pawn target = quest.GeneratePawn_NewTemp(
                kind, hostFaction, map.Tile,
                allowAddictions: false,
                mustBeCapableOfViolence: true,
                ensureNonNumericName: true,
                forceGenerateNewPawn: true,
                developmentalStages: DevelopmentalStage.Adult,
                allowPregnant: false);

            slate.Set("target", target);

            string leaveSignal = QuestGenUtility.HardcodedSignalWithQuestID("warlordRefugee.Leave");
            string recruitedSignal = QuestGenUtility.HardcodedSignalWithQuestID("warlordRefugee.Recruited");
            string bountyEnableSignal = QuestGenUtility.HardcodedSignalWithQuestID("warlordRefugee.BountyEnable");
            string bountySuccessSignal = QuestGenUtility.HardcodedSignalWithQuestID("warlordRefugee.BountySuccess");

            quest.PawnJoinOffer(
                target,
                "WarlordRefugeeJoinLabel".Translate(target.Named("PAWN")),
                "WarlordRefugeeJoinTitle".Translate(target.Named("PAWN")),
                "WarlordRefugeeJoinText".Translate(target.Named("PAWN"), map.Parent.Named("MAP")),
                accepted: () => Find.SignalManager.SendSignal(new Signal(recruitedSignal, target.Named("SUBJECT"))),
                rejected: null,
                inSignal: null,
                outSignalPawnAccepted: null,
                outSignalPawnRejected: null,
                charity: false,
                sendLetterOnEnable: true);

            quest.PawnsArrive(
                new[] { target }, null, map.Parent, null,
                joinPlayer: true, null,
                "WarlordRefugeeArrivesLabel".Translate(),
                "WarlordRefugeeArrivesText".Translate(target.Named("PAWN")),
                null, null,
                isSingleReward: false,
                rewardDetailsHidden: false,
                sendStandardLetter: true);

            quest.ExtraFaction(hostFaction, new[] { target }, ExtraFactionType.MiniFaction, areHelpers: false, inSignalsRemovePawn: new List<string> { recruitedSignal, leaveSignal });
            quest.JoinPlayer(map.Parent, Gen.YieldSingle(target), joinPlayer: true, inSignal: recruitedSignal);
            quest.Leave(
                new[] { target }, leaveSignal,
                sendStandardLetter: false,
                leaveOnCleanup: false,
                inSignalRemovePawn: recruitedSignal,
                wakeUp: false);

            QuestPart_WarlordBounty bountyPart = new QuestPart_WarlordBounty
            {
                target = target,
                issuer = null,
                hostFaction = hostFaction,
                successSignal = bountySuccessSignal,
                inSignalEnable = bountyEnableSignal
            };
            quest.AddPart(bountyPart);

            int bountyDelay = Mathf.FloorToInt(durationTicks * bountyTimePct.RandomInRange);
            quest.Delay(bountyDelay, () =>
            {
                if (!TryPickIssuer(out Faction issuer))
                    return;
                if (target == null || target.Destroyed || target.Dead || !target.Spawned)
                    return;

                bountyPart.issuer = issuer;
                quest.GiveRewards(
                    new RewardsGeneratorParams
                    {
                        allowGoodwill = true,
                        allowRoyalFavor = false,
                        giverFaction = issuer,
                        rewardValue = 300f + target.GetPowerRoot().energy.level * 50f
                    },
                    bountySuccessSignal,
                    null, null, null, null,
                    asker: issuer.leader,
                    overridePopulationIntent: 0f);

                Find.SignalManager.SendSignal(new Signal(bountyEnableSignal));
            }, debugLabel: "WarlordBounty");

            quest.Delay(durationTicks, () => Find.SignalManager.SendSignal(new Signal(leaveSignal, target.Named("SUBJECT"))),
                debugLabel: "WarlordRefugeeLeave");

            quest.End(QuestEndOutcome.Success, 0, null, recruitedSignal);
            quest.End(QuestEndOutcome.Success, 0, null, leaveSignal);
            quest.End(QuestEndOutcome.Success, 0, null, bountySuccessSignal);
        }

        private static bool TryPickIssuer(out Faction issuer)
        {
            issuer = null;
            if (Faction.OfPlayer == null)
                return false;

            List<Faction> candidates = new List<Faction>();
            foreach (Faction f in Find.FactionManager.AllFactions)
            {
                if (f == null || f.IsPlayer || f.defeated || f.temporary || f.Hidden)
                    continue;
                if (!f.def.humanlikeFaction)
                    continue;
                if (f.def.PermanentlyHostileTo(Faction.OfPlayer.def))
                    continue;
                if (f.HostileTo(Faction.OfPlayer))
                    continue;

                WarlordFactionExtension ext = f.def.GetModExtension<WarlordFactionExtension>();
                if (ext == null || !ext.canIssueBounty || ext.bountyWeight <= 0f)
                    continue;
                if (f.leader == null)
                    continue;

                candidates.Add(f);
            }

            if (candidates.Count == 0)
                return false;

            float total = 0f;
            foreach (Faction f in candidates)
                total += f.def.GetModExtension<WarlordFactionExtension>().bountyWeight;

            float roll = Rand.Value * total;
            foreach (Faction f in candidates)
            {
                roll -= f.def.GetModExtension<WarlordFactionExtension>().bountyWeight;
                if (roll <= 0f)
                {
                    issuer = f;
                    return true;
                }
            }

            issuer = candidates[candidates.Count - 1];
            return true;
        }
    }
}
