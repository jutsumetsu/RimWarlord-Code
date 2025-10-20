using Electromagnetic.Core;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Electromagnetic.UI
{
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
