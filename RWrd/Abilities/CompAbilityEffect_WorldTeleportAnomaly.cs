using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Sound;
using Electromagnetic.Core;
using UnityEngine;

namespace Electromagnetic.Abilities
{
    public class CompAbilityEffect_WorldTeleportAnomaly : CompAbilityEffect_Electromagnetic
    {

        private IEnumerable<Pawn> PawnsToTeleport(Pawn caster)
        {
            Caravan caravan = caster.GetCaravan();
            if (caravan != null)
            {
                foreach (Pawn p in caravan.pawns)
                    yield return p;
                yield break;
            }

            bool homeMap = caster.Map.IsPlayerHome;
            foreach (Thing t in GenRadial.RadialDistinctThingsAround(caster.Position, caster.Map, GetRadius(), true))
            {
                if (t is Pawn p && !p.Dead)
                {
                    if (!p.IsColonist && !p.IsPrisonerOfColony)
                    {
                        if (homeMap || !p.RaceProps.Animal)
                            continue;
                        if (p.Faction == null || !p.Faction.IsPlayer)
                            continue;
                    }
                    yield return p;
                }
            }
        }

        private float GetRadius()
        {
            return parent.def.verbProperties.range;
        }

        public override bool Valid(GlobalTargetInfo target, bool throwMessages = false)
        {
            int num = Find.WorldGrid.TraversalDistanceBetween(
                this.Caster.GetCaravan() != null ? this.Caster.GetCaravan().Tile : this.Caster.Map.Tile,
                target.Tile
            );
            //Log.Message($"instance: {num}");
            if (num < GetRangeForPawn(this.Caster) + 1f && num >= 0)
            {
                return true;
            }

            return false;
        }
        public override void OnGizmoUpdate()
        {
            base.OnGizmoUpdate();

            if ((Find.CurrentMap == null && Caster != null)||(WorldRendererUtility.WorldSelected))
            {
                int tile = Caster.Tile;
                int range = GetRangeForPawn(Caster);
                GenDraw.DrawWorldRadiusRing(tile, range);
            }
            else if (Caster.Spawned)
            {
                float radius = GetRadius();
                GenDraw.DrawRadiusRing(Caster.Position, radius);
            }
        }
        public override string WorldMapExtraLabel(GlobalTargetInfo target)
        {
            if ((WorldRendererUtility.WorldSelected))
            {
                int tile = Caster.Tile;
                int range = GetRangeForPawn(Caster);
                GenDraw.DrawWorldRadiusRing(tile, range);
            }
            return null;
        }
        private int GetRangeForPawn(Pawn caster)
        {
            return caster.GetPowerRoot().energy.level; //大地图格子射程
        }

        public override void Apply(GlobalTargetInfo target)
        {
            Pawn casterPawn = this.Caster;
            Map targetMap = (target.WorldObject as MapParent)?.Map;
            Caravan casterCaravan = casterPawn.GetCaravan();
            List<Pawn> pawns = PawnsToTeleport(casterPawn).ToList();
            Log.Message($"[DEBUG] Teleporting {pawns.Count} pawns: {string.Join(", ", pawns.Select(x => x.LabelShort))}");

            if (casterPawn.Spawned)
                SoundDefOf.Psycast_Skip_Pulse.PlayOneShot(new TargetInfo(target.Cell, casterPawn.Map));

            if (targetMap != null)
            {
                Pawn allied = targetMap.mapPawns.AllPawnsSpawned.FirstOrDefault(p =>
                    p.IsColonist && p.HomeFaction == Faction.OfPlayer);
                IntVec3 targetCell = allied?.Position ?? target.Cell;
                if(allied==null)
                {
                    if (!CellFinderLoose.TryFindRandomNotEdgeCellWith(12, c =>
                                c.Standable(targetMap),
                                targetMap, out IntVec3 spawn))
                    {
                        spawn = CellFinder.RandomClosewalkCellNear(targetMap.Center, targetMap, 10);
                    }
                    foreach (Pawn p in pawns)
                    {
                        if (p.Spawned)
                        {
                            p.teleporting = true;
                            p.ExitMap(false, Rot4.Invalid);
                            p.teleporting = false;
                        }
                        
                        GenSpawn.Spawn(p, spawn, targetMap);
                        if (p.drafter != null && p.IsColonistPlayerControlled)
                            p.drafter.Drafted = true;
                        p.Notify_Teleported();
                        FleckMaker.ThrowSmoke(p.DrawPos, p.Map, 1f);
                    }
                }
                else
                {
                    foreach (Pawn p in pawns)
                    {
                        if (p.Spawned)
                        {
                            p.teleporting = true;
                            p.ExitMap(false, Rot4.Invalid);
                            p.teleporting = false;
                        }

                        if (CellFinder.TryFindRandomSpawnCellForPawnNear(targetCell, targetMap, out IntVec3 spawn, 4))
                        {
                            GenSpawn.Spawn(p, spawn, targetMap);
                            if (p.drafter != null && p.IsColonistPlayerControlled)
                                p.drafter.Drafted = true;
                            p.Notify_Teleported();
                            FleckMaker.ThrowSmoke(p.DrawPos, p.Map, 1f);
                        }
                    }
                }

                

                if (casterCaravan != null)
                    casterCaravan.Destroy();

                CameraJumper.TryJump(targetCell, targetMap);
            }
            else if (target.WorldObject is Caravan caravan && caravan.Faction == casterPawn.Faction)
            {
                if (casterCaravan != null)
                {
                    casterCaravan.pawns.TryTransferAllToContainer(caravan.pawns);
                    caravan.Destroy();
                }
                else
                {
                    foreach (Pawn p in pawns)
                    {
                        caravan.AddPawn(p, true);
                        p.ExitMap(false, Rot4.Invalid);
                    }
                }
            }
            else if (casterPawn.GetCaravan() is Caravan caravan2)
            {
                caravan2.Tile = target.Tile;
                caravan2.pather.StopDead();
            }
            //没有就创建远行队
            else
            {
                CaravanMaker.MakeCaravan(pawns, casterPawn.Faction, target.Tile, false);
                foreach (Pawn p in pawns)
                    p.ExitMap(false, Rot4.Invalid);
            }
            if (!Valid(target))
            {
                Messages.Message("目标太远或无效。1", MessageTypeDefOf.RejectInput);
            }
        }

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            if ((Find.World != null && Find.CurrentMap == null && Caster != null) || (WorldRendererUtility.WorldSelected))
            {
                // 世界地图下
                int tile = Caster.Tile;
                int range = GetRangeForPawn(Caster);
                GenDraw.DrawWorldRadiusRing(tile, range);
            }
            else if (Caster.Spawned)
            {
                // 小地图
                GenDraw.DrawRadiusRing(Caster.Position, GetRadius());
            }
        }


    }
}