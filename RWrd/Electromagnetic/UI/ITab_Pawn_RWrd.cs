using System;
using System.Collections.Generic;
using System.Linq;
using Electromagnetic.Core;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using static HarmonyLib.Code;

namespace Electromagnetic.UI
{
    [StaticConstructorOnStartup]
    public class ITab_Pawn_RWrd : ITab
    {
        static ITab_Pawn_RWrd()
        {
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs)
            {
                RaceProperties race = thingDef.race;
                bool flag = race != null && race.Humanlike;
                if (flag)
                {
                    List<Type> inspectorTabs = thingDef.inspectorTabs;
                    if (inspectorTabs != null)
                    {
                        inspectorTabs.Add(typeof(ITab_Pawn_RWrd));
                    }
                    List<InspectTabBase> inspectorTabsResolved = thingDef.inspectorTabsResolved;
                    if (inspectorTabsResolved != null)
                    {
                        inspectorTabsResolved.Add(InspectTabManager.GetSharedInstance(typeof(ITab_Pawn_RWrd)));
                    }
                }
            }
        }

        public ITab_Pawn_RWrd()
        {
            this.labelKey = "RWrd.Powerfulperson";
            this.size = new Vector2((float)Verse.UI.screenWidth, (float)Verse.UI.screenHeight * 0.75f);
            /*this.pathsByTab = (from def in DefDatabase<RWrd_RouteDef>.AllDefs
                               group def by def.defName).ToDictionary((IGrouping<string, RWrd_RouteDef> group)
                               => group.Key, (IGrouping<string, RWrd_RouteDef> group)
                               => group.ToList<RWrd_RouteDef>());
            this.tabs = (from kv in this.pathsByTab
                         select new TabRecord(kv.Key, delegate ()
                         {
                             this.curTab = kv.Key;
                         }, () => this.curTab == kv.Key)).ToList<TabRecord>();
            this.curTab = this.pathsByTab.Keys.FirstOrDefault<string>();*/
            this.tabs = new List<TabRecord>();
            this.curTab = "DefaultTab"; 
        }
        protected override void UpdateSize()
        {
            base.UpdateSize();
            this.size.y = this.PaneTopY - 30f;
            this.pathsPerRow = Mathf.FloorToInt(this.size.x * 0.67f / 200f);            
        }
        public Vector2 Size
        {
            get
            {
                return this.size;
            }
        }

        public float RequestedPsysetsHeight { get; private set; }

        public override bool IsVisible
        {
            get
            {
                Pawn pawn = Find.Selector.SingleSelectedThing as Pawn;
                bool result;
                if (pawn != null && pawn.health.hediffSet.HasHediff(RWrd_DefOf.Hediff_RWrd_PowerRoot, false))
                {
                    Faction faction = pawn.Faction;
                    result = (faction != null && faction.IsPlayer);
                }
                else
                {
                    result = false;
                }
                return result;
            }
        }

        public override void OnOpen()
        {
            base.OnOpen();
            this.pawn = (Pawn)Find.Selector.SingleSelectedThing;
            this.InitCache();
        }

        private void InitCache()
        {
            this.hediff = pawn.GetRoot();
            this.abilityPos.Clear();
        }

        protected override void CloseTab()
        {
            base.CloseTab();
            this.pawn = null;
            this.hediff = null;
            this.abilityPos.Clear();
        }

        protected override void FillTab()
        {
            Pawn pawn = Find.Selector.SingleSelectedThing as Pawn;
            bool flag = pawn != null && this.pawn != pawn;
            if (flag)
            {
                this.pawn = pawn;
                this.InitCache();
            }

            bool flag3 = this.pawn == null || this.hediff == null;
            if (!flag3)
            {
                GameFont font = Text.Font;
                TextAnchor anchor = Text.Anchor;

                Rect rect = new Rect(Vector2.one * 20f, this.size - Vector2.one * 40f);
                Widgets.DrawBoxSolid(rect, Color.gray);

                Text.Font = font;
                Text.Anchor = anchor;
                Rect rectF = new Rect(Vector2.one * 20f, this.size - Vector2.one * 40f);
                Rect rectS = rect.TakeLeftPart(this.size.x * 0.3f);
                Rect rectT = rect.ContractedBy(5f);
                TabDrawer.DrawTabs<TabRecord>(new Rect(rectT.x, rectT.y + 40f, rectT.width, rectT.height), this.tabs, 200f);
                rectT.yMin += 40f;
                Widgets.DrawMenuSection(rectT);
                Rect rect8 = new Rect(0f, 0f, rectT.width - 20f, this.lastPathsHeight);
                //Widgets.BeginScrollView(rectT.ContractedBy(2f), ref this.pathsScrollPos, rect8, true);
                this.DoPaths(rect8);
                //Widgets.EndScrollView();
            }
        }

