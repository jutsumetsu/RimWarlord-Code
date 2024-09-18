using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine.Diagnostics;
using UnityEngine;
using Verse;
using Verse.Sound;
using Electromagnetic.Core;
using Electromagnetic.Setting;

namespace Electromagnetic.UI
{
    public class Dialog_SelectThings : Window
    {
        public override Vector2 InitialSize => new Vector2(620f, 500f);

        public List<ThingDef> allArtifactDefs;
        private readonly ITab_Pawn_RWrd parent;
        private Vector2 scrollPosition;

        string searchKey;

        public Dialog_SelectThings(ITab_Pawn_RWrd parent)
        {
            doCloseButton = false;
            doCloseX = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = false;
            allArtifactDefs = AllThingsDefs.ToList();
            forcePause = true;
            this.parent = parent;
        }
        /// <summary>
        /// 获取物品列表
        /// </summary>
        public static IEnumerable<ThingDef> AllThingsDefs
        {
            get
            {
                IEnumerable <RWrd_ItemFilterDef> filterDefs = DefDatabase<RWrd_ItemFilterDef>.AllDefs;
                List<ThingCategoryDef> CWL = new List<ThingCategoryDef>();
                List<ThingCategoryDef> CBL = new List<ThingCategoryDef>();
                List<ThingDef> TWL = new List<ThingDef>();
                List<ThingDef> TBL = new List<ThingDef>();
                foreach (var filter in filterDefs)
                {
                    if (filter.categoryWhiteList != null)
                    {
                        CWL.AddRange(filter.categoryWhiteList);
                    }
                    if (filter.categoryBlackList != null)
                    {
                        CBL.AddRange(filter.categoryBlackList);
                    }
                    if (filter.thingWhiteList != null)
                    {
                        TWL.AddRange(filter.thingWhiteList);
                    }
                    if (filter.thingBlackList != null)
                    {
                        TBL.AddRange(filter.thingBlackList);
                    }
                }
                var categoryWhiteList = CWL.Distinct().ToList();
                var categoryBlackList = CBL.Distinct().ToList();
                var thingWhiteList = TWL.Distinct().ToList();
                var thingBlackList = TBL.Distinct().ToList();
                return DefDatabase<ThingDef>.AllDefs.Where(delegate (ThingDef x)
                {
                    List<ThingCategoryDef> thingCategories = x.thingCategories;
                    bool InCWL;
                    bool InCBL;
                    if (categoryWhiteList.Count == 0 || thingCategories == null)
                    {
                        InCWL = false;
                    }
                    else
                    {
                        InCWL = thingCategories.Intersect(categoryWhiteList).Any();
                    }
                    if (categoryBlackList.Count == 0 || thingCategories == null)
                    {
                        InCBL = false;
                    }
                    else
                    {
                        InCBL = thingCategories.Intersect(categoryBlackList).Any();
                    }
                    bool flag = InCWL || thingWhiteList.Contains(x);
                    bool flag2 = InCBL || thingBlackList.Contains(x);
                    return thingCategories != null && flag && !flag2;
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

            Pawn pawn = parent.pawn;
            if (parent.GetSelectedPawn() == null)
            {
                Close();
            }

            Rect outRect = new Rect(inRect);
            outRect.y = searchRect.yMax + 5;
            outRect.yMax -= 70f;
            outRect.width -= 16f;

            var thingDefs = searchKey.NullOrEmpty() ? allArtifactDefs : allArtifactDefs.Where(x => x.label.ToLower().Contains(searchKey.ToLower())).ToList();

            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, thingDefs.Count() * 35f);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            try
            {
                float num = 0f;
                foreach (ThingDef thingDef in thingDefs.OrderBy(x => x.label))
                {
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

                    // 在按钮左侧绘制 Slider
                    /*Rect sliderRect = new Rect(iconRect.xMax + 180f, num, 120f, 32f);
                    Widgets.Label(new Rect(sliderRect.x + sliderRect.x / 4, sliderRect.y+5, sliderRect.width, 24f), sliderValues[thingDef].ToString());
                    int maxStackLimit = thingDef.stackLimit;
                    sliderValues[thingDef] = (int)Widgets.HorizontalSlider(sliderRect, sliderValues[thingDef], 1f, maxStackLimit);*/
                    Rect textFieldNumeric = new Rect(iconRect.xMax + 345f, num, 50f, 32f);
                    int textFieldNumber = thingValues[thingDef];
                    string text = textFieldNumber.ToString();
                    int maxNumber = (int)Math.Floor(pawn.GetPowerRoot().energy.energy / CalculateValueToShow(thingDef));
                    Widgets.TextFieldNumeric<int>(textFieldNumeric, ref textFieldNumber, ref text, 1f, Math.Min(thingDef.stackLimit, maxNumber));
                    thingValues[thingDef] = textFieldNumber;


                    //能耗数值
                    float valueToShow = CalculateValueToShow(thingDef) * thingValues[thingDef];
                    float valueToReduce = CalculateValueToShow(thingDef);
                    bool isButtonEnabled = pawn != null && pawn.IsHavePowerRoot() && valueToShow <= pawn.GetPowerRoot().energy.energy;
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
                            for (int i = 0; i < itemCount; i++)
                            {
                                Thing thing = ThingMaker.MakeThing(thingDef, GenStuff.DefaultStuffFor(thingDef));
                                parent.AssignArtifact(thing);
                                SoundDefOf.Click.PlayOneShotOnCamera();
                                GenPlace.TryPlaceThing(thing, pawn.Position, pawn.Map, ThingPlaceMode.Near);
                                pawn.GetPowerRoot().energy.SetEnergy((float)(-valueToReduce));
                                pawn.GetPowerRoot().energy.SetExp(0.1f * (float)(-(float)valueToReduce));
                            }

                            Close();
                        }
                    }
                    num += 35f;
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
        public float CalculateValueToShow(ThingDef thingDef)
        {
            float value = 1f;
            //获取配方
            RecipeDef recipeDef = DefDatabase<RecipeDef>.AllDefs.FirstOrDefault(r => r.products.Any(p => p.thingDef == thingDef));
            //获取工作量及市场价
            float workAmount = recipeDef != null ? recipeDef.workAmount : 0f;
            float baseMarketValue = thingDef.BaseMarketValue;

            workAmount = Math.Max(0f, workAmount);
            baseMarketValue = Math.Max(0f, baseMarketValue);

            // 返回较大的值
            value = Math.Max(workAmount, baseMarketValue);
            return value;
        }

        private Dictionary<ThingDef, int> thingValues = new Dictionary<ThingDef, int>();

    }
}
