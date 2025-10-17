using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Electromagnetic.Core;
using HarmonyLib;
using Verse;

namespace Electromagnetic.HarmonyPatchs
{
    internal class HarmonyInit
    {
        [StaticConstructorOnStartup]
        public class PatchMain
        {
            static PatchMain()
            {
                HarmonyInit.PatchMain.instance.PatchAll(Assembly.GetExecutingAssembly());
                // 延迟应用RJW相关补丁
                if (ModDetector.RJWIsLoaded)
                {
                    LongEventHandler.QueueLongEvent(SafePatchRJW, "RWrd_RJWPatches", false, null);
                }
                // 订阅游戏初始化事件
                LongEventHandler.QueueLongEvent(() =>
                {
                    // 确保事件只触发一次
                    if (RWrd_DefOf.RWrd_Pawn_BaakSide != null)
                    {
                        ApplyRelationPatch();
                    }
                    else
                    {
                        // 若未就绪，延迟到下一帧
                        LongEventHandler.ExecuteWhenFinished(ApplyRelationPatch);
                    }
                }, "DelayedHarmonyInit", false, null);
            }
            private static void ApplyRelationPatch()
            {
                var targetMethods = Pawn_Patch.RelationPatch.TargetMethods().ToList();
                Log.Message($"[RimWarlord] Found {targetMethods.Count} methods to patch");

                foreach (var method in targetMethods)
                {
                    try
                    {
                        instance.Patch(method,
                             prefix: new HarmonyMethod(typeof(Pawn_Patch.RelationPatch), nameof(Pawn_Patch.RelationPatch.CreateRelation_Prefix)));
                        Log.Message($"[RimWarlord] Patched: {method.DeclaringType?.Name}.{method.Name}");
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"[RimWarlord] Failed to patch {method.Name}: {ex}");
                    }
                }
            }
            private static bool WaitForRJWInitialization()
            {
                try
                {
                    int maxAttempts = 50; // 5秒超时 (50 * 100ms)
                    for (int i = 0; i < maxAttempts; i++)
                    {
                        if (IsRJWReady())
                        {
                            return true;
                        }
                        Thread.Sleep(100); // 等待100ms
                    }
                    return false;
                }
                catch
                {
                    return false;
                }
            }
            private static void SafePatchRJW()
            {
                try
                {
                    // 等待RJW完全初始化
                    if (!WaitForRJWInitialization())
                    {
                        Log.Warning("[RWrd] RJW模组未在超时时间内初始化，跳过RJW补丁");
                        return;
                    }

                    // 手动应用RJW补丁
                    ApplyRJWPatches();
                    Log.Message("[RWrd] RJW补丁应用完成");
                }
                catch (Exception ex)
                {
                    Log.Error($"[RWrd] RJW补丁应用失败: {ex}");
                }
            }

            private static bool IsRJWReady()
            {
                try
                {
                    // 检查RJW核心类型是否已加载且可访问
                    var type = Type.GetType("rjw.Hediff_BasePregnancy, RJW");
                    if (type == null) return false;

                    // 尝试访问静态方法而不触发完整初始化
                    var method = type.GetMethod("BabyPostBirth", BindingFlags.Public | BindingFlags.Static);
                    return method != null;
                }
                catch (TypeInitializationException)
                {
                    // 类型初始化异常，需要继续等待
                    return false;
                }
                catch
                {
                    // 其他异常，认为RJW未就绪
                    return false;
                }
            }
            private static void ApplyRJWPatches()
            {
                try
                {
                    // 手动应用RJW补丁
                    var originalMethod = AccessTools.Method("rjw.Hediff_BasePregnancy:PostBirth");
                    var postfixMethod = typeof(RJWPatches.RJWPregnancyPatch).GetMethod("Postfix");

                    if (originalMethod != null && postfixMethod != null)
                    {
                        instance.Patch(originalMethod, postfix: new HarmonyMethod(postfixMethod));
                        Log.Message("[RWrd] RJW PostBirth补丁应用成功");
                    }
                    else
                    {
                        Log.Warning("[RWrd] 无法找到RJW补丁方法");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"[RWrd] RJW补丁应用异常: {ex}");
                }
            }
            public static Harmony instance = new Harmony("RWrd.Harmony");
        }
    }
}
