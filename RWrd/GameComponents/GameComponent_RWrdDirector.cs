using RimWorld;
using System;
using System.Collections.Generic;
using Electromagnetic.Core;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Electromagnetic.UI;
using UnityEngine;
using System.Linq;

namespace Electromagnetic.GameComponents
{
    public class GameComponent_RWrdDirector : Verse.GameComponent
    {
        public static GameComponent_RWrdDirector Instance { get; private set; }
        public GameComponent_RWrdDirector(Game game) : base()
        {

        }
        public static bool IsDirectorActive
        {
            get
            {
                StorytellerDef storyteller = Find.Storyteller?.def;
                return Storytellers.Contains(storyteller);
            }
        }
        public Storyteller PrimaryPersona
        {
            get
            {
                if (IsDirectorActive) return Find.Storyteller;
                return null;
            }
        }
        public Storyteller SecondaryPersona
        {
            get
            {
                if (this.PrimaryPersona == null) return null;
                
                if (this.secondaryPersona == null)
                {
                    secondaryPersona = new Storyteller(
                        PrimaryPersona.def == RWrd_DefOf.RWrd_Storytellers_EvilBaakMounaam
                        ? RWrd_DefOf.RWrd_Storytellers_SaatAmtin
                        : RWrd_DefOf.RWrd_Storytellers_EvilBaakMounaam, PrimaryPersona.difficultyDef);
                }
                else if (secondaryPersona.difficulty != PrimaryPersona.difficulty) secondaryPersona.difficulty = PrimaryPersona.difficulty;
                return secondaryPersona;
            }
        }
        public Storyteller GetScenePersona(DirectorSceneDef def)
        {
            if (def.persona == null) return PrimaryPersona;
            if (PrimaryPersona != null && def.persona == PrimaryPersona.def) return PrimaryPersona;
            return SecondaryPersona;
        }
        public StorytellerDef ResolveSpeaker(DirectorSceneDef def, DirectorBeat beat)
        {
            if (beat != null && beat.speaker != null) return beat.speaker;
            if (def != null && def.defaultSpeaker != null) return def.defaultSpeaker;
            return PrimaryPersona == null ? null : PrimaryPersona.def;
        }
        public bool HasShownScene(string defName)
        {
            DirectorSceneState state;
            return sceneStates.TryGetValue(defName, out state) && state.hasShownOnce;
        }
        public DirectorSceneState GetOrCreateState(DirectorSceneDef def)
        {
            DirectorSceneState state;
            if (sceneStates.TryGetValue(def.defName, out state)) return state;

            state = new DirectorSceneState();
            sceneStates[def.defName] = state;
            return state;
        }

