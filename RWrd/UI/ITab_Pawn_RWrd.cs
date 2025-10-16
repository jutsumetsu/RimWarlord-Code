using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Electromagnetic.Abilities;
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

        public float RequestedAbilitySetsHeight { get; private set; }
        //是否可见
        public override bool IsVisible
        {
            get
            {
                Pawn pawn = Find.Selector.SingleSelectedThing as Pawn;
                bool result;
                //判断有无力量之源
                if (pawn != null && pawn.IsHavePowerRoot())
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
        public Pawn GetSelectedPawn()
        {
            return Find.Selector.SingleSelectedThing as Pawn;
        }

        public override void OnOpen()
        {
            base.OnOpen();
            this.pawn = (Pawn)Find.Selector.SingleSelectedThing;
            this.InitCache();
        }
        /// <summary>
        /// 初始化缓存
        /// </summary>
        private void InitCache()
        {
            this.root = pawn.GetPowerRoot();
            UIUtility.abilities = this.pawn.abilities;
            this.abilityPos.Clear();
        }

        protected override void CloseTab()
        {
            base.CloseTab();
            this.pawn = null;
            this.root = null;
            UIUtility.abilities = null;
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

            bool flag3 = this.pawn == null || this.root == null;
            if (!flag3)
            {
                GameFont font = Text.Font;
                TextAnchor anchor = Text.Anchor;
                Text.Font = font;
                Text.Anchor = anchor;
                Rect rect = new Rect(Vector2.one * 20f, this.size - Vector2.one * 40f);
                //左侧矩形
                Rect rect2 = rect.TakeLeftPart(this.size.x * 0.12f);
                //右侧矩形
                Rect rect3 = rect.TakeRightPart(this.size.x * 0.85f);
                rect3.ContractedBy(5f);
                Listing_Standard listing_Standard = new Listing_Standard();
                listing_Standard.Begin(rect2);
                Text.Font = GameFont.Medium;
                //显示人物名称
                listing_Standard.Label(this.pawn.Name.ToStringShort, -1f);
                //显示当前等级
                if (this.root.energy.level == 0)
                {
                    listing_Standard.Label("RWrd_BE".Translate(), -1f, null);
                }
                else
                {
                    if (Tools.IsChineseLanguage)
                    {
                        string realmTitle = " - ";
                        if (this.root.energy.OnMaxLevel)
                        {
                            realmTitle += "RWrd_FinalLevel".Translate();
                        }
                        else if (this.root.energy.level >= 75)
                        {
                            realmTitle += "RWrd_AtomSplitLevel".Translate();
                        }
                        else if (this.root.energy.level >= 50)
                        {
                            realmTitle += "RWrd_AntigravityLevel".Translate();
                        }
                        if (this.root.Qigong)
                        {
                            listing_Standard.Label("RWrd_QigongLevel".Translate(this.root.energy.level.ToString()) + "匹", -1f, null);
                        }
                        else
                        {
                            listing_Standard.Label("RWrd_BM".Translate(this.root.energy.level.ToString()) + "匹", -1f, null);
                        }
                        if (realmTitle != " - ")
                        {
                            realmTitle += " - ";
                            listing_Standard.Label(realmTitle, -1f);
                        }
                    }
                    else
                    {
                        if (this.root.Qigong)
                        {
                            listing_Standard.Label("RWrd_QigongLevel".Translate(this.root.energy.level.ToString()), -1f, null);
                        }
                        else
                        {
                            listing_Standard.Label("RWrd_BM".Translate(this.root.energy.level.ToString()), -1f, null);
                        }
                    }
                }
                listing_Standard.Gap(10f);
                //经验值进度条
                Rect rectBar = listing_Standard.GetRect(30f, 1f);
                rectBar.x -= 10f;
                Widgets.FillableBar(rectBar, this.root.energy.Exp / this.root.energy.MaxExp);
                //经验值百分比
                float num = this.root.energy.Exp / this.root.energy.MaxExp;
                Vector2 offset = new Vector2(rectBar.center.x - 5f, rectBar.center.y - rectBar.height / 2f);
                Rect rectBarCenter = new Rect(offset, rectBar.size);
                Widgets.Label(rectBarCenter, num.ToString("P2"));
                //经验获取提示
                Text.Font = GameFont.Tiny;
                listing_Standard.Label("RWrd_EarnXP".Translate(), -1f, null);
                listing_Standard.Gap(10f);
                Text.Font = GameFont.Small;
                if (this.root.energy.IsUltimate)
                {
                    listing_Standard.Label("RWrd_Ultimate".Translate() + ": " + this.root.energy.PowerEnergy.ToString() + "RWrd_UltimatePower".Translate(), -1f, null);
                }
                listing_Standard.Label("RWrd_CompleteRealm".Translate() + ": " + this.root.energy.completerealm.ToString("0.##") + "/10000", -1f, null);
                listing_Standard.Label("RWrd_PowerFlow".Translate() + ": " + this.root.energy.powerflow.ToString("0.##") + "/100000000", -1f, null);
                listing_Standard.Label("RWrd_MartialTalent".Translate() + ": " + this.root.MartialTalent.ToString("0.##"), -1f, null);
                listing_Standard.Label("RWrd_TrainingDesire".Translate() + ": " + this.root.energy.trainDesireFactor.ToString(), -1f, null);
                //力量体系介绍按钮
                Rect buttonRect = listing_Standard.GetRect(40f, 1f);
                Rect buttonLeft = listing_Standard.GetRect(40f, 0.5f);
                Rect buttonRight = new Rect(buttonLeft.x + buttonLeft.width, buttonLeft.y, buttonLeft.width, buttonLeft.height);
                Rect buttonRect2 = listing_Standard.GetRect(40f, 1f);
                if (Widgets.ButtonText(buttonRect, "RWrd_IntroduceButton".Translate()))
                {
                    Find.WindowStack.Add(new Dialog_PowerIntroduce());
                }
                //出力设置按钮&&技能组按钮
                if (Mouse.IsOver(buttonLeft))
                {
                    TooltipHandler.TipRegion(buttonLeft, "RWrd_OPIntroduce".Translate());
                }
                if (Widgets.ButtonText(buttonLeft, "RWrd_OutputPower".Translate()))
                {
                    var powerOption = new Dialog_OutputPower(this);
                    Find.WindowStack.Add(powerOption);
                }
                if (Widgets.ButtonText(buttonRight, "RWrd_AbilitySets".Translate()))
                {
                    var editSets = new Dialog_EditAbilitySets(this);
                    Find.WindowStack.Add(editSets);
                }
                if (DebugSettings.godMode && Widgets.ButtonText(buttonRect2, "RWrd_ChangeEMStats".Translate()))
                {
                    var EMStats = new Dialog_ChangeEMStats(this);
                    Find.WindowStack.Add(EMStats);
                }
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
            IEnumerable<RWrd_RouteDef> paths = DefDatabase<RWrd_RouteDef>.AllDefs;
            foreach (RWrd_RouteDef def in paths)
            {
                Texture2D texture2D = def.backgroundImage;
                float num4 = num / (float)texture2D.width * (float)texture2D.height + 30f;
                Rect rect = new Rect(vector, new Vector2(num, num4));
                //绘制技能树背景图
                DrawPathBackground(ref rect, def);
                rect.height -= 30f;
                bool flag = this.root.routes.Contains(def);
                if (flag)
                {
                    //绘制技能图标
                    DoPathAbilities(rect, def, this.abilityPos, new Action<Rect, AbilityDef, RWrd_RouteNode>(this.DoAbility));
                }
                else
                {
                    //涂暗
                    Widgets.DrawRectFast(rect, new Color(0f, 0f, 0f, 0.55f), null);
                    //工具提示
                    TooltipHandler.TipRegion(rect, () => ((def.unlockRequired != null) ? "RWrd_Locked".Translate().Resolve() + def.unlockRequired + "\n\n" : "") + def.description + "\n\n" + "RWrd_AbilitiesList".Translate() + "\n" + (from ab in def.AllAbilities select ab.label).ToLineList("  ", true), def.GetHashCode());
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

        private void DoAbility(Rect inRect, AbilityDef abilityDef, RWrd_RouteNode node)
        {
            //判断小人是否拥有该技能
            bool flag = this.pawn.abilities.GetAbility(abilityDef) == null;
            RWrd_AbilityBase ability;
            float mastery = 0;
            DrawAbility(inRect, abilityDef);
            if (flag)
            {
                //涂暗
                Widgets.DrawRectFast(inRect, new Color(0f, 0f, 0f, 0.6f), null);
            }
            else
            {
                //发光
                Widgets.DrawStrongHighlight(inRect.ExpandedBy(2f));
                ability = (RWrd_AbilityBase)this.pawn.abilities.GetAbility(abilityDef);
                mastery = ability.mastery;
            }
            TooltipHandler.TipRegion(inRect, () => string.Format("{0}\n\n{1}\n{2}", abilityDef.label, abilityDef.description, flag ? ("\n" + "RWrd_Locked".Translate().Resolve()) + "\n" + node.unlockRequired : "\n" + "RWrd_Mastery".Translate().Resolve() + mastery.ToString()), abilityDef.GetHashCode());

        }
        public static void DoPathAbilities(Rect inRect, RWrd_RouteDef path, Dictionary<AbilityDef, Vector2> abilityPos, Action<Rect, AbilityDef, RWrd_RouteNode> doAbility)
        {
            // 获取每个节点的技能和对应层级
            var routeNodes = path.routeNodes;
            Dictionary<AbilityDef, Rect> abilityRect = new Dictionary<AbilityDef, Rect>();

            for (int level = 1; level <= path.MaxLevel; level++)
            {
                //按照层级创建技能列表
                List<AbilityDef> abilities = new List<AbilityDef>();
                foreach (var node in routeNodes)
                {
                    if (node.level == level)
                    {
                        abilities.AddRange(node.abilities);
                    }
                }
                // 为每个层级创建一个矩形区域
                Rect rect = new Rect(inRect.x, inRect.y + (float)level * inRect.height / 8f, inRect.width, inRect.height / 8f);

                // 检查 abilityTreeXOffsets是否足够长
                if (abilities.Count - 1 < abilityTreeXOffsets.Length)
                {
                    // 遍历该层级中的所有能力
                    for (int j = 0; j < abilities.Count; j++)
                    {
                        // 检查 abilityTreeXOffsets是否足够长
                        if (j < abilityTreeXOffsets[abilities.Count - 1].Length)
                        {
                            //获取技能绘制信息
                            Rect arg = new Rect(rect.x + rect.width / 2f + abilityTreeXOffsets[abilities.Count - 1][j], rect.y, 36f, 36f);
                            AbilityDef abilityDef = abilities[j];
                            abilityRect[abilityDef] = arg;
                            abilityPos[abilityDef] = arg.center;
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
            //绘制技能连线
            foreach (var node in routeNodes)
            {
                if (node.preNode != 0)
                {
                    RWrd_RouteNode preNode;
                    foreach (var node2 in routeNodes)
                    {
                        if (node2.number == node.preNode)
                        {
                            preNode = node2;
                            foreach (var abilityDef2 in preNode.abilities)
                            {
                                foreach (var abilityDef in node.abilities)
                                {
                                    Widgets.DrawLine(abilityPos[abilityDef], abilityPos[abilityDef2], (UIUtility.abilities.GetAbility(abilityDef) != null) ? Color.white : Color.grey, 2f);
                                }
                            }
                        }
                    }
                }
            }
            //绘制技能
            foreach (var node in routeNodes)
            {
                foreach (var abilityDef in node.abilities)
                {
                    doAbility(abilityRect[abilityDef], abilityDef, node);
                }
            }
        }
        //绘制技能树背景
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
            //工具提示
            TooltipHandler.TipRegion(rect3, () => def.description + "\n\n" + "RWrd_AbilitiesList".Translate() + "\n" + (from ab in def.AllAbilities select ab.label).ToLineList("  ", true), def.GetHashCode());
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
        public void DoAbilitySets(Rect inRect)
        {
            var dialogTitleLabel = new Rect(inRect.x, inRect.y, 300, 24);
            Widgets.Label(dialogTitleLabel, "RWrd_AbilitySetSetting".Translate(pawn.Name.ToStringShort));
            Listing_Standard listing_Standard = new Listing_Standard();
            Rect outRect = new Rect(inRect);
            outRect.y += 30;
            outRect.yMax -= 70f;
            listing_Standard.Begin(outRect);
            if (this.root.abilitySets.Count > 0)
            {
                foreach (AbilitySet abilitySet in this.root.abilitySets.ToList<AbilitySet>())
                {
                    Rect rect = listing_Standard.GetRect(30f, 1f);
                    Widgets.Label(rect.LeftHalf().LeftHalf(), abilitySet.Name);
                    bool flag = Widgets.ButtonText(rect.LeftHalf().RightHalf(), "RWrd_Rename".Translate(), true, true, true, null);
                    if (flag)
                    {
                        Find.WindowStack.Add(new Dialog_RenameAbilitySet(abilitySet));
                    }
                    bool flag2 = Widgets.ButtonText(rect.RightHalf().LeftHalf(), "RWrd_Edit".Translate(), true, true, true, null);
                    if (flag2)
                    {
                        Find.WindowStack.Add(new Dialog_AbilitySet(abilitySet, this.pawn));
                    }
                    bool flag3 = Widgets.ButtonText(rect.RightHalf().RightHalf(), "RWrd_Remove".Translate(), true, true, true, null);
                    if (flag3)
                    {
                        this.root.RemoveAbilitySet(abilitySet);
                    }
                }
            }
            bool flag4 = Widgets.ButtonText(listing_Standard.GetRect(70f, 1f).LeftHalf().ContractedBy(5f), "RWrd_CreateAbilitySet".Translate(), true, true, true, null);
            if (flag4)
            {
                AbilitySet abilitySet2 = new AbilitySet
                {
                    Name = "RWrd_Untitled".Translate()
                };
                this.root.abilitySets.Add(abilitySet2);
                Find.WindowStack.Add(new Dialog_AbilitySet(abilitySet2, this.pawn));
            }
            this.RequestedAbilitySetsHeight = listing_Standard.CurHeight + 70f;
            listing_Standard.End();
        }

        private readonly Dictionary<AbilityDef, Vector2> abilityPos = new Dictionary<AbilityDef, Vector2>();
        //private readonly Dictionary<string, List<RWrd_RouteDef>> pathsByTab = new Dictionary<string, List<RWrd_RouteDef>>();
        private readonly List<TabRecord> tabs;
        private string curTab;
        private bool devMode;
        public Hediff_RWrd_PowerRoot root;
        private float lastPathsHeight;
        private int pathsPerRow;
        private Vector2 pathsScrollPos;
        public Pawn pawn;
        private Vector2 psysetsScrollPos;
        private static readonly float[][] abilityTreeXOffsets = new float[][]
        {
            new float[] { -18f },                   // 1 ability
            new float[] { -47f, 11f },               // 2 abilities
            new float[] { -69f, -18f, 33f },          // 3 abilities
            new float[] { -84f, -40f, 4f, 48f },       // 4 abilities
            new float[] { -92f, -55f, -18f, 19f, 56f }  // 5 abilities
        };

    }
}
