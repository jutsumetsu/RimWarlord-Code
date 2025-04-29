using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse.AI;
using Verse;
using Electromagnetic.Abilities;

namespace Electromagnetic.HarmonyPatchs
{

    public static class AntiGravityPatches
    {

        public static void Do(Harmony harm)
        {
            harm.Patch(AccessTools.Method(typeof(Pawn_PathFollower), "GenerateNewPath", null, null), new HarmonyMethod(typeof(AntiGravityPatches), "GenerateNewPath_Prefix", null), null, null, null);
            harm.Patch(AccessTools.Method(typeof(Pawn_PathFollower), "CostToMoveIntoCell", new Type[]
            {
                typeof(Pawn),
                typeof(IntVec3)
            }, null), null, null, new HarmonyMethod(typeof(AntiGravityPatches), "CostToMoveIntoCell_Transpile", null), null);
            harm.Patch(AccessTools.Method(typeof(GenGrid), "WalkableBy", null, null), new HarmonyMethod(typeof(AntiGravityPatches), "WalkableBy_Prefix", null), null, null, null);
            harm.Patch(AccessTools.Method(typeof(Pawn_PathFollower), "BuildingBlockingNextPathCell", null, null), new HarmonyMethod(typeof(AntiGravityPatches), "NoBuildingBlocking", null), null, null, null);
            harm.Patch(AccessTools.Method(typeof(Pawn_PathFollower), "TryEnterNextPathCell", null, null), null, new HarmonyMethod(typeof(AntiGravityPatches), "UnfogEnteredCells", null), null, null);
            harm.Patch(AccessTools.Method(typeof(Reachability), "CanReach", new Type[]
            {
                typeof(IntVec3),
                typeof(LocalTargetInfo),
                typeof(PathEndMode),
                typeof(TraverseParms)
            }, null), new HarmonyMethod(typeof(AntiGravityPatches), "AllReachable", null), null, null, null);
            harm.Patch(AccessTools.Method(typeof(Pawn_PathFollower), "StartPath", null, null), new HarmonyMethod(typeof(AntiGravityPatches), "StartPath_Prefix", null), new HarmonyMethod(typeof(AntiGravityPatches), "StartPath_Postfix", null), null, null);
            harm.Patch(AccessTools.Method(typeof(Pawn), "SpawnSetup", null, null), null, new HarmonyMethod(typeof(AntiGravityPatches), "CheckPhasing", null), null, null);
            harm.Patch(AccessTools.Method(typeof(Pawn), "DeSpawn", null, null), null, new HarmonyMethod(typeof(AntiGravityPatches), "Despawn_Postfix", null), null, null);
        }

        public static void UnfogEnteredCells(Pawn_PathFollower __instance, Pawn ___pawn)
        {
            bool flag = ___pawn.Spawned && __instance.nextCell.Fogged(___pawn.Map) && ___pawn.IsAntiGravity();
            if (flag)
            {
                AntiGravityPatches.FloodUnfogAdjMI.Invoke(___pawn.Map.fogGrid, new object[]
                {
                    __instance.nextCell,
                    true
                });
            }
        }

        public static bool AllReachable(TraverseParms traverseParams, ref bool __result)
        {
            bool flag = (traverseParams.pawn != null && traverseParams.pawn.IsAntiGravity()) || (AntiGravityPatches.patherPawn != null && AntiGravityPatches.patherPawn.IsAntiGravity());
            bool result;
            if (flag)
            {
                __result = true;
                result = false;
            }
            else
            {
                result = true;
            }
            return result;
        }

        public static void StartPath_Prefix(Pawn ___pawn)
        {
            AntiGravityPatches.patherPawn = ___pawn;
        }

        public static void StartPath_Postfix()
        {
            AntiGravityPatches.patherPawn = null;
        }

        public static bool NoBuildingBlocking(ref Building __result, Pawn ___pawn)
        {
            bool flag = ___pawn.IsAntiGravity();
            bool result;
            if (flag)
            {
                __result = null;
                result = false;
            }
            else
            {
                result = true;
            }
            return result;
        }

        public static bool WalkableBy_Prefix(ref bool __result, Pawn pawn, IntVec3 c)
        {
            bool flag = pawn.IsAntiGravity();
            bool result;
            if (flag)
            {
                __result = true;
                result = false;
            }
            else
            {
                result = true;
            }
            return result;
        }

        public static IEnumerable<CodeInstruction> CostToMoveIntoCell_Transpile(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
            MethodInfo info1 = AccessTools.PropertyGetter(typeof(Thing), "Map");
            int index = list.FindIndex((CodeInstruction ins) => ins.Calls(info1)) - 2;
            MethodInfo info2 = AccessTools.PropertyGetter(typeof(Pawn), "CurJob");
            int index2 = list.FindIndex((CodeInstruction ins) => ins.Calls(info2)) - 1;
            Label label = generator.DefineLabel();
            List<Label> labels = list[index].ExtractLabels();
            list[index2].labels.Add(label);
            list.InsertRange(index, new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldarg_0, null).WithLabels(labels),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AntiGravityUtils), "IsAntiGravity", null, null)),
                new CodeInstruction(OpCodes.Brtrue, label)
            });
            return list;
        }

        public static bool GenerateNewPath_Prefix(Pawn_PathFollower __instance, Pawn ___pawn, LocalTargetInfo ___destination, PathEndMode ___peMode, ref PawnPath __result)
        {
            bool flag = ___pawn.IsAntiGravity();
            bool result;
            if (flag)
            {
                __instance.lastPathedTargetPosition = ___destination.Cell;
                __result = ___pawn.Map.pathFinder.FindPath(___pawn.Position, ___destination, new TraverseParms
                {
                    pawn = ___pawn,
                    alwaysUseAvoidGrid = false,
                    canBashDoors = true,
                    canBashFences = true,
                    fenceBlocked = false,
                    maxDanger = Danger.Deadly,
                    mode = TraverseMode.PassAllDestroyableThings
                }, ___peMode, new PathFinderCostTuning
                {
                    costBlockedDoor = 0,
                    costBlockedDoorPerHitPoint = 0f,
                    costBlockedWallBase = 0,
                    costBlockedWallExtraForNaturalWalls = 0,
                    costBlockedWallExtraPerHitPoint = 0f,
                    costOffLordWalkGrid = 0
                });
                result = false;
            }
            else
            {
                result = true;
            }
            return result;
        }

        public static void CheckPhasing(Pawn __instance)
        {
            bool flag = __instance.IsAntiGravitySlow();
            if (flag)
            {
                AntiGravityUtils.AntiGravityPawns.Add(__instance);
            }
        }

        public static void Despawn_Postfix(Pawn __instance)
        {
            bool flag = AntiGravityUtils.AntiGravityPawns.Contains(__instance);
            if (flag)
            {
                AntiGravityUtils.AntiGravityPawns.Remove(__instance);
            }
        }

        private static readonly MethodInfo FloodUnfogAdjMI = AccessTools.Method(typeof(FogGrid), "FloodUnfogAdjacent", new Type[] { typeof(IntVec3), typeof(bool) });

        private static Pawn patherPawn;
    }
}
