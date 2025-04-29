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
            // 创建一个无效的 IntVec3 对象，用于存储找到的位置
            IntVec3 invalid = IntVec3.Invalid;

            // 尝试找到一个位于殖民地外的随机位置
            RCellFinder.TryFindRandomSpotJustOutsideColony(
                pawn.Position,        // 起始位置（当前 pawn 的位置）
                pawn.Map,             // 当前地图
                pawn,                 // 当前 pawn 作为寻找位置的参考
                out invalid,          // 输出参数，找到的位置存储在这里
                (IntVec3 x) =>        // 匿名函数，用于对每个候选位置进行过滤
                {
                    // 过滤条件：位置在地图范围内
                    // 过滤条件：位置可行走
                    // 过滤条件：位置不被禁止
                    // 过滤条件：距离限制（如果 distance > 0，要求位置到当前 pawn 的距离不超过 distance）
                    // 过滤条件：是否可以到达（如果 canReach 为 true，要求 pawn 可以到达这个位置）
                    // 过滤条件：是否可以预定（如果 canReserve 为 true，要求 pawn 可以预定这个位置）
                    return x.InBounds(pawn.Map) &&
                           x.Walkable(pawn.Map) &&
                           !x.IsForbidden(pawn) &&
                           (distance <= 0f || x.DistanceTo(pawn.Position) <= distance * distance) &&
                           (!canReach || ReachabilityUtility.CanReach(pawn, x, PathEndMode.OnCell, Danger.None, false, false, TraverseMode.ByPawn)) &&
                           (!canReserve || pawn.CanReserve(x, 1, -1, null, false));
                }
            );

            // 返回找到的位置（可能是无效的）
            return invalid;
        }

        public static IEnumerable<IntVec3> FindAroundSpotFromTarget(Pawn pawn, IntVec3 target, float maxRadius, float minRadius, bool canSee = true, bool canReach = false, bool canReserve = false)
        {
            // 从目标位置周围的所有单元格中筛选符合条件的位置
            return from x in GenRadial.RadialCellsAround(target, maxRadius, true)
                   where x.InBounds(pawn.Map) &&    // 位置在地图范围内
                         x.Walkable(pawn.Map) &&    // 位置可行走
                         !x.IsForbidden(pawn) &&    // 位置不被禁止
                         (float)x.DistanceToSquared(target) > minRadius * minRadius &&    // 距离限制（位置到目标的距离大于 minRadius）
                         (!canReach || ReachabilityUtility.CanReach(pawn, x, PathEndMode.OnCell, Danger.None, false, false, TraverseMode.ByPawn)) &&    // 是否可以到达
                         (!canReserve || pawn.CanReserve(x, 1, -1, null, false)) &&    // 是否可以预定
                         !(canSee & !GenSight.LineOfSight(target, x, pawn.Map, false, null, 0, 0))    // 是否可以视线通过（如果 canSee 为 true）
                   select x;    // 选择符合条件的位置
        }

        public static IntVec3 FindRandomSpotInZone(Pawn pawn, Zone zone, bool canReach = false, bool canReserve = false)
        {
            // 创建一个无效的 IntVec3 对象，用于存储找到的位置
            IntVec3 invalid = IntVec3.Invalid;

            // 从 zone 的所有单元格中筛选符合条件的位置，并尝试随机选择一个
            if ((from x in zone.cells
                 where x.InBounds(pawn.Map) &&    // 位置在地图范围内
                       x.Walkable(pawn.Map) &&    // 位置可行走
                       !x.IsForbidden(pawn) &&    // 位置不被禁止
                       (!canReach || ReachabilityUtility.CanReach(pawn, x, PathEndMode.OnCell, Danger.None, false, false, TraverseMode.ByPawn)) &&    // 是否可以到达
                       (!canReserve || pawn.CanReserve(x, 1, -1, null, false))    // 是否可以预定
                 select x).TryRandomElement(out invalid))    // 尝试随机选择一个位置
            {
                // 如果成功选择了位置，则返回该位置
                return invalid;
            }

            // 如果没有找到符合条件的位置，则返回无效位置
            return IntVec3.Invalid;
        }
    }
}
