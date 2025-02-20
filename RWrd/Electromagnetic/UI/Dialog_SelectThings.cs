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
        private Hediff_RWrd_PowerRoot root;
        private Vector2 scrollPosition;

        string searchKey;

        public Dialog_SelectThings(Hediff_RWrd_PowerRoot root)
        {
            doCloseButton = false;
            doCloseX = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = false;
            allArtifactDefs = PowerRootUtillity.spawnableThings;
            forcePause = true;
            this.root = root;
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
                        x.thingCategories.Any(c => categoryWhiteList.Contains(c));

                    bool hasBlack = categoryBlackList.Count > 0 &&
                        x.thingCategories.Any(c => categoryBlackList.Contains(c));

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
            outRect.yMax -= 10f;

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

                    // 在按钮左侧绘制输入框
                    Rect textFieldNumeric = new Rect(iconRect.xMax + 345f, num, 50f, 32f);
                    int textFieldNumber = thingValues[thingDef];
                    string text = textFieldNumber.ToString();
                    int maxNumber = (int)Math.Floor(root.energy.energy / CalculateValueToShow(thingDef));
                    Widgets.TextFieldNumeric<int>(textFieldNumeric, ref textFieldNumber, ref text, 1f, Math.Min(thingDef.stackLimit, maxNumber));
                    thingValues[thingDef] = textFieldNumber;


                    //能耗数值
                    float valueToShow = CalculateValueToShow(thingDef) * thingValues[thingDef];
                    float valueToReduce = CalculateValueToShow(thingDef);
                    bool isButtonEnabled = pawn != null && valueToShow <= root.energy.energy;
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
                                SoundDefOf.Click.PlayOneShotOnCamera();
                                GenPlace.TryPlaceThing(thing, pawn.Position, pawn.Map, ThingPlaceMode.Near);
                                root.energy.SetEnergy((float)(-valueToReduce));
                                root.energy.SetExp(0.1f * (float)(-(float)valueToReduce));
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
        public static float CalculateValueToShow(ThingDef thingDef)
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
            if (((workAmount != 0f) || (baseMarketValue != 0f && !float.IsNaN(baseMarketValue))) && !float.IsNaN(workAmount))
            {
                value = Math.Max(workAmount, baseMarketValue);
            }
            else
            {
                value = 0f;
            }
            return value;
        }

        private Dictionary<ThingDef, int> thingValues = new Dictionary<ThingDef, int>();

    }
}
