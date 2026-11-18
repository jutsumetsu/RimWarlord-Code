using Electromagnetic.GameComponents;
using Electromagnetic.ModExtension;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Electromagnetic.Factions
{
    public static class RWrd_FactionUtility
    {
        /// <summary>
        /// 获取宗主
        /// </summary>
        /// <param name="vassal"></param>
        /// <returns></returns>
        public static Faction GetOverlord(Faction vassal)
        {
            if (vassal == null)
            {
                return null;
            }

            if (vassal.IsPlayer && Current.Game != null)
            {
                GameComponent_RWrdVassal comp = Current.Game.GetComponent<GameComponent_RWrdVassal>();
                if (comp != null)
                {
                    if (comp.overlord != null && !comp.overlord.defeated)
                    {
                        return comp.overlord;
                    }
                    if (comp.abandonedStaticOverlord)
                    {
                        return null;
                    }
                }
            }

            RelationCustom ext = vassal.def.GetModExtension<RelationCustom>();
            if (ext != null && ext.overlord != null)
            {
                Faction staticOverlord = Find.FactionManager.FirstFactionOfDef(ext.overlord);
                if (staticOverlord != null && !staticOverlord.defeated)
                {
                    return staticOverlord;
                }
            }
            return null;
        }
        /// <summary>
        /// 是宗主吗
        /// </summary>
        /// <param name="overlord"></param>
        /// <param name="vassal"></param>
        /// <returns></returns>
        public static bool IsOverlordOf(Faction overlord, Faction vassal)
        {
            return overlord != null && GetOverlord(vassal) == overlord;
        }
        /// <summary>
        /// 设置玩家宗主
        /// </summary>
        /// <param name="faction"></param>
        public static void SetPlayerOverlord(Faction faction)
        {
            GameComponent_RWrdVassal comp = Current.Game.GetComponent<GameComponent_RWrdVassal>();
            if (comp == null)
            {
                return;
            }
            comp.overlord = faction;
            comp.abandonedStaticOverlord = false;
        }
        /// <summary>
        /// 清除玩家宗主
        /// </summary>
        public static void ClearPlayerOverlord()
        {
            GameComponent_RWrdVassal comp = Current.Game.GetComponent<GameComponent_RWrdVassal>();
            if (comp == null)
            {
                return;
            }

            Faction oldOverlord = comp.overlord ?? GetOverlord(RimWorld.Faction.OfPlayer);
            comp.overlord = null;

            RelationCustom ext = RimWorld.Faction.OfPlayer.def.GetModExtension<RelationCustom>();
            if (ext != null && ext.overlord != null)
            {
                comp.abandonedStaticOverlord = true;
            }

            if (oldOverlord != null)
            {
                SetNeutralZero(RimWorld.Faction.OfPlayer, oldOverlord);
            }
        }
        /// <summary>
        /// 好感度归零
        /// </summary>
        /// <param name="player"></param>
        /// <param name="overlord"></param>
        private static void SetNeutralZero(Faction player, Faction overlord)
        {
            FactionRelation a = player.RelationWith(overlord, true);
            FactionRelation b = overlord.RelationWith(player, true);
            if (a != null)
            {
                a.kind = FactionRelationKind.Neutral;
            }
            if (b != null)
            {
                b.kind = FactionRelationKind.Neutral;
            }

            if (a != null)
            {
                a.baseGoodwill = 0;
            }
            if (b != null)
            {
                b.baseGoodwill = 0;
            }
        }
    }
}
