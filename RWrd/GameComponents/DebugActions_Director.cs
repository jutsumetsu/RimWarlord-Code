using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Electromagnetic.GameComponents
{
    public static class DebugActions_Director
    {
        [DebugAction("Electromagnetic", "Director: play scene...",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void PlayScene()
        {
            GameComponent_RWrdDirector comp = GameComponent_RWrdDirector.Instance;
            if (comp == null)
            {
                Messages.Message("Director 未初始化。", MessageTypeDefOf.RejectInput, false);
                return;
            }

            List<DebugMenuOption> options = new List<DebugMenuOption>();
            foreach (DirectorSceneDef def in DefDatabase<DirectorSceneDef>.AllDefs)
            {
                DirectorSceneDef local = def;
                string label = local.defName + " [" + local.minStage + "]";
                options.Add(new DebugMenuOption(label, DebugMenuOptionMode.Action,
                    () => comp.ForcePlay(local)));
            }
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
        }

        [DebugAction("Electromagnetic", "Director: set stage...",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static List<DebugActionNode> SetStage()
        {
            List<DebugActionNode> nodes = new List<DebugActionNode>();
            foreach (DirectorStage st in Enum.GetValues(typeof(DirectorStage)))
            {
                DirectorStage local = st;
                DebugActionNode node = new DebugActionNode(local.ToString());
                node.action = delegate
                {
                    GameComponent_RWrdDirector comp = GameComponent_RWrdDirector.Instance;
                    if (comp != null) comp.stage = local;
                };
                nodes.Add(node);
            }
            return nodes;
        }

        [DebugAction("Electromagnetic", "Director: reset state",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ResetState()
        {
            GameComponent_RWrdDirector comp = GameComponent_RWrdDirector.Instance;
            if (comp == null) return;
            comp.sceneStates.Clear();
            comp.session = null;
        }

        [DebugAction("Electromagnetic", "Director: fire secondary incident",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void FireSecondary()
        {
            GameComponent_RWrdDirector comp = GameComponent_RWrdDirector.Instance;
            if (comp == null) return;
            bool ok = comp.TrySecondaryIncident(Find.AnyPlayerHomeMap);
            Messages.Message(ok ? "副叙述者已触发事件。" : "没有可用事件。",
                ok ? MessageTypeDefOf.PositiveEvent : MessageTypeDefOf.RejectInput, false);
        }

        [DebugAction("Electromagnetic", "Director: print state",
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void PrintState()
        {
            GameComponent_RWrdDirector comp = GameComponent_RWrdDirector.Instance;
            if (comp == null) { Log.Message("Director: null"); return; }
            Log.Message("Director: stage=" + comp.stage +
                " session=" + (comp.session == null ? "null" : comp.session.scene.defName) +
                " scenes=" + comp.sceneStates.Count);
        }
    }
}
