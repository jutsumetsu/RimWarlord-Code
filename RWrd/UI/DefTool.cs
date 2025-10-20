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
    internal static class DefTool
    {
        internal static string SDefname(this Def d)
        {
            return (d == null || d.defName.NullOrEmpty()) ? "" : d.defName;
        }
        internal static HashSet<T> ListByMod<T>(string name) where T : Def
        {
            return (from x in DefDatabase<T>.AllDefs
                    where !x.IsNullOrEmpty() && (name.NullOrEmpty() || x.IsFromMod(name))
                    orderby x.label
                    select x).ToHashSet<T>();
        }
        internal static HashSet<T> ListByMod<T>(string name, Func<T, bool> condition) where T : Def
        {
            return (from x in DefDatabase<T>.AllDefs
                    where !x.IsNullOrEmpty() && (name.NullOrEmpty() || x.IsFromMod(name)) && condition(x)
                    orderby x.label
                    select x).ToHashSet<T>();
        }

        internal static List<T> ListAll<T>() where T : Def
        {
            return (from x in DefDatabase<T>.AllDefs
                    orderby x.label
                    select x).ToList<T>();
        }
        internal static bool IsNullOrEmpty(this Def d)
        {
            return d == null || d.defName.NullOrEmpty();
        }
        internal static bool IsFromMod(this Def d, string modname)
        {
            return d != null && d.modContentPack != null && d.modContentPack.Name == modname;
        }
        internal static HashSet<T> ListBy<T>(Func<T, bool> condition) where T : Def
        {
            return (from x in DefDatabase<T>.AllDefs
                    where condition(x)
                    orderby x.label
                    select x).ToHashSet<T>();
        }
        internal static Color GetTColor<T>(this T def, ThingDef stuff = null)
        {
            bool flag = typeof(T) == typeof(GeneDef);
            Color result;
            if (flag)
            {
                result = (def as GeneDef).IconColor;
            }
            else
            {
                bool flag2 = typeof(T) == typeof(XenotypeDef);
                if (flag2)
                {
                    result = RimWorld.XenotypeDef.IconColor;
                }
                else
                {
                    bool flag3 = typeof(T) == typeof(ThingDef);
                    if (flag3)
                    {
                        ThingDef thingDef = def as ThingDef;
                        bool flag4 = stuff != null;
                        if (flag4)
                        {
                            result = thingDef.GetColor(stuff);
                        }
                        else
                        {
                            result = thingDef.uiIconColor;
                        }
                    }
                    else
                    {
                        bool flag5 = typeof(T) == typeof(Selected);
                        if (flag5)
                        {
                            Selected selected = def as Selected;
                            result = selected.thingDef.GetTColor(selected.stuff);
                        }
                        else
                        {
                            bool flag6 = typeof(T) == typeof(CustomXenotype);
                            if (flag6)
                            {
                                CustomXenotype customXenotype = def as CustomXenotype;
                                result = (customXenotype.inheritable ? RimWorld.XenotypeDef.IconColor : new Color(0.58f, 0.757f, 0.91f));
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
        internal static string STooltip<T>(this T def)
        {
            bool flag = def == null;
            string result;
            if (flag)
            {
                result = "";
            }
            else
            {
                string text = "";
                bool flag2 = typeof(T) == typeof(StatDef);
                if (flag2)
                {
                    StatDef statDef = def as StatDef;
                    text = ((statDef.label != null) ? statDef.label.CapitalizeFirst() : "");
                    text += "\n";
                    text += ((statDef.category != null && statDef.category.label != null) ? statDef.category.label.Colorize(Color.yellow) : "");
                    text += ((statDef.category != null && statDef.category.defName != null) ? (" [" + statDef.category.defName + "]").Colorize(Color.gray) : "");
                    text += "\n\n";
                    text += (statDef.description.NullOrEmpty() ? "" : statDef.description);
                }
                else
                {
                    bool flag3 = typeof(T) == typeof(GeneDef);
                    if (flag3)
                    {
                        GeneDef geneDef = def as GeneDef;
                        text += geneDef.DescriptionFull;
                        text += "\n\n";
                        text += geneDef.Modname().Colorize(Color.gray);
                    }
                    else
                    {
                        bool flag4 = typeof(T) == typeof(Gene);
                        if (flag4)
                        {
                            Gene gene = def as Gene;
                            text += gene.def.DescriptionFull;
                            text += "\n\n";
                            text += gene.def.Modname().Colorize(Color.gray);
                        }
                        else
                        {
                            bool flag5 = typeof(T) == typeof(HediffDef);
                            if (flag5)
                            {
                                HediffDef hediffDef = def as HediffDef;
                                text += hediffDef.Description;
                                text += "\n\n";
                                text += hediffDef.Modname().Colorize(Color.gray);
                            }
                            else
                            {
                                bool flag6 = typeof(T) == typeof(XenotypeDef);
                                if (flag6)
                                {
                                    XenotypeDef xenotypeDef = def as XenotypeDef;
                                    text += (xenotypeDef.descriptionShort ?? xenotypeDef.description);
                                    text += "\n\n";
                                    text += xenotypeDef.Modname().Colorize(Color.gray);
                                }
                                else
                                {
                                    bool flag8 = typeof(T) == typeof(Ability);
                                    if (flag8)
                                    {
                                        Ability ability = def as Ability;
                                        text += ability.def.GetTooltip(null);
                                        text += "\n\n";
                                        text += ability.def.Modname().Colorize(Color.gray);
                                    }
                                    else
                                    {
                                        bool flag9 = typeof(T) == typeof(AbilityDef);
                                        if (flag9)
                                        {
                                            AbilityDef abilityDef = def as AbilityDef;
                                            text += abilityDef.GetTooltip(null);
                                            text += "\n\n";
                                            text += abilityDef.Modname().Colorize(Color.gray);
                                        }
                                        else
                                        {
                                            bool flag10 = typeof(T) == typeof(DamageDef);
                                            if (flag10)
                                            {
                                                DamageDef damageDef = def as DamageDef;
                                                text += (damageDef.description.NullOrEmpty() ? "" : (damageDef.description + "\n\n"));
                                                text = text + damageDef.SDefname() + "\n";
                                                text += damageDef.Modname().Colorize(Color.gray);
                                            }
                                            else
                                            {
                                                bool flag11 = typeof(T) == typeof(ResearchProjectDef);
                                                if (flag11)
                                                {
                                                    ResearchProjectDef researchProjectDef = def as ResearchProjectDef;
                                                    text = researchProjectDef.GetTip();
                                                }
                                                else
                                                {
                                                    bool flag12 = DefTool.IsDef<T>();
                                                    if (flag12)
                                                    {
                                                        Def def2 = def as Def;
                                                        text += (def2.description.NullOrEmpty() ? "" : (def2.description + "\n\n"));
                                                        text += def2.Modname().Colorize(Color.gray);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                result = text;
            }
            return result;
        }
        internal static string Modname(this Def t)
        {
            return (t != null && t.modContentPack != null && t.modContentPack.Name != null) ? t.modContentPack.Name : "";
        }
        internal static bool IsDef<T>()
        {
            return typeof(T) == typeof(Def) || typeof(T).BaseType == typeof(Def) || (typeof(T).BaseType != null && typeof(T).BaseType.BaseType == typeof(Def));
        }
    }
}
