using Electromagnetic.Setting;
using Electromagnetic.Factions;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Electromagnetic.UI
{
    public class RWrd_VassalDialog
    {
        public static DiaOption TryMakeJoinOption(Pawn negotiator, RimWorld.Faction faction)
        {
            if (negotiator == null || faction == null || faction.IsPlayer || faction.defeated)
            {
                return null;
            }

            RimWorld.Faction player = RimWorld.Faction.OfPlayerSilentFail;
            if (player == null)
            {
                return null;
            }

            if (!RWrdSettings.AllowAnyFactionVassal
                && faction.def.defName.IndexOf("RWrd", StringComparison.Ordinal) < 0)
            {
                return null;
            }

            RimWorld.Faction currentOverlord = RWrd_FactionUtility.GetOverlord(player);
            if (currentOverlord == faction)
            {
                DiaOption already = new DiaOption("RWrd_AlreadyVassalOf".Translate(faction.Name));
                already.Disable("RWrd_AlreadyVassal".Translate());
                return already;
            }
            if (currentOverlord != null)
            {
                DiaOption blocked = new DiaOption("RWrd_RequestJoinFaction".Translate(faction.Name));
                blocked.Disable("RWrd_AlreadyHasOverlord".Translate(currentOverlord.Name));
                return blocked;
            }

            FactionRelationKind relation = faction.PlayerRelationKind;
            bool hasTitle = HasColonistWithTitle(faction);
            if (relation == FactionRelationKind.Hostile)
            {
                return null;
            }
            if (relation == FactionRelationKind.Neutral && !hasTitle)
            {
                DiaOption needTitle = new DiaOption("RWrd_RequestJoinFaction".Translate(faction.Name));
                needTitle.Disable("RWrd_NeedAllyOrTitle".Translate());
                return needTitle;
            }

            DiaOption option = new DiaOption("RWrd_RequestJoinFaction".Translate(faction.Name));
            option.action = delegate
            {
                RWrd_FactionUtility.SetPlayerOverlord(faction);
                EnsureAlly(faction);
            };
            option.link = new DiaNode("RWrd_JoinFactionAccepted".Translate(faction.Name))
            {
                options =
        {
            new DiaOption("OK".Translate())
            {
                resolveTree = true
            }
        }
            };
            return option;
        }

        public static DiaOption TryMakeLeaveOption(Pawn negotiator, RimWorld.Faction faction)
        {
            if (negotiator == null || faction == null || faction.IsPlayer)
            {
                return null;
            }

            RimWorld.Faction player = RimWorld.Faction.OfPlayerSilentFail;
            if (player == null || RWrd_FactionUtility.GetOverlord(player) != faction)
            {
                return null;
            }

            DiaOption option = new DiaOption("RWrd_RenounceOverlord".Translate(faction.Name));
            option.link = new DiaNode("RWrd_RenounceOverlordConfirm".Translate(faction.Name))
            {
                options =
        {
            new DiaOption("Yes".Translate())
            {
                action = delegate
                {
                    RWrd_FactionUtility.ClearPlayerOverlord();
                    Messages.Message("RWrd_RenounceOverlordDone".Translate(faction.Name),
                        MessageTypeDefOf.NeutralEvent, false);
                },
                resolveTree = true
            },
            new DiaOption("No".Translate())
            {
                resolveTree = true
            }
        }
            };
            return option;
        }

        private static bool HasColonistWithTitle(RimWorld.Faction faction)
        {
            if (!ModsConfig.RoyaltyActive)
            {
                return false;
            }
            Map map = Find.AnyPlayerHomeMap;
            if (map == null)
            {
                return false;
            }
            foreach (Pawn pawn in map.mapPawns.FreeColonistsSpawned)
            {
                if (pawn.royalty != null && pawn.royalty.HasAnyTitleIn(faction))
                {
                    return true;
                }
            }
            return false;
        }

        private static void EnsureAlly(RimWorld.Faction faction)
        {
            RimWorld.Faction player = RimWorld.Faction.OfPlayer;
            FactionRelation a = player.RelationWith(faction, true);
            FactionRelation b = faction.RelationWith(player, true);
            if (a == null || b == null)
            {
                return;
            }

            // SetPlayerOverlord 后 CanChangeGoodwillFor 已被宗主锁定拦掉，
            // 不能再走 TryAffectGoodwillWith，直接写双方关系。
            a.kind = FactionRelationKind.Ally;
            b.kind = FactionRelationKind.Ally;
            a.baseGoodwill = 100;
            b.baseGoodwill = 100;
        }
    }
}
