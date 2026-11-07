using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using RimWorld;
using UnityEngine.Diagnostics;
using UnityEngine;
using Verse;
using Verse.Sound;
using Electromagnetic.Core;
using Electromagnetic.Setting;
using Electromagnetic.Abilities;

namespace Electromagnetic.UI
{
    public class Dialog_SelectThings : Window
    {
        public override Vector2 InitialSize => new Vector2(620f, 500f);

        public List<ThingDef> allArtifactDefs;
        private Hediff_RWrd_PowerRoot root;
        private RWrd_AbilityBase ability;
        private float mastery = 0;
        private Vector2 scrollPosition;

        string searchKey;

        public Dialog_SelectThings(Hediff_RWrd_PowerRoot root, RWrd_AbilityBase ability = null)
        {
            doCloseButton = false;
            doCloseX = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = false;
            allArtifactDefs = PowerRootUtillity.spawnableThings;
            forcePause = true;
            this.root = root;
            this.ability = ability;
            this.mastery = ability.mastery;
            baseCostCache = new Dictionary<ThingDef, float>();
        }
        /// <summary>
        /// 获取物品列表
        /// </summary>
        public static IEnumerable<ThingDef> AllThingsDefs
        {
            get
            {
                var filters = DefDatabase<RWrd_ItemFilterDef>.AllDefs;

                var categoryWhiteList = filters
                    .SelectNotNull<RWrd_ItemFilterDef, ThingCategoryDef>(f => f.categoryWhiteList)
                    .Distinct()
                    .ToHashSet();

                var categoryBlackList = filters
                    .SelectNotNull<RWrd_ItemFilterDef, ThingCategoryDef>(f => f.categoryBlackList)
                    .Distinct()
                    .ToHashSet();

                var thingWhiteList = filters
                    .SelectNotNull<RWrd_ItemFilterDef, ThingDef>(f => f.thingWhiteList)
                    .Distinct()
                    .ToHashSet();

                var thingBlackList = filters
                    .SelectNotNull<RWrd_ItemFilterDef, ThingDef>(f => f.thingBlackList)
                    .Distinct()
                    .ToHashSet();

                return DefDatabase<ThingDef>.AllDefs.Where(x =>
                {
                    if (x.thingCategories == null)
                        return false;

                    bool hasWhite = categoryWhiteList.Count > 0 &&
                        x.thingCategories.Any(c => categoryWhiteList.Any(w => c == w || c.Parents.Contains(w)));
                    bool hasBlack = categoryBlackList.Count > 0 &&
                        x.thingCategories.Any(c => categoryBlackList.Any(b => c == b || c.Parents.Contains(b)));

                    return (hasWhite || thingWhiteList.Contains(x)) &&
                           !(hasBlack || thingBlackList.Contains(x)) &&
                           CalculateValueToShow(x) != 0;
                });
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Text.Anchor = TextAnchor.MiddleLeft;
            var searchLabel = new Rect(inRect.x, inRect.y, 60, 24);
            Widgets.Label(searchLabel, "RWrd_Search".Translate());
            var searchRect = new Rect(searchLabel.xMax + 5, searchLabel.y, 200, 24f);
            searchKey = Widgets.TextField(searchRect, searchKey);
            Text.Anchor = TextAnchor.UpperLeft;

            Pawn pawn = root.pawn;

            Rect outRect = new Rect(inRect);
            outRect.y = searchRect.yMax + 5;
            outRect.height = inRect.yMax - outRect.y - 10f;

            float offset = this.mastery / 100 * 0.5f;
            baseCostCache.Clear();

            var filtered = allArtifactDefs.Where(x => Mathf.Clamp(
                    Mathf.FloorToInt(root.energy.energy / CostForDraw(x, offset)),
                    0, x.stackLimit) > 0);

            if (!searchKey.NullOrEmpty())
            {
                string key = searchKey.ToLower();
                filtered = filtered.Where(x => x.label.ToLower().Contains(key));
            }

            List<ThingDef> displayItem = SortForDraw(filtered.ToList(), x => x.label);

            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, displayItem.Count * 35f);
            Widgets.AdjustRectsForScrollView(outRect, ref outRect, ref viewRect);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            int firstRow = Mathf.Max(0, Mathf.FloorToInt(scrollPosition.y / 35f) - 1);
            int lastRow = Mathf.Min(displayItem.Count() - 1, Mathf.CeilToInt((scrollPosition.y + outRect.height) / 35f) + 1);
            try
            {
                for (int i = firstRow; i <= lastRow; i++)
                {
                    ThingDef thingDef = displayItem[i];
                    int maxNumber = Mathf.Clamp(
                        Mathf.FloorToInt(root.energy.energy / CostForDraw(thingDef, offset)),
                        0, thingDef.stackLimit);
                    float num = i * 35f;
                    //图标矩形
                    Rect iconRect = new Rect(0f, num, 24, 32);
                    //详情界面按钮
                    Widgets.InfoCardButton(iconRect, thingDef);
                    iconRect.x += 24;
                    //物品图标
                    Widgets.ThingIcon(iconRect, thingDef);
                    //物品名称
                    Rect rect = new Rect(iconRect.xMax + 5, num, viewRect.width * 0.7f, 32f);
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.LabelFit(rect, thingDef.LabelCap);
                    Text.Anchor = TextAnchor.UpperLeft;
                    rect.x = rect.xMax + 10;
                    rect.width = 100;

                    if (!thingValues.ContainsKey(thingDef))
                    {
                        thingValues[thingDef] = 1; // 默认值
                    }

                    // 在按钮左侧绘制输入框
                    Rect textFieldNumeric = new Rect(iconRect.xMax + 345f, num, 50f, 32f);
                    int textFieldNumber = thingValues[thingDef];
                    string text = textFieldNumber.ToString();
                    /*int maxNumber = (int)Math.Floor(root.energy.energy / CalculateValueToShow(thingDef, offset));*/
                    /*Widgets.TextFieldNumeric<int>(textFieldNumeric, ref textFieldNumber, ref text, 1f, Math.Min(thingDef.stackLimit, maxNumber));
                    thingValues[thingDef] = textFieldNumber;*/
                    int stored = thingValues[thingDef];
                    int count = Mathf.Clamp(stored, 0, maxNumber);
                    string buffer = countBuffers.TryGetValue(thingDef, out string b) ? b : count.ToString();
                    if (count != stored) buffer = count.ToString();
                    Widgets.TextFieldNumeric<int>(textFieldNumeric, ref count, ref buffer, 0f, maxNumber);
                    thingValues[thingDef] = Mathf.Clamp(count, 0, maxNumber);
                    countBuffers[thingDef] = buffer;


                    //能耗数值
                    float valueToShow = CostForDraw(thingDef, offset) * thingValues[thingDef];
                    float valueToReduce = CostForDraw(thingDef, offset);
                    bool isButtonEnabled = pawn != null &&  valueToShow <= root.energy.energy;
                    //能耗显示
                    if (Mouse.IsOver(rect))
                    {
                        TooltipHandler.TipRegion(rect, "RWrd_EnergyReduce".Translate() + valueToShow.ToString());
                    }
                    //创造物品按钮
                    if (Widgets.ButtonText(rect, "RWrd_Create".Translate(),true,true, isButtonEnabled))
                    {
                        if (isButtonEnabled)
                        {
                            int itemCount = thingValues[thingDef];
                            for (int j = 0; j < itemCount; j++)
                            {
                                Thing thing = ThingMaker.MakeThing(thingDef, GenStuff.DefaultStuffFor(thingDef));
                                SoundDefOf.Click.PlayOneShotOnCamera();
                                GenPlace.TryPlaceThing(thing, pawn.Position, pawn.Map, ThingPlaceMode.Near);
                                root.energy.SetEnergy(-(float)valueToReduce);
                                root.energy.SetExp(0.1f * -(float)valueToReduce);
                                ability.SetMastery(0.05f);
                            }

                            Close();
                        }
                    }
                }
            }
            finally
            {
                Widgets.EndScrollView();
            }
        }
        /// <summary>
        /// 计算物品EMC
        /// </summary>
        /// <param name="thingDef"></param>
        /// <returns></returns>
        public static float CalculateValueToShow(ThingDef thingDef, float offset = 0)
        {
            float value = 1f;
            //获取配方
            RecipeDef recipeDef = DefDatabase<RecipeDef>.AllDefs.FirstOrDefault(r => r.products.Any(p => p.thingDef == thingDef));
            //获取工作量及市场价
            float workAmount = recipeDef != null ? recipeDef.workAmount : 0f;
            float baseMarketValue = thingDef.BaseMarketValue;

            if (float.IsNaN(baseMarketValue)) baseMarketValue = 0f;
            if (float.IsNaN(workAmount)) workAmount = 0f;

            workAmount = Math.Max(0f, workAmount);
            baseMarketValue = Math.Max(0f, baseMarketValue);

            // 返回较大的值
            if (workAmount != 0f || baseMarketValue != 0f)
            {
                value = Math.Max(workAmount, baseMarketValue);
            }
            else
            {
                value = 0f;
            }
            value *= 1 - offset;
            return value;
        }
        private void EnsureRecipeIndex()
        {
            if (recipeByProduct != null)
                return;

            recipeByProduct = new Dictionary<ThingDef, RecipeDef>();
            foreach (RecipeDef recipe in DefDatabase<RecipeDef>.AllDefs)
            {
                foreach (ThingDefCountClass product in recipe.products)
                {
                    if (!recipeByProduct.ContainsKey(product.thingDef))
                        recipeByProduct.Add(product.thingDef, recipe);
                }
            }
        }
        /// <summary>
        /// 基础消耗
        /// </summary>
        /// <param name="thingDef"></param>
        /// <returns></returns>
        private float BaseCost(ThingDef thingDef)
        {
            if (baseCostCache.TryGetValue(thingDef, out float cached))
                return cached;

            EnsureRecipeIndex();
            recipeByProduct.TryGetValue(thingDef, out RecipeDef recipe);

            float workAmount = recipe != null ? recipe.workAmount : 0f;
            float baseMarketValue = thingDef.BaseMarketValue;
            if (float.IsNaN(baseMarketValue)) baseMarketValue = 0f;
            if (float.IsNaN(workAmount)) workAmount = 0f;
            workAmount = Math.Max(0f, workAmount);
            baseMarketValue = Math.Max(0f, baseMarketValue);

            float value;
            if (workAmount != 0f || baseMarketValue != 0f) value = Math.Max(workAmount, baseMarketValue);
            else value = 0f;

            baseCostCache[thingDef] = value;
            return value;
        }

