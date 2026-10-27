using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Electromagnetic.Core;
using Verse;

// ============================================================================
// HeadTool.cs —— 借自「人物编辑器」(Character Editor) 的头部工具
// 用途：躯体重塑对话框的「头部」页：列出该种族/性别实际有贴图的头型、前后/随机切换。
// 说明：头型名是从贴图路径里「猜」出来的（♀/♂、Average/Narrow/Wide/Pointy/Normal），
//       这是人物编辑器的启发式做法，路径不规则时会退化到末段子串。
// ============================================================================
namespace Electromagnetic.UI
{
    /// <summary>
    /// 头部类型（HeadTypeDef）的候选列表、名称与切换。静态类，无状态。
    /// </summary>
    /// <remarks>
    /// 成员：
    /// <list type="bullet">
    /// <item><description><c>GetHeadName(p, path)</c> —— 从 graphicPath 猜「性别符 + 头型名」。</description></item>
    /// <item><description><c>GetHeadDefList(p, genderized)</c> —— 用头部贴图目录过滤 HeadTypeDef，优先「实测贴图存在」的集合。</description></item>
    /// <item><description><c>GetTestedHeadList(l, skipSkull)</c> —— 逐个验贴图，可跳过 Skull/Stump。</description></item>
    /// <item><description><c>TestHead(headPath)</c> —— 只测 <c>&lt;path&gt;_south</c> 是否存在（TextureTool.TestTexturePath）。</description></item>
    /// <item><description><c>SetHeadTypeDef(def)</c> —— 设置并刷新渲染。</description></item>
    /// <item><description><c>SetHead(p, next, random)</c> —— 在候选列表里前/后/随机切换。</description></item>
    /// <item><description><c>GetHeadPathFolderFor</c> —— 由当前头部路径推出「性别化 / 非性别化」目录（标准 Heads 目录与 /Male、/Female 两种布局）。</description></item>
    /// </list>
    /// </remarks>
    internal static class HeadTool
    {
        internal static string GetHeadName(this Pawn pawn, string path = null)
        {
            string text = "";
            bool flag = pawn == null;
            string result;
            if (flag)
            {
                result = "";
            }
            else
            {
                string text2;
                if ((text2 = path) == null)
                {
                    if (pawn == null)
                    {
                        text2 = null;
                    }
                    else
                    {
                        Pawn_StoryTracker story = pawn.story;
                        text2 = ((story != null) ? story.headType.graphicPath : null);
                    }
                }
                path = text2;
                path = (path ?? "");
                bool flag2 = path.Contains("Female");
                if (flag2)
                {
                    text += "♀ ";
                }
                else
                {
                    bool flag3 = path.Contains("Male");
                    if (flag3)
                    {
                        text += "♂ ";
                    }
                }
                bool flag4 = path.Contains("Average");
                if (flag4)
                {
                    text += "Average";
                }
                else
                {
                    bool flag5 = path.Contains("Narrow");
                    if (flag5)
                    {
                        text += "Narrow";
                    }
                }
                bool flag6 = path.Contains("Wide");
                if (flag6)
                {
                    text += " Wide";
                }
                else
                {
                    bool flag7 = path.Contains("Pointy");
                    if (flag7)
                    {
                        text += " Pointy";
                    }
                    else
                    {
                        bool flag8 = path.Contains("Normal");
                        if (flag8)
                        {
                            text += " Normal";
                        }
                        else
                        {
                            text += ((path.EndsWith("Average") || path.EndsWith("Narrow")) ? "" : path.SubstringBackwardFrom("_", false));
                        }
                    }
                }
                bool flag9 = text.Contains("/");
                if (flag9)
                {
                    text = text.SubstringBackwardFrom("/", true);
                }
                result = text;
            }
            return result;
        }
        internal static HashSet<HeadTypeDef> GetHeadDefList(this Pawn pawn, bool genderized = false)
        {
            bool flag = pawn == null && pawn.story == null;
            HashSet<HeadTypeDef> result;
            if (flag)
            {
                result = null;
            }
            else
            {
                string pathG;
                string pathNG;
                pawn.GetHeadPathFolderFor(out pathG, out pathNG);
                HashSet<HeadTypeDef> hashSet2 = DefTool.ListBy<HeadTypeDef>((HeadTypeDef x) => x.graphicPath.Contains(pathG));
                HashSet<HeadTypeDef> hashSet3 = DefTool.ListBy<HeadTypeDef>((HeadTypeDef x) => x.graphicPath.Contains(pathNG));
                HashSet<HeadTypeDef> testedHeadList2 = HeadTool.GetTestedHeadList(pawn, hashSet2, true);
                HashSet<HeadTypeDef> testedHeadList3 = HeadTool.GetTestedHeadList(pawn, hashSet3, true);
                bool flag6 = testedHeadList3.Count > 0;
                if (flag6)
                {
                    result = testedHeadList3;
                }
                else
                {
                    bool flag7 = testedHeadList2.Count > 0;
                    if (flag7)
                    {
                        result = testedHeadList2;
                    }
                    else
                    {
                        bool flag8 = hashSet2.Count > 0;
                        if (flag8)
                        {
                            result = hashSet2;
                        }
                        else
                        {
                            bool flag9 = hashSet3.Count > 0;
                            if (flag9)
                            {
                                result = hashSet3;
                            }
                            else
                            {
                                result = DefTool.ListBy<HeadTypeDef>((HeadTypeDef x) => x.modContentPack == pawn.kindDef.modContentPack);
                            }
                        }
                    }
                }
            }
            return result;
        }
        internal static HashSet<HeadTypeDef> GetTestedHeadList(Pawn pawn, HashSet<HeadTypeDef> l, bool skipSkull = false)
        {
            bool flag = (pawn == null && pawn.story == null) || l.NullOrEmpty<HeadTypeDef>();
            HashSet<HeadTypeDef> result;
            if (flag)
            {
                result = new HashSet<HeadTypeDef>();
            }
            else
            {
                HashSet<HeadTypeDef> hashSet = new HashSet<HeadTypeDef>();
                foreach (HeadTypeDef headTypeDef in l)
                {
                    bool flag2 = pawn.TestHead(headTypeDef.graphicPath);
                    if (flag2)
                    {
                        bool flag3 = skipSkull && (headTypeDef.graphicPath.Contains("Skull") || headTypeDef.graphicPath.Contains("Stump"));
                        if (!flag3)
                        {
                            hashSet.Add(headTypeDef);
                        }
                    }
                }
                result = hashSet;
            }
            return result;
        }
        internal static bool TestHead(this Pawn pawn, string headPath)
        {
            bool flag = pawn == null || headPath.NullOrEmpty();
            bool result;
            if (flag)
            {
                result = false;
            }
            else
            {
                string text = headPath.Replace("\\", "/");
                bool flag2 = TextureTool.TestTexturePath(text + "_south", false);
                bool flag3 = !flag2;
                if (flag3)
                {
                    result = false;
                }
                else
                {
                    result = flag2;
                }
            }
            return result;
        }
        internal static void SetHeadTypeDef(this Pawn p, HeadTypeDef def)
        {
            bool flag = (p == null && p.story == null) || def == null;
            if (!flag)
            {
                p.story.headType = def;
                p.SetDirty();
            }
        }
        internal static bool SetHead(this Pawn pawn, bool next, bool random)
        {
            bool flag = pawn == null;
            bool result;
            if (flag)
            {
                result = false;
            }
            else
            {
                bool flag2 = pawn.story == null;
                if (flag2)
                {
                    result = true;
                }
                else
                {
                    HashSet<HeadTypeDef> headDefList = pawn.GetHeadDefList(false);
                    HeadTypeDef headType = pawn.story.headType;
                    int index = headDefList.IndexOf(headType);
                    index = headDefList.NextOrPrevIndex(index, next, random);
                    HeadTypeDef headTypeDef = headDefList.ElementAt(index);
                    pawn.SetHeadTypeDef(headTypeDef);
                    result = true;
                }
            }
            return result;
        }
        internal static void GetHeadPathFolderFor(this Pawn pawn, out string pathGenderized, out string pathNonGenderized)
        {
            pathGenderized = "";
            pathNonGenderized = "";
            bool flag = (pawn == null && pawn.story == null) || pawn.story.headType == null;
            if (!flag)
            {
                string text = pawn.story.headType.graphicPath;
                text = (text ?? "");
                bool devMode = Prefs.DevMode;
                if (devMode)
                {
                    Log.Message("head graphic path=" + text);
                }
                text = text.SubstringBackwardTo("/", true);
                bool devMode2 = Prefs.DevMode;
                if (devMode2)
                {
                    Log.Message("heads path=" + text);
                }
                Gender gender = pawn.gender;
                bool flag2 = text.StartsWith("Things/Pawn/Humanlike/Heads");
                if (flag2)
                {
                    pathNonGenderized = "Things/Pawn/Humanlike/Heads/";
                    bool flag3 = gender == Gender.Female;
                    if (flag3)
                    {
                        pathGenderized = text.Replace("Male", "Female");
                    }
                    else
                    {
                        bool flag4 = gender == Gender.Male;
                        if (flag4)
                        {
                            pathGenderized = text.Replace("Female", "Male");
                        }
                    }
                }
                else
                {
                    bool flag5 = text.Contains("/Male") || text.Contains("/Female");
                    if (flag5)
                    {
                        pathNonGenderized = (text.Contains("/Male") ? text.SubstringBackwardTo("/Male", true) : text.SubstringBackwardTo("/Female", true));
                        bool flag6 = gender == Gender.Female;
                        if (flag6)
                        {
                            pathGenderized = text.Replace("/Male", "/Female");
                        }
                        else
                        {
                            bool flag7 = gender == Gender.Male;
                            if (flag7)
                            {
                                pathGenderized = text.Replace("/Female", "/Male");
                            }
                        }
                    }
                }
                bool devMode3 = Prefs.DevMode;
                if (devMode3)
                {
                    Log.Message("genderized  subpath=" + pathGenderized);
                    Log.Message("non-genderized path=" + pathNonGenderized);
                }
            }
        }
    }
}
