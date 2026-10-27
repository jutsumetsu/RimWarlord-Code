using RimWorld;
using System;
using Electromagnetic.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;

// ============================================================================
// HairTool.cs —— 借自「人物编辑器」(Character Editor) 的头发辅助
// 用途：按模组筛选头发定义、前后切换/随机、读写发色；躯体重塑对话框的「头发」页用它。
// 依赖：DefTool.ListByMod 提供「按来源模组分组」的 Def 列表；改完外观后刷新渲染缓存。
// ============================================================================
namespace Electromagnetic.UI
{
    /// <summary>
    /// 头发定义与发色的读写/选择辅助。静态类，无状态。
    /// </summary>
    /// <remarks>
    /// 成员：
    /// <list type="bullet">
    /// <item><description><c>GetHairColor(p, primary)</c> —— 取当前发色（无数据时白）。</description></item>
    /// <item><description><c>AChooseHairCustom(pawn)</c> —— 弹「按模组分组的头发」浮动选择菜单，选中即设置。</description></item>
    /// <item><description><c>GetHairList(modname)</c> —— 取某模组的头发定义集合，<c>noGraphic</c> 的排到后面。</description></item>
    /// <item><description><c>SetHair(pawn, next, random, modname)</c> —— 在列表中前/后/随机切换。</description></item>
    /// <item><description><c>SetHairColor(p, primary, col)</c> —— 反射写 <c>story.hairColor</c>，再刷新渲染。</description></item>
    /// </list>
    /// </remarks>
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
            UIPrimitives.FloatMenuOnRect<HairDef>(
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
        // hairColor 是 Pawn_StoryTracker 的非公开字段，1.6 没有公开 setter；
        // 这里沿用人物编辑器的做法用反射写入，再刷新渲染。
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
