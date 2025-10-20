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
    internal static class BodyTool
    {
        internal static string GetBodyTypeName(this Pawn pawn)
        {
            TaggedString? taggedString;
            if (pawn == null)
            {
                taggedString = null;
            }
            else
            {
                Pawn_StoryTracker story = pawn.story;
                if (story == null)
                {
                    taggedString = null;
                }
                else
                {
                    BodyTypeDef bodyType = story.bodyType;
                    taggedString = ((bodyType != null) ? new TaggedString?(bodyType.defName.Translate()) : null);
                }
            }
            TaggedString? taggedString2 = taggedString;
            string text = (taggedString2 != null) ? taggedString2.GetValueOrDefault() : null;
            return text ?? "";
        }
        internal static List<BodyTypeDef> GetBodyDefList(this Pawn pawn, bool restricted = false)
        {
            return pawn.GetBodyList();
        }

        internal static void SetBody(this Pawn p, bool next, bool random)
        {
            bool flag = p == null || p.story == null;
            if (!flag)
            {
                List<BodyTypeDef> bodyDefList = p.GetBodyDefList(false);
                int index = bodyDefList.IndexOf(p.story.bodyType);
                index = bodyDefList.NextOrPrevIndex(index, next, random);
                bool flag2 = !bodyDefList.NullOrEmpty<BodyTypeDef>();
                if (flag2)
                {
                    p.SetBody(bodyDefList[index]);
                }
            }
        }
        internal static void SetBody(this Pawn p, BodyTypeDef b)
        {
            bool flag = (p == null || p.story == null) || b == null;
            if (!flag)
            {
                p.story.bodyType = b;
                p.SetDirty();
            }
        }
        internal static List<BodyTypeDef> GetBodyList(this Pawn pawn)
        {
            bool flag = pawn == null || pawn.story == null;
            List<BodyTypeDef> result;
            if (flag)
            {
                result = null;
            }
            else
            {
                BodyTypeDef bodyType = pawn.story.bodyType;
                string a;
                if (bodyType == null)
                {
                    a = null;
                }
                else
                {
                    ModContentPack modContentPack = bodyType.modContentPack;
                    a = ((modContentPack != null) ? modContentPack.Name : null);
                }
                bool flag2 = a == "Alien Vs Predator";
                List<BodyTypeDef> list = DefTool.ListAll<BodyTypeDef>();
                List<BodyTypeDef> list2 = new List<BodyTypeDef>();
                foreach (BodyTypeDef bodyTypeDef in list)
                {
                    try
                    {
                        string text = pawn.Drawer.renderer.BodyGraphic.path;
                        bool flag3 = text.Contains("/Naked");
                        if (flag3)
                        {
                            text = text.SubstringTo("/Naked_", false) + bodyTypeDef.defName + "_south";
                        }
                        else
                        {
                            bool flag4 = text.Contains("_Naked_");
                            if (flag4)
                            {
                                text = text.SubstringTo("_Naked_", false) + bodyTypeDef.defName + "_south";
                            }
                            else
                            {
                                bool flag5 = text.Contains("Naked_");
                                if (flag5)
                                {
                                    text = text.SubstringBackwardTo("Naked_", true) + "Naked_" + bodyTypeDef.defName + "_south";
                                }
                            }
                        }
                        bool flag6 = TextureTool.TestTexturePath(text, false);
                        bool flag7 = flag2;
                        if (flag7)
                        {
                            flag6 = (pawn.kindDef.modContentPack == bodyTypeDef.modContentPack && flag6);
                        }
                        bool flag8 = flag6;
                        if (flag8)
                        {
                            list2.Add(bodyTypeDef);
                        }
                    }
                    catch
                    {
                    }
                }
                result = list2;
            }
            return result;
        }
    }
}