        public bool FireOn(Storyteller persona, IncidentDef def, IIncidentTarget target)
        {
            if (persona == null || def == null || target == null) return false;
            IncidentParms parms = StorytellerUtility.DefaultParmsNow(def.category, target);
            return persona.TryFire(new FiringIncident(def, null, parms));
        }
        public bool TrySecondaryIncident(IIncidentTarget target)
        {
            Storyteller persona = this.SecondaryPersona;
            if (persona == null) return false;

            List<FiringIncident> candidates = persona.MakeIncidentsForInterval()
                .Where(FI => FI != null && FI.def != null && FI.sourceQuestPart == null)
                .Take(32)
                .ToList();
            if (candidates.Count == 0) return false;

            FiringIncident fi = candidates.RandomElement();
            if (fi.parms == null) fi.parms = new IncidentParms();
            if (fi.parms.target == null) fi.parms.target = target ?? Find.AnyPlayerHomeMap;
            if (fi.parms.target == null) return false;

            return persona.TryFire(fi);
        }
        private bool CanStart(DirectorSceneDef def)
        {
            if (def == null || stage < def.minStage) return false;

            DirectorSceneState state;
            if (!sceneStates.TryGetValue(def.defName, out state))
            {
                state = new DirectorSceneState();
                sceneStates[def.defName] = state;
            }

            if (def.once && state.hasShownOnce) return false;
            if (def.cooldownDays > 0)
            {
                int cooldownTicks = def.cooldownDays * 60000;
                if (Find.TickManager.TicksGame - state.lastShownTick < cooldownTicks) return false;
            }

            DirectorContext ctx = new DirectorContext
            {
                comp = this,
                map = Find.AnyPlayerHomeMap,
                now = Find.TickManager.TicksGame
            };
            return DirectorConditionLibrary.Check(def.conditionKey, ctx);
        }
        private void TryStartScene()
        {
            List<DirectorSceneDef> candidates = new List<DirectorSceneDef>();
            foreach (DirectorSceneDef def in DefDatabase<DirectorSceneDef>.AllDefs)
            {
                if (CanStart(def)) candidates.Add(def);
            }
            if (candidates.Count == 0) return;

            DirectorSceneDef picked = candidates.RandomElementByWeight(d => Mathf.Max(0.01f, d.weight));
            ForcePlay(picked);
        }
        public Storyteller GetPersona(StorytellerDef def)
        {
            if (def == null) return null;
            if (PrimaryPersona != null && PrimaryPersona.def == def) return PrimaryPersona;
            if (SecondaryPersona != null && SecondaryPersona.def == def) return SecondaryPersona;
            return null;
        }
        private int CountSpeakers(DirectorSceneDef def)
        {
            HashSet<StorytellerDef> speakers = new HashSet<StorytellerDef>();
            foreach (DirectorBeat beat in def.beats) speakers.Add(ResolveSpeaker(def, beat));
            foreach (DirectorBeat beat in def.timeoutBeats) speakers.Add(ResolveSpeaker(def, beat));
            speakers.Remove(null);
            return speakers.Count;
        }
        public bool ForcePlay(DirectorSceneDef def)
        {
            if (def == null || def.beats == null || def.beats.Count == 0) return false;

            this.session = new DirectorSession
            {
                scene = def,
                queue = new List<DirectorBeat>(def.beats),
                HasTwoSpeakers = def.layout == DirectorLayout.Dialogue ||
                    (def.layout != DirectorLayout.Single && CountSpeakers(def) > 1)
            };

            DirectorSceneState state = GetOrCreateState(def);
            state.lastShownTick = Find.TickManager.TicksGame;
            if (def.once) state.hasShownOnce = true;

            this.session.Advance(this, Time.realtimeSinceStartup);
            return true;
        }

        public override void FinalizeInit()
        {
            GameComponent_RWrdDirector.Instance = this;
        }
        public override void GameComponentTick()
        {
            if (!IsDirectorActive) return;
            if (Find.TickManager.TicksGame % 2500 == 0 && this.session == null) TryStartScene();
        }

        // UI 层每帧钩子：暂停/倍速都不影响；session 计时全部用真实秒
        public override void GameComponentUpdate()
        {
            if (!IsDirectorActive) return;
            if (this.session != null) this.session.Update(this, Time.realtimeSinceStartup);
        }

        public override void GameComponentOnGUI()
        {
            base.GameComponentOnGUI();
            if (!IsDirectorActive) return;
            DirectorOverlay.Draw(this);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look<Storyteller>(ref this.secondaryPersona, "secondaryPersona", null);
            Scribe_Values.Look<DirectorStage>(ref this.stage, "stage", DirectorStage.Background);
        }

        private Storyteller secondaryPersona;
        private static readonly List<StorytellerDef> Storytellers = new List<StorytellerDef>()
        {
            RWrd_DefOf.RWrd_Storytellers_EvilBaakMounaam,
            RWrd_DefOf.RWrd_Storytellers_SaatAmtin
        };

        public DirectorStage stage = DirectorStage.Background;
        public DirectorSession session = null;
        public Dictionary<string, DirectorSceneState> sceneStates = new Dictionary<string, DirectorSceneState>();
    }
    public enum DirectorStage
    {
        Background,
        Signs,
        Contact,
        Active
    }
}
