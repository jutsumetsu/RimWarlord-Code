using Electromagnetic.Core;
using Electromagnetic.Setting;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Electromagnetic.GameComponents
{
    public enum DirectorEmotion
    {
        Neutral,
        Joy,
        Smirk,
        Angry,
        Cold,
        Concerned
    }
    public enum DirectorLayout
    {
        Auto,
        Single,
        Dialogue
    }
    public class DirectorSceneDef : Def
    {
        public StorytellerDef persona;      // null = 主 persona
        public StorytellerDef defaultSpeaker;
        public DirectorStage minStage = DirectorStage.Contact;
        public float weight = 1f;
        public int cooldownDays;
        public bool once;
        public string conditionKey;         // C# 条件注册表的 key，空 = 无条件
        public DirectorLayout layout = DirectorLayout.Auto;
        public List<DirectorBeat> beats = new List<DirectorBeat>();
        public List<DirectorBeat> timeoutBeats = new List<DirectorBeat>();
    }
    public sealed class DirectorSceneState : IExposable
    {
        public int lastShownTick = -999999; // 上次播放这个场景的 tick
        public bool hasShownOnce;           // once=true 的场景是否已经播过

        public void ExposeData()
        {
            Scribe_Values.Look(ref lastShownTick, "lastShownTick", -999999);
            Scribe_Values.Look(ref hasShownOnce, "hasShownOnce", false);
        }
    }
    public class DirectorBeat
    {
        public string textKey;
        public DirectorEmotion emotion = DirectorEmotion.Neutral;
        public StorytellerDef speaker;      // 具体叙述者；null = scene.defaultSpeaker
        public string actionKey;            // C# 动作注册表的 key，可为空
        public List<DirectorChoice> choices = new List<DirectorChoice>();
    }

    public class DirectorChoice
    {
        public string labelKey;
        public string actionKey;            // 选择后果，可为空
        public List<DirectorBeat> reply = new List<DirectorBeat>();
    }
    public enum DirectorSessionState
    {
        Playing,
        Choosing,
        Closing
    }
    public sealed class DirectorSession
    {
        public void Advance(GameComponent_RWrdDirector comp, float now)
        {
            if (this.queue.Count == 0)
            {
                this.state = DirectorSessionState.Closing;
                this.current = null;
                this.closeAt = now + 1f;
                return;
            }

            this.current = this.queue[0];
            this.queue.RemoveAt(0);
            this.beatStartedAt = now;
            string fullText = this.current.textKey.TranslateSimple();
            float typingSeconds = fullText.Length / 30f;
            float baseDuration = Mathf.Clamp(3f + fullText.Length * 0.05f, 3f, 8f);
            this.beatDuration = Mathf.Max(baseDuration, typingSeconds + 1f);
            this.choicesAt = now + (RWrdSettings.DirectorChoicesWaitForText && RWrdSettings.DirectorTypewriter
                ? typingSeconds : 0f);

            this.currentSpeaker = comp.ResolveSpeaker(this.scene, this.current);
            if (this.currentSpeaker != null) this.emotions[this.currentSpeaker] = this.current.emotion;

            DirectorContext ctx = new DirectorContext
            {
                comp = comp,
                map = Find.AnyPlayerHomeMap,
                now = Find.TickManager.TicksGame
            };
            DirectorActionLibrary.Run(this.current.actionKey, ctx);

            if (this.current.choices != null && this.current.choices.Count > 0)
            {
                this.state = DirectorSessionState.Choosing;
                this.choiceDeadline = this.choicesAt + 15f; // 真实秒，从选项出现起算
            }
            else
            {
                this.state = DirectorSessionState.Playing;
            }
        }

        public void Update(GameComponent_RWrdDirector comp, float now)
        {
            if (this.state == DirectorSessionState.Closing)
            {
                if (now >= this.closeAt) comp.session = null;
                return;
            }
            if (this.state == DirectorSessionState.Choosing)
            {
                if (now >= this.choiceDeadline) this.Timeout(comp, now);
                return;
            }
            if (this.current != null && now >= this.beatStartedAt + this.beatDuration)
            {
                this.Advance(comp, now);
            }
        }

        public void Choose(int index, GameComponent_RWrdDirector comp, float now)
        {
            if (this.state != DirectorSessionState.Choosing) return;
            if (index < 0 || index >= this.current.choices.Count) return;

            DirectorChoice choice = this.current.choices[index];
            DirectorContext ctx = new DirectorContext
            {
                comp = comp,
                map = Find.AnyPlayerHomeMap,
                now = Find.TickManager.TicksGame
            };
            DirectorActionLibrary.Run(choice.actionKey, ctx);

            if (choice.reply != null && choice.reply.Count > 0)
            {
                this.queue.InsertRange(0, choice.reply);
            }
            this.state = DirectorSessionState.Playing;
            this.Advance(comp, now);
        }

        public void Timeout(GameComponent_RWrdDirector comp, float now)
        {
            if (this.scene.timeoutBeats == null || this.scene.timeoutBeats.Count == 0)
            {
                this.state = DirectorSessionState.Closing;
                this.current = null;
                this.closeAt = now + 1f;
                return;
            }

            this.queue.InsertRange(0, this.scene.timeoutBeats);
            this.state = DirectorSessionState.Playing;
            this.Advance(comp, now);
        }
        public DirectorEmotion GetEmotion(StorytellerDef persona)
        {
            DirectorEmotion emotion;
            return persona != null && emotions.TryGetValue(persona, out emotion)
                ? emotion : DirectorEmotion.Neutral;
        }

        public DirectorSceneDef scene;
        public List<DirectorBeat> queue = new List<DirectorBeat>();
        public DirectorBeat current;
        public StorytellerDef currentSpeaker;
        public float beatStartedAt;
        public float beatDuration;
        public float choicesAt;
        public float choiceDeadline;
        public float closeAt;
        public DirectorSessionState state;
        public DirectorEmotion primaryEmotion;
        public DirectorEmotion secondaryEmotion;
        public Dictionary<StorytellerDef, DirectorEmotion> emotions = new Dictionary<StorytellerDef, DirectorEmotion>();
        public bool HasTwoSpeakers;
    }
    public sealed class DirectorContext
    {
        public GameComponent_RWrdDirector comp;
        public Map map;
        public int now;
    }
    public static class DirectorConditionLibrary
    {
        private static readonly Dictionary<string, Func<DirectorContext, bool>> conditions =
            new Dictionary<string, Func<DirectorContext, bool>>();

        static DirectorConditionLibrary()
        {
            conditions["ContactFirst"] = ctx =>
                ctx.comp.stage >= DirectorStage.Contact && !ctx.comp.HasShownScene("RWrd_Director_EvilBaak_Contact");
            conditions["IdleColony"] = ctx =>
                ctx.map != null && ctx.map.mapPawns.FreeColonistsSpawnedCount <= 5;
            // 新条件在这里加一行；XML 只写 key。
        }

        public static bool Check(string key, DirectorContext ctx)
        {
            if (string.IsNullOrEmpty(key)) return true;
            Func<DirectorContext, bool> condition;
            if (!conditions.TryGetValue(key, out condition))
            {
                Log.Warning("[RimWarlord] Director condition not found: " + key);
                return false;
            }
            return condition(ctx);
        }
    }
    public static class DirectorActionLibrary
    {
        private static readonly Dictionary<string, Action<DirectorContext>> actions =
            new Dictionary<string, Action<DirectorContext>>();

        static DirectorActionLibrary()
        {
            actions["SecondaryIncident"] = ctx => ctx.comp.TrySecondaryIncident(ctx.map);
            // 新动作在这里加一行。
        }

        public static void Run(string key, DirectorContext ctx)
        {
            if (string.IsNullOrEmpty(key)) return;
            Action<DirectorContext> action;
            if (actions.TryGetValue(key, out action)) action(ctx);
            else Log.Warning("[RimWarlord] Director action not found: " + key);
        }
    }
    public static class DirectorPersonaArt
    {
        public static Texture2D Get(StorytellerDef persona, DirectorEmotion emotion)
        {
            string basePath = BasePath(persona);
            if (basePath == null) return persona.portraitLargeTex;

            Texture2D variant = ContentFinder<Texture2D>.Get(basePath + "_" + emotion, false);
            if (variant != null) return variant;
            return ContentFinder<Texture2D>.Get(basePath, false) ?? persona.portraitLargeTex;
        }

        private static string BasePath(StorytellerDef persona)
        {
            if (persona == RWrd_DefOf.RWrd_Storytellers_EvilBaakMounaam)
                return "UI/HeroArt/Storytellers/EvilBaakMounaam";
            if (persona == RWrd_DefOf.RWrd_Storytellers_SaatAmtin)
                return "UI/HeroArt/Storytellers/SaatAmtin";
            return null;
        }
    }
}
