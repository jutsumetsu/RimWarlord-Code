using RimWorld;
using System;
using Electromagnetic.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;

namespace Electromagnetic.UI
{
    internal static class HairTool
    {
        internal static bool onMouseover = false;
        internal static Color GetHairColor(this Pawn p, bool primary)
        {
            return (p != null && p.story != null) ? p.story.HairColor : Color.white;
        }
        internal static void AChooseHairCustom(Pawn pawn)
        {
            // 使用闭包捕获传入的pawn
            Dialog_BodyRemold.FloatMenuOnRect<HairDef>(
                HairTool.GetHairList(null),
                (HairDef s) => s.LabelCap,
                (HairDef hairDef) => HairTool.ASetHairCustom(pawn, hairDef), // 传递pawn
                null,
                true
            );
        }
        internal static HashSet<HairDef> GetHairList(string modname)
        {
            return (from hr in DefTool.ListByMod<HairDef>(modname)
                    orderby !hr.noGraphic
                    select hr).ToHashSet<HairDef>();
        }
        internal static void ASelectedHairModName(string val)
        {
            HairTool.selectedHairModName = val;
            HairTool.lOfHairDefs = HairTool.GetHairList(HairTool.selectedHairModName);
        }
        internal static void ASetHairCustom(Pawn pawn, HairDef hairDef)
        {
            // 使用传入的pawn而非静态引用
            pawn.SetHair(hairDef);
        }
        internal static void SetHair(this Pawn pawn, bool next, bool random, string modname = null)
        {
            bool flag = pawn == null || pawn.story == null;
            if (!flag)
            {
                List<HairDef> list = DefTool.ListByMod<HairDef>(modname).ToList<HairDef>();
                int index = list.IndexOf(pawn.story.hairDef);
                index = list.NextOrPrevIndex(index, next, random);
                pawn.SetHair(list[index]);
            }
        }
        internal static void SetHair(this Pawn p, HairDef h)
        {
            bool flag = (p == null && p.story == null) || h == null;
            if (!flag)
            {
                p.story.hairDef = h;
                p.Drawer.renderer.SetAllGraphicsDirty();
            }
        }
        internal static string GetHairName(this Pawn p)
        {
            return ((p != null && p.story != null) && p.story.hairDef != null) ? p.story.hairDef.LabelCap.ToString() : "";
        }
        internal static void SetHairColor(this Pawn p, bool primary, Color col)
        {
            bool flag = p == null && p.story == null;
            if (!flag)
            {
                try
                {
                    p.story.SetMemberValue("hairColor", col);
                    p.Drawer.renderer.SetAllGraphicsDirty();
                }
                catch (Exception e)
                {
                    Log.Error(e.ToString());
                }
            }
        }
        internal static string selectedHairModName = null;
        internal static HashSet<HairDef> lOfHairDefs = null;
    }
}
