using Electromagnetic.Core;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

// ============================================================================
// ThingTool.cs —— 借自「人物编辑器」(Character Editor) 的物品辅助
// 用途：物品的「材料 / 颜色 / 样式 / 品质」选择与显示，颜色选择器/物品选择对话框用。
// 关键点：材料和样式都是「先列候选、再按索引取」，索引越界自动回到 0；
//         colorGenerator 用于随机色物品，取 ExemplaryColor 作为代表色。
// ============================================================================
namespace Electromagnetic.UI
{
    /// <summary>
    /// ThingDef/Thing 的材料、颜色、样式与品质辅助。静态类，无状态。
    /// </summary>
    /// <remarks>
    /// 成员：
    /// <list type="bullet">
    /// <item><description><c>GetStuff(t, ref set, ref index, random)</c> —— 列出可用材料（按材料类别、label 排序），random 时随机索引。</description></item>
    /// <item><description><c>GetColor(t, stuff)</c> —— 材料色 → 矿脉 colorTwo → 色板代表色 → graphicData.color → 白。</description></item>
    /// <item><description><c>GetStyle / ListOfThingStyleDefs</c> —— 反查该 ThingDef 可用的 ThingStyleDef（withNull 可含「无样式」）。</description></item>
    /// <item><description><c>GetQuality(t)</c> —— 品质转 int，无品质返回 0。</description></item>
    /// </list>
    /// </remarks>
    internal static class ThingTool
    {
        internal static ThingDef GetStuff(this ThingDef thingDef, ref HashSet<ThingDef> lOfStuff, ref int stuffIndex, bool random = false)
        {
            lOfStuff = new HashSet<ThingDef>();
            bool flag = thingDef == null || !thingDef.MadeFromStuff;
            ThingDef result;
            if (flag)
            {
                result = null;
            }
            else
            {
                lOfStuff = GenStuff.AllowedStuffsFor(thingDef, TechLevel.Undefined, false).OrderBy(delegate (ThingDef d)
                {
                    StuffProperties stuffProps = d.stuffProps;
                    return (stuffProps != null) ? stuffProps.categories.FirstOrDefault<StuffCategoryDef>().SDefname() : null;
                }).ThenBy((ThingDef t) => t.label).ToHashSet<ThingDef>();
                if (random)
                {
                    stuffIndex = Tools.zufallswert.Next(lOfStuff.Count);
                }
                bool flag2 = !lOfStuff.NullOrEmpty<ThingDef>();
                if (flag2)
                {
                    bool flag3 = lOfStuff.Count > stuffIndex;
                    if (flag3)
                    {
                        result = lOfStuff.At(stuffIndex);
                    }
                    else
                    {
                        stuffIndex = 0;
                        result = lOfStuff.At(stuffIndex);
                    }
                }
                else
                {
                    result = null;
                }
            }
            return result;
        }
        internal static Color GetColor(this ThingDef t, ThingDef stuff)
        {
            bool flag = t == null;
            Color result;
            if (flag)
            {
                result = Color.white;
            }
            else
            {
                bool flag2 = stuff != null && t.MadeFromStuff;
                if (flag2)
                {
                    result = stuff.stuffProps.color;
                }
                else
                {
                    bool flag3 = t.mineable && t.graphicData != null;
                    if (flag3)
                    {
                        result = t.graphicData.colorTwo;
                    }
                    else
                    {
                        bool flag4 = t.colorGenerator != null;
                        if (flag4)
                        {
                            result = t.colorGenerator.ExemplaryColor;
                        }
                        else
                        {
                            bool flag5 = t.graphicData != null;
                            if (flag5)
                            {
                                result = t.graphicData.color;
                            }
                            else
                            {
                                result = Color.white;
                            }
                        }
                    }
                }
            }
            return result;
        }
        internal static ThingStyleDef GetStyle(this ThingDef thingDef, ref HashSet<ThingStyleDef> l, ref int styleIndex, bool random)
        {
            l = new HashSet<ThingStyleDef>();
            bool flag = thingDef == null || !thingDef.CanBeStyled();
            ThingStyleDef result;
            if (flag)
            {
                result = null;
            }
            else
            {
                l = ThingTool.ListOfThingStyleDefs(thingDef, null, true);
                if (random)
                {
                    styleIndex = Tools.zufallswert.Next(l.Count);
                }
                bool flag2 = !l.NullOrEmpty<ThingStyleDef>();
                if (flag2)
                {
                    bool flag3 = l.Count <= styleIndex;
                    if (flag3)
                    {
                        styleIndex = 0;
                    }
                    result = l.At(styleIndex);
                }
                else
                {
                    result = null;
                }
            }
            return result;
        }
        internal static HashSet<ThingStyleDef> ListOfThingStyleDefs(ThingDef thingDef, string modname, bool withNull)
        {
            HashSet<ThingStyleDef> hashSet = new HashSet<ThingStyleDef>();
            if (withNull)
            {
                hashSet.Add(null);
            }
            bool flag = thingDef == null;
            HashSet<ThingStyleDef> result;
            if (flag)
            {
                result = hashSet;
            }
            else
            {
                bool flag2 = modname.NullOrEmpty();
                HashSet<StyleCategoryDef> hashSet2 = DefTool.ListByMod<StyleCategoryDef>(modname);
                foreach (StyleCategoryDef styleCategoryDef in hashSet2)
                {
                    foreach (ThingDefStyle thingDefStyle in styleCategoryDef.thingDefStyles)
                    {
                        bool flag3 = thingDef == thingDefStyle.ThingDef;
                        if (flag3)
                        {
                            hashSet.Add(thingDefStyle.StyleDef);
                        }
                    }
                }
                result = hashSet;
            }
            return result;
        }
        internal static int GetQuality(this Thing t)
        {
            QualityCategory qualityCategory;
            bool flag = t.TryGetQuality(out qualityCategory);
            int result;
            if (flag)
            {
                result = (int)qualityCategory;
            }
            else
            {
                result = 0;
            }
            return result;
        }
    }
}
