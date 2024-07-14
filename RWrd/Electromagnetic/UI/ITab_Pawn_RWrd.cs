using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
        //选项卡信息
        public ITab_Pawn_RWrd()
        {
            this.labelKey = "RWrd_ITab".Translate();
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
        //是否可见
        public override bool IsVisible
        {
            get
            {
                Pawn pawn = Find.Selector.SingleSelectedThing as Pawn;
                bool result;
                //判断有无力量之源
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
        //填充选项卡界面
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

                /*Widgets.DrawBoxSolid(rect, Color.gray);*/
                //游戏语言检测
                bool lflag = LanguageDatabase.activeLanguage.ToString() != "Simplified Chinese";
                bool lflag2 = LanguageDatabase.activeLanguage.ToString() != "Traditional Chinese";

                Text.Font = font;
                Text.Anchor = anchor;
                Rect rect = new Rect(Vector2.one * 20f, this.size - Vector2.one * 40f);
                //左侧矩形
                Rect rect2 = rect.TakeLeftPart(this.size.x * 0.2f);
                //右侧矩形
                Rect rect3 = rect.TakeRightPart(this.size.x * 0.8f);
                rect3.ContractedBy(5f);
                Listing_Standard listing_Standard = new Listing_Standard();
                listing_Standard.Begin(rect2);
                Text.Font = GameFont.Medium;
                //显示人物名称
                listing_Standard.Label(this.pawn.Name.ToStringFull, -1f, null);
                //显示当前等级
                if (lflag && lflag2)
                {
                    listing_Standard.Label(this.hediff.energy.CurrentDef.label, -1f, null);
                }
                else
                {
                    int level = this.hediff.energy.CurrentDef.level;
                    if (level > 0)
                    {
                        listing_Standard.Label(this.hediff.energy.CurrentDef.label + "匹", -1f, null);
                    }
                    else
                    {
                        listing_Standard.Label(this.hediff.energy.CurrentDef.label, -1f, null);
                    }
                }
                //经验值进度条
                Rect rectBar = listing_Standard.GetRect(30f, 1f);
                rectBar.x -= 50f;
                rectBar.width -= 100f;
                Widgets.FillableBar(rectBar, this.hediff.energy.Exp / this.hediff.energy.CurrentDef.EXP);
                //经验值百分比
                float num = this.hediff.energy.Exp / this.hediff.energy.CurrentDef.EXP;
                Vector2 offset = new Vector2(rectBar.center.x - 5f, rectBar.center.y - rectBar.height / 2f);
                Rect rectBarCenter = new Rect(offset, rectBar.size);
                Widgets.Label(rectBarCenter, num.ToString("P2"));
                //经验获取提示
                Text.Font = GameFont.Tiny;
                listing_Standard.Label("RWrd_EarnXP".Translate(), -1f, null);
                listing_Standard.Gap(10f);
                listing_Standard.End();
                //绘制技能树显示区域
                TabDrawer.DrawTabs<TabRecord>(new Rect(rect3.x, rect3.y + 40f, rect3.width, rect3.height), this.tabs, 200f);
                rect3.yMin += 40f;
                Widgets.DrawMenuSection(rect3);
                Rect rect8 = new Rect(rect3.x, rect3.y, rect3.width - 20f, this.lastPathsHeight);
                //绘制技能树
                this.DoPaths(rect8);
            }
        }

        private void DoPaths(Rect inRect)
        {
            Vector2 vector = inRect.position + Vector2.one * 10f;
            float num = (inRect.width - (float)(this.pathsPerRow + 1) * 10f) / (float)this.pathsPerRow;
            float num2 = 0f;
            int num3 = this.pathsPerRow;
            //读取所有技能树
            IEnumerable<RWrd_RouteDef> prepaths = DefDatabase<RWrd_RouteDef>.AllDefs
                                                .OrderBy((RWrd_RouteDef path) => path.label.Substring(0, 1));
            IEnumerable<RWrd_RouteDef> paths = prepaths.Reverse();
            foreach (RWrd_RouteDef def in paths)
            {
                Texture2D texture2D = def.backgroundImage;
                float num4 = num / (float)texture2D.width * (float)texture2D.height + 30f;
                Rect rect = new Rect(vector, new Vector2(num, num4));
                //绘制技能树背景图
                DrawPathBackground(ref rect, def);
                rect.height -= 30f;
                bool flag = this.hediff.routes.Contains(def);
                if (flag)
                {
                    //绘制技能图标
                    DoPathAbilities(rect, def, this.abilityPos, new Action<Rect, AbilityDef>(this.DoAbility));
                }
                else
                {
                    //涂暗
                    Widgets.DrawRectFast(rect, new Color(0f, 0f, 0f, 0.55f), null);
                }

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
            //判断小人是否拥有该技能
            bool flag = this.pawn.abilities.GetAbility(ability) == null;
            DrawAbility(inRect, ability);
            if (flag)
            {
                //涂暗
                Widgets.DrawRectFast(inRect, new Color(0f, 0f, 0f, 0.6f), null);
            }
            else
            {
                //发光
                Widgets.DrawStrongHighlight(inRect.ExpandedBy(5f), null);
            }
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
                Rect rect = new Rect(inRect.x, inRect.y + (float)level * inRect.height / 8f, inRect.width, inRect.height / 8f);

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
            Rect rect2 = new Rect(rect.x, rect.y, rect.width, rect.height - 30f);
            Widgets.DrawBox(rect.ExpandedBy(2f), 1, Texture2D.whiteTexture);
            GUI.DrawTexture(rect2, image);
            GUI.color = Color.white;
            Rect rect3 = rect.TakeBottomPart(30f);
            Widgets.DrawRectFast(rect3, Widgets.WindowBGFillColor, null);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect3, def.label);
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
