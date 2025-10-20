using Electromagnetic.Core;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Electromagnetic.UI
{
    internal static class StyleTool
    {
        internal static void AChooseBeardCustom(Pawn pawn)
        {
            Dialog_​BodyRemold.FloatMenuOnRect<BeardDef>(StyleTool.GetBeardList(null), (BeardDef s) => s.LabelCap, (BeardDef beardDef) => StyleTool.ASetBeardCustom(pawn, beardDef), null, true);
        }
        internal static string GetBeardName(this Pawn p)
        {
            return ((p != null && p.style != null) && p.style.beardDef != null) ? p.style.beardDef.LabelCap.ToString() : "";
        }
        internal static HashSet<BeardDef> GetBeardList(string modname)
        {
            return (from b in DefTool.ListByMod<BeardDef>(modname)
                    orderby !b.noGraphic
                    select b).ToHashSet<BeardDef>();
        }
        internal static HashSet<TattooDef> GetFaceTattooList(string modname)
        {
            return (from x in DefTool.ListByMod<TattooDef>(modname, (TattooDef x) => x.tattooType == TattooType.Face)
                    orderby !x.noGraphic
                    select x).ToHashSet<TattooDef>();
        }
        internal static HashSet<TattooDef> GetBodyTattooList(string modname)
        {
            return (from x in DefTool.ListByMod<TattooDef>(modname, (TattooDef x) => x.tattooType == TattooType.Body)
                    orderby !x.noGraphic
                    select x).ToHashSet<TattooDef>();
        }
        internal static void ASelectedBeardModName(string val)
        {
            StyleTool.selectedBeardModName = val;
            StyleTool.lOfBeardDefs = StyleTool.GetBeardList(StyleTool.selectedBeardModName);
        }
        internal static void ASetBeardCustom(Pawn pawn, BeardDef beardDef)
        {
            pawn.SetBeard(beardDef);
        }
        internal static void SetFaceTattoo(this Pawn p, TattooDef t)
        {
            bool flag = (p == null && p.style == null) || t == null;
            if (!flag)
            {
                try
                {
                    p.style.SetMemberValue("faceTattoo", t);
                    p.Drawer.renderer.SetAllGraphicsDirty();
                }
                catch (Exception e)
                {
                    Log.Error(e.ToString());
                }
            }
        }
        internal static bool SetFaceTattoo(this Pawn p, bool next, bool random)
        {
            bool flag = p == null && p.style == null;
            bool result;
            if (flag)
            {
                result = false;
            }
            else
            {
                List<TattooDef> list = StyleTool.GetFaceTattooList(null).ToList<TattooDef>();
                bool flag2 = list.EnumerableNullOrEmpty<TattooDef>();
                if (flag2)
                {
                    result = false;
                }
                else
                {
                    TattooDef faceTattoo = p.style.FaceTattoo;
                    int index = list.IndexOf(faceTattoo);
                    index = list.NextOrPrevIndex(index, next, random);
                    TattooDef t = list[index];
                    p.SetFaceTattoo(t);
                    result = true;
                }
            }
            return result;
        }

        internal static bool SetBodyTattoo(this Pawn p, bool next, bool random)
        {
            bool flag = (p == null && p.style == null);
            bool result;
            if (flag)
            {
                result = false;
            }
            else
            {
                List<TattooDef> list = StyleTool.GetBodyTattooList(null).ToList<TattooDef>();
                bool flag2 = list.EnumerableNullOrEmpty<TattooDef>();
                if (flag2)
                {
                    result = false;
                }
                else
                {
                    TattooDef bodyTattoo = p.style.BodyTattoo;
                    int index = list.IndexOf(bodyTattoo);
                    index = list.NextOrPrevIndex(index, next, random);
                    TattooDef t = list[index];
                    p.SetBodyTattoo(t);
                    result = true;
                }
            }
            return result;
        }

        internal static void SetBodyTattoo(this Pawn p, TattooDef t)
        {
            bool flag = (p == null && p.style == null) || t == null;
            if (!flag)
            {
                try
                {
                    p.style.SetMemberValue("bodyTattoo", t);
                    p.SetDirty();
                }
                catch (Exception e)
                {
                    Log.Error(e.ToString());
                }
            }
        }
        internal static bool SetBeard(this Pawn p, bool next, bool random)
        {
            bool flag = p == null && p.style == null;
            bool result;
            if (flag)
            {
                result = false;
            }
            else
            {
                List<BeardDef> list = StyleTool.GetBeardList(null).ToList<BeardDef>();
                bool flag2 = list.EnumerableNullOrEmpty<BeardDef>();
                if (flag2)
                {
                    result = false;
                }
                else
                {
                    BeardDef beardDef = p.style.beardDef;
                    int index = list.IndexOf(beardDef);
                    index = list.NextOrPrevIndex(index, next, random);
                    BeardDef b = list[index];
                    p.SetBeard(b);
                    result = true;
                }
            }
            return result;
        }
        internal static void SetBeard(this Pawn p, BeardDef b)
        {
            bool flag = (p == null && p.style == null) || b == null;
            if (!flag)
            {
                try
                {
                    bool flag2 = b == BeardDefOf.NoBeard;
                    if (flag2)
                    {
                        StyleTool.FixForCATMod_BeardsNotSettableToNoBeard(p, b);
                    }
                    else
                    {
                        p.style.beardDef = b;
                    }
                    p.Drawer.renderer.SetAllGraphicsDirty();
                }
                catch (Exception e)
                {
                    Log.Error(e.ToString());
                }
            }
        }
        internal static string GetFaceTattooName(this Pawn p)
        {
            return ((p != null && p.style != null) && p.style.FaceTattoo != null) ? p.style.FaceTattoo.LabelCap.ToString() : "";
        }
        internal static string GetBodyTattooName(this Pawn p)
        {
            return ((p != null && p.style != null) && p.style.BodyTattoo != null) ? p.style.BodyTattoo.LabelCap.ToString() : "";
        }
        internal static void FixForCATMod_BeardsNotSettableToNoBeard(Pawn p, BeardDef b)
        {
            BeardDefOf.NoBeard.noGraphic = false;
            BeardDefOf.NoBeard.texPath = "bclear";
            p.style.beardDef = b;
            BeardDefOf.NoBeard.noGraphic = true;
        }
        internal static string selectedBeardModName;
        internal static HashSet<BeardDef> lOfBeardDefs;
    }
}
