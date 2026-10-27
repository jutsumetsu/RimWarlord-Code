using Electromagnetic.Core;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

// ============================================================================
// StyleTool.cs —— 借自「人物编辑器」(Character Editor) 的胡须/纹身辅助
// 用途：按模组筛选胡须、面纹/身纹，前后/随机切换；发色之外的「风格」页都用它。
// 关键点：1.6 的 faceTattoo / bodyTattoo 没有公开 setter，这里沿用人物编辑器的做法，
//         用反射（SetMemberValue）写入；胡须的「无胡须」在装了 CAT 类模组时无法直接设，
//         需要临时改 NoBeard 的贴图再设、随后恢复（FixForCATMod_BeardsNotSettableToNoBeard）。
// ============================================================================
namespace Electromagnetic.UI
{
    /// <summary>
    /// 胡须、纹身的读写与候选列表构建。静态类，无状态。
    /// </summary>
    /// <remarks>
    /// 成员：
    /// <list type="bullet">
    /// <item><description><c>GetBeardList / GetFaceTattooList / GetBodyTattooList</c> —— 按来源模组过滤，<c>noGraphic</c> 排后。</description></item>
    /// <item><description><c>AChooseBeardCustom(pawn)</c> / <c>ASelectedBeardModName</c> —— 浮动菜单选择胡须。</description></item>
    /// <item><description><c>SetFaceTattoo / SetBodyTattoo</c> —— 反射写 <c>style</c> 字段，含 (next, random) 重载。</description></item>
    /// <item><description><c>SetBeard</c> —— 直接/切换两版；直接版会处理 CAT 模组的 NoBeard 问题。</description></item>
    /// </list>
    /// </remarks>
    internal static class StyleTool
    {
        internal static void AChooseBeardCustom(Pawn pawn)
        {
            UIPrimitives.FloatMenuOnRect<BeardDef>(StyleTool.GetBeardList(null), (BeardDef s) => s.LabelCap, (BeardDef beardDef) => StyleTool.ASetBeardCustom(pawn, beardDef), null, true);
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
