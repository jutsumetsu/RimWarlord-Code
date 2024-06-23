using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace Electromagnetic.Core
{
    public static class AIUtility
    {
        public static IntVec3 FindRandomSpotOutsideColony(Pawn pawn, float distance = -1f, bool canReach = false, bool canReserve = false)
        {
            IntVec3 invalid = IntVec3.Invalid;
            RCellFinder.TryFindRandomSpotJustOutsideColony(pawn.Position, pawn.Map, pawn, out invalid, (IntVec3 x) => x.InBounds(pawn.Map) && x.Walkable(pawn.Map) && !x.IsForbidden(pawn) && (distance <= 0f || x.DistanceTo(pawn.Position) <= distance * distance) && (!canReach || ReachabilityUtility.CanReach(pawn, x, PathEndMode.OnCell, Danger.None, false, false, TraverseMode.ByPawn)) && (!canReserve || pawn.CanReserve(x, 1, -1, null, false)));
            return invalid;
        }
        public static IEnumerable<IntVec3> FindAroundSpotFromTarget(Pawn pawn, IntVec3 target, float maxRadius, float minRadius, bool canSee = true, bool canReach = false, bool canReserve = false)
        {
            return from x in GenRadial.RadialCellsAround(target, maxRadius, true)
                   where x.InBounds(pawn.Map) && x.Walkable(pawn.Map) && !x.IsForbidden(pawn) && (float)x.DistanceToSquared(target) > minRadius * minRadius && (!canReach || ReachabilityUtility.CanReach(pawn, x, PathEndMode.OnCell, Danger.None, false, false, TraverseMode.ByPawn)) && (!canReserve || pawn.CanReserve(x, 1, -1, null, false)) && !(canSee & !GenSight.LineOfSight(target, x, pawn.Map, false, null, 0, 0))
                   select x;
        }
        public static IntVec3 FindRandomSpotInZone(Pawn pawn, Zone zone, bool canReach = false, bool canReserve = false)
        {
            IntVec3 invalid = IntVec3.Invalid;
            if ((from x in zone.cells
                 where x.InBounds(pawn.Map) && x.Walkable(pawn.Map) && !x.IsForbidden(pawn) && (!canReach || ReachabilityUtility.CanReach(pawn, x, PathEndMode.OnCell, Danger.None, false, false, TraverseMode.ByPawn)) && (!canReserve || pawn.CanReserve(x, 1, -1, null, false))
                 select x).TryRandomElement(out invalid))
            {
                return invalid;
            }
            return IntVec3.Invalid;
        }
    }
}
