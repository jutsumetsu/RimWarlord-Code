using Electromagnetic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Electromagnetic.UI
{
    public static class UIPrimitives
    {
        public static void ButtonText(Rect rect, string label, Action action, string toolTip = "")
        {
            bool flag = Widgets.ButtonText(rect, label, true, true, true, null);
            if (flag)
            {
                bool flag2 = action != null;
                if (flag2)
                {
                    action();
                }
            }
            bool flag3 = !toolTip.NullOrEmpty();
            if (flag3)
            {
                TooltipHandler.TipRegion(rect, toolTip);
            }
        }
        public static List<FloatMenuOption> FloatMenuOnRect<T>(ICollection<T> l, Func<T, string> labelGetter, Action<T> action, Selected s = null, bool doWindow = true)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            bool flag = !l.EnumerableNullOrEmpty<T>();
            if (flag)
            {
                using (IEnumerator<T> enumerator = l.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        T element = enumerator.Current;
                        string label = labelGetter(element);
                        FloatMenuOption floatMenuOption = new FloatMenuOption(label, delegate ()
                        {
                            bool flag3 = action != null;
                            if (flag3)
                            {
                                action(element);
                            }
                        }, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                        bool flag2 = element != null;
                        if (flag2)
                        {
                            floatMenuOption.SetFMOIcon(element.GetTIcon(s));
                            floatMenuOption.iconColor = element.GetTColor(null);
                            floatMenuOption.tooltip = new TipSignal?(element.STooltip<T>());
                        }
                        list.Add(floatMenuOption);
                    }
                }
                if (doWindow)
                {
                    WindowTool.Open(new FloatMenu(list));
                }
            }
            return list;
        }
        public static Texture2D IconForStyleCustom(Selected s, ThingStyleDef style)
        {
            return (s != null && s.thingDef != null && s.stuff != null && style != null) ? Widgets.GetIconFor(s.thingDef, s.stuff, style, null) : null;
        }
        public static void Image(Rect rect, string texPath)
        {
            GUI.DrawTexture(rect, ContentFinder<Texture2D>.Get(texPath, true));
        }
        public static void ButtonHighlight(Rect rect, string texPath, Action<Color> action, Color color, string toolTip = "")
        {
            bool flag = texPath.NullOrEmpty();
            if (!flag)
            {
                bool flag2 = Mouse.IsOver(rect);
                if (flag2)
                {
                    Widgets.DrawBoxSolid(rect, new Color(color.r, color.g, color.b, 0.4f));
                }
                bool flag3 = Widgets.ButtonImage(rect, ContentFinder<Texture2D>.Get(texPath, true), color, color, true, null);
                if (flag3)
                {
                    bool flag4 = action != null;
                    if (flag4)
                    {
                        action(color);
                    }
                }
                bool flag5 = !toolTip.NullOrEmpty();
                if (flag5)
                {
                    TooltipHandler.TipRegion(rect, toolTip);
                }
            }
        }
        public static void ButtonTextureTextHighlight(Rect rect, string text, Texture2D icon, Color color, Action action, string toolTip = "")
        {
            bool flag = Mouse.IsOver(rect);
            if (flag)
            {
                Widgets.DrawHighlight(rect);
            }
            GUI.color = color;
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.height, rect.height), icon);
            GUI.color = Color.white;
            Text.Font = GameFont.Small;
            float num = rect.height - 10f - rect.height / 2f;
            Widgets.Label(new Rect(rect.x + rect.height + 5f, rect.y + num, rect.width - rect.height - 5f, rect.height), text);
            UIPrimitives.ButtonInvisible(rect, action, toolTip);
        }
        public static void ButtonTextureTextHighlight2(Rect rect, string text, string texPath, Color color, Action action, string toolTip = "", bool withButton = true, float textOffset = 10f)
        {
            bool flag = Mouse.IsOver(rect);
            if (flag)
            {
                Widgets.DrawHighlight(rect);
            }
            bool flag2 = texPath != null;
            if (flag2)
            {
                GUI.color = color;
                GUI.DrawTexture(new Rect(rect.x, rect.y, rect.height, rect.height), ContentFinder<Texture2D>.Get(texPath, true));
                GUI.color = Color.white;
            }
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;
            float num = rect.height - textOffset - rect.height / 2f;
            bool flag3 = texPath == null;
            if (flag3)
            {
                Widgets.Label(rect, text);
            }
            else
            {
                Widgets.Label(new Rect(rect.x + rect.height + 5f, rect.y + num, rect.width - rect.height - 5f, rect.height), text);
            }
            Text.Anchor = TextAnchor.UpperLeft;
            if (withButton)
            {
                ButtonInvisible(rect, action, toolTip);
            }
        }
        public static void ButtonImageCol(float x, float y, float w, float h, string texPath, Action<Color> action, Color color, string toolTip = "")
        {
            bool flag = texPath.NullOrEmpty();
            if (!flag)
            {
                Rect rect = new Rect(x, y, w, h);
                bool flag2 = Widgets.ButtonImage(rect, ContentFinder<Texture2D>.Get(texPath, true), color, true, null);
                if (flag2)
                {
                    bool flag3 = action != null;
                    if (flag3)
                    {
                        action(color);
                    }
                }
                bool flag4 = !toolTip.NullOrEmpty();
                if (flag4)
                {
                    TooltipHandler.TipRegion(rect, toolTip);
                }
            }
        }
        public static void ButtonInvisible(Rect rect, Action action, string toolTip = "")
        {
            bool flag = Widgets.ButtonInvisible(rect, true);
            if (flag)
            {
                bool flag2 = action != null;
                if (flag2)
                {
                    action();
                }
            }
            bool flag3 = !toolTip.NullOrEmpty();
            if (flag3)
            {
                TooltipHandler.TipRegion(rect, toolTip);
            }
        }
        public static void NavSelectorImageBox(Rect rect, Action onClicked, Action onPrev, Action onNext, Action onTextureClick, string label, string tipLabel = null, string tipTexture = null, string texturePath = null, Color colTex = default(Color))
        {
            bool flag = onPrev != null;
            if (flag)
            {
                ButtonImage(RectPrevious(rect), "bbackward", onPrev, "");
            }
            bool flag2 = onNext != null;
            if (flag2)
            {
                ButtonImage(RectNext(rect), "bforward", onNext, "");
            }
            Text.Font = GameFont.Small;
            bool flag3 = texturePath != null;
            LabelBackground(RectSolid(rect, true), label, new Color(0.115f, 0.115f, 0.115f), flag3 ? 25 : 0, "", colTex);
            ButtonImage(RectTexture(rect), texturePath, onTextureClick, tipTexture);
            int num = 0;
            ButtonInvisibleMouseOver(RectOnClick(rect, flag3, num, true), onClicked, tipLabel);
        }
        public static void LabelBackground(Rect rect, string text, Color col, int offset = 0, string tooltip = "", Color colText = default(Color))
        {
            Widgets.DrawBoxSolid(rect, col);
            bool flag = text == null;
            if (flag)
            {
                text = "";
            }
            Rect rect2 = new Rect(rect.x + 3f + (float)offset, rect.y, rect.width - 3f, rect.height);
            bool flag2 = rect.height > 20f && text.Length <= 22;
            if (flag2)
            {
                rect2.y += (rect.height - 20f) / 2f;
            }
            bool flag3 = colText != default(Color);
            if (flag3)
            {
                Color color = GUI.color;
                GUI.color = colText;
                Widgets.Label(rect2, text);
                GUI.color = color;
            }
            else
            {
                Widgets.Label(rect2, text);
            }
            bool flag4 = !tooltip.NullOrEmpty();
            if (flag4)
            {
                TooltipHandler.TipRegion(rect2, tooltip);
            }
        }
        public static void ButtonInvisibleMouseOver(Rect rect, Action action, string toolTip = "")
        {
            bool flag = Mouse.IsOver(rect);
            if (flag)
            {
                Widgets.DrawHighlight(rect);
            }
            bool flag2 = Widgets.ButtonInvisible(rect, true);
            if (flag2)
            {
                bool flag3 = action != null;
                if (flag3)
                {
                    action();
                }
            }
            bool flag4 = !toolTip.NullOrEmpty();
            if (flag4)
            {
                TooltipHandler.TipRegion(rect, toolTip);
            }
        }
        public static void ButtonImage(Rect rect, string texPath, Action action, string tooolTip = "")
        {
            bool flag = texPath.NullOrEmpty();
            if (!flag)
            {
                bool flag2 = Widgets.ButtonImage(rect, ContentFinder<Texture2D>.Get(texPath, true), true, null);
                if (flag2)
                {
                    bool flag3 = action != null;
                    if (flag3)
                    {
                        action();
                    }
                }
                bool flag4 = !tooolTip.NullOrEmpty();
                if (flag4)
                {
                    TooltipHandler.TipRegion(rect, tooolTip);
                }
            }
        }
        public static Rect RectSolid(Rect rect, bool showEdit = true)
        {
            return new Rect(rect.x + (float)(showEdit ? 21 : 0), rect.y, rect.width - (float)(showEdit ? 40 : 19), 24f);
        }
        public static Rect RectTexture(Rect rect)
        {
            return new Rect(rect.x + 25f, rect.y, 24f, 24f);
        }
        public static Rect RectPrevious(Rect rect)
        {
            return new Rect(rect.x, rect.y + 2f, 22f, 22f);
        }
        public static Rect RectOnClick(Rect rect, bool hasTexture, int offset = 0, bool showEdit = true)
        {
            return new Rect(rect.x + (float)(showEdit ? 21 : 0) + (float)(hasTexture ? 25 : 0), rect.y, rect.width - (float)(showEdit ? 40 : 19) - (float)(hasTexture ? 25 : 0) - (float)offset, 24f);
        }
        public static Rect RectNext(Rect rect)
        {
            return new Rect(rect.x + rect.width - 22f, rect.y + 2f, 22f, 22f);
        }
        public static Rect RectToggle(Rect rect)
        {
            return new Rect(rect.x + rect.width - 42f, rect.y, 22f, 22f);
        }
    }
}
