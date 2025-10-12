using LudeonTK;
using RimWorld;
using Verse;

namespace Electromagnetic
{
    public static class DebugActions_ExpPool
    {
        [DebugAction("Electromagnetic", "Add 500 EXP", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void Add500Exp()
        {
            var comp = Find.CurrentMap?.GetComponent<MapComponent_ExpPool>();
            if (comp != null)
            {
                comp.AddExp(500f);
                Messages.Message($"已添加 500 点经验，现在共有 {comp.GetCurrentExp():F1} / {comp.maxExp:F1}。", MessageTypeDefOf.PositiveEvent, false);
            }
            else
            {
                Messages.Message($"经验池未开启。", MessageTypeDefOf.RejectInput, false);
            }
        }

        [DebugAction("Electromagnetic", "Consume 500 EXP", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void Consume500Exp()
        {
            var comp = Find.CurrentMap?.GetComponent<MapComponent_ExpPool>();
            if (comp != null)
            {
                if (comp.ConsumeExp(500f))
                    Messages.Message($"消耗了 500 点经验，剩余 {comp.GetCurrentExp():F1} / {comp.maxExp:F1}。", MessageTypeDefOf.PositiveEvent, false);
                else
                    Messages.Message($"经验不足，当前仅有 {comp.GetCurrentExp():F1} 点。", MessageTypeDefOf.RejectInput, false);
            }
            else
            {
                Messages.Message($"经验池未开启。", MessageTypeDefOf.RejectInput, false);
            }
        }

        [DebugAction("Electromagnetic", "Clear EXP", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ClearExp()
        {
            var comp = Find.CurrentMap?.GetComponent<MapComponent_ExpPool>();
            if (comp != null)
            {
                comp.SetExp(0f);
                Messages.Message($"经验池已清空。", MessageTypeDefOf.PositiveEvent, false);
            }
            else
            {
                Messages.Message($"经验池未开启。", MessageTypeDefOf.RejectInput, false);
            }
        }

        [DebugAction("Electromagnetic", "Set Max EXP (to 20000)", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void SetMaxExp()
        {
            var comp = Find.CurrentMap?.GetComponent<MapComponent_ExpPool>();
            if (comp != null)
            {
                comp.SetMaxExp(20000f);
                Messages.Message($"经验池最大容量已设为 20000。", MessageTypeDefOf.PositiveEvent, false);
            }
            else
            {
                Messages.Message($"经验池未开启。", MessageTypeDefOf.RejectInput, false);
            }
        }

        
    }
}