        private void DoPaths(Rect inRect)
        {
            Vector2 vector = inRect.position + Vector2.one * 10f;
            float num = (inRect.width - (float)(this.pathsPerRow + 1) * 10f) / (float)this.pathsPerRow;
            float num2 = 0f;
            int num3 = this.pathsPerRow;
            IEnumerable<RWrd_RouteDef> paths = DefDatabase<RWrd_RouteDef>.AllDefs
                                                .OrderBy((RWrd_RouteDef path) => path.label.Substring(0, 1));

            foreach (RWrd_RouteDef def in paths)
            {
                Texture2D texture2D = def.backgroundImage;
                float num4 = num / (float)texture2D.width * (float)texture2D.height + 30f;
                Rect rect = new Rect(vector, new Vector2(num, num4));
                DrawPathBackground(ref rect, def);

                DoPathAbilities(rect, def, this.abilityPos, new Action<Rect, AbilityDef>(this.DoAbility));

                num2 = Mathf.Max(num2, num4 + 10f);
                vector.x += num + 10f;
                num3--;
                if (num3 == 0)
                {
                    vector.x = inRect.x + 10f;
                    vector.y += num2;
                    num3 = this.pathsPerRow;
                    num2 = 0f;
                }
            }
            this.lastPathsHeight = vector.y + num2;
        }

        private void DoAbility(Rect inRect, AbilityDef ability)
        {
            
            DrawAbility(inRect, ability);
            /*bool flag4 = flag;
            if (flag4)
            {
                Widgets.DrawRectFast(inRect, new Color(0f, 0f, 0f, 0.6f), null);
            }*/
            
        }
        public static void DoPathAbilities(Rect inRect, RWrd_RouteDef path, Dictionary<AbilityDef, Vector2> abilityPos, Action<Rect, AbilityDef> doAbility)
        {
            // 获取每个节点的技能和对应层级
            var routeNodes = path.routeNodes;

            foreach (var node in routeNodes)
            {
                int level = node.level;
                List<AbilityDef> abilities = node.abilities;

                // 为每个等级创建一个矩形区域
                Rect rect = new Rect(inRect.x, inRect.y + (float)(path.MaxLevel - 1 - level) * inRect.height / (float)path.MaxLevel + 10f, inRect.width, inRect.height / 5f);

                // 检查 abilityTreeXOffsets是否足够长
                if (abilities.Count - 1 < abilityTreeXOffsets.Length)
                {
                    // 遍历该等级中的所有能力
                    for (int j = 0; j < abilities.Count; j++)
                    {
                        // 检查 abilityTreeXOffsets是否足够长
                        if (j < abilityTreeXOffsets[abilities.Count - 1].Length)
                        {
                            Rect arg = new Rect(rect.x + rect.width / 2f + abilityTreeXOffsets[abilities.Count - 1][j], rect.y, 36f, 36f);
                            AbilityDef abilityDef = abilities[j];
                            abilityPos[abilityDef] = arg.center;
                            doAbility(arg, abilityDef);
                        }
                        else
                        {
                            Log.Warning($"abilityTreeXOffsets 内部数组长度不足，abilities.Count: {abilities.Count}, j: {j}");
                        }
                    }
                }
                else
                {
                    Log.Warning($"abilityTreeXOffsets 长度不足，abilities.Count: {abilities.Count}");
                }
            }
        }



        public static void DrawPathBackground(ref Rect rect, RWrd_RouteDef def)
        {
            Texture2D image = def.backgroundImage;
            GUI.color = new ColorInt(97, 108, 122).ToColor;
            Widgets.DrawBox(rect.ExpandedBy(2f), 1, Texture2D.whiteTexture);
            GUI.color = Color.white;
            Rect rect2 = rect.TakeBottomPart(30f);
            Widgets.DrawRectFast(rect2, Widgets.WindowBGFillColor, null);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect2, def.LabelCap);
            GUI.DrawTexture(rect, image);
            Text.Anchor = TextAnchor.UpperLeft;
        }
        public static void DrawAbility(Rect inRect, AbilityDef ability)
        {
            Color color = Mouse.IsOver(inRect) ? GenUI.MouseoverColor : Color.white;
            MouseoverSounds.DoRegion(inRect, SoundDefOf.Mouseover_Command);
            GUI.color = color;
            GUI.DrawTexture(inRect, Command.BGTexShrunk);
            GUI.color = Color.white;
            GUI.DrawTexture(inRect, ContentFinder<Texture2D>.Get(ability.iconPath, true));
        }
        
        private readonly Dictionary<AbilityDef, Vector2> abilityPos = new Dictionary<AbilityDef, Vector2>();
        //private readonly Dictionary<string, List<RWrd_RouteDef>> pathsByTab = new Dictionary<string, List<RWrd_RouteDef>>();
        private readonly List<TabRecord> tabs;
        private string curTab;
        private bool devMode;
        private Hediff_RWrd_PowerRoot hediff;
        private float lastPathsHeight;
        private int pathsPerRow;
        private Vector2 pathsScrollPos;
        private Pawn pawn;
        private Vector2 psysetsScrollPos;
        private static readonly float[][] abilityTreeXOffsets = new float[][]
        {
            new float[] { -18f },                 // 1 ability
            new float[] { -47f, 11f },            // 2 abilities
            new float[] { -69f, -18f, 33f },      // 3 abilities
            new float[] { -90f, -45f, 0f, 45f }   // 4 abilities
        };

    }
}