        private float CostForDraw(ThingDef thingDef, float offset)
        {
            return BaseCost(thingDef) * (1f - offset);
        }
        private List<ThingDef> SortForDraw(List<ThingDef> source, Func<ThingDef, string> selector)
        {
            bool reuse = sortedSource != null
                && sortedCulture == CultureInfo.CurrentCulture
                && source.Count == sortedSource.Count;

            if (reuse)
            {
                for (int i = 0; i < source.Count; i++)
                {
                    if (source[i] != sortedSource[i] || selector(source[i]) != sortedLabels[i])
                    {
                        reuse = false;
                        break;
                    }
                }
            }

            if (reuse) return sortedDefs;

            sortedSource = new List<ThingDef>(source);
            sortedLabels = new string[source.Count];
            for (int i = 0; i < source.Count; i++) sortedLabels[i] = selector(source[i]);
            sortedCulture = CultureInfo.CurrentCulture;
            sortedDefs = source.OrderBy(selector).ToList();
            return sortedDefs;
        }
        private Dictionary<ThingDef, int> thingValues = new Dictionary<ThingDef, int>();
        private Dictionary<ThingDef, string> countBuffers = new Dictionary<ThingDef, string>();
        private Dictionary<ThingDef, RecipeDef> recipeByProduct;
        private Dictionary<ThingDef, float> baseCostCache;
        private List<ThingDef> sortedDefs;
        private List<ThingDef> sortedSource;
        private string[] sortedLabels;
        private CultureInfo sortedCulture;
    }
}
