using Electromagnetic.GameComponent;
using Electromagnetic.Setting;
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
    public static class DirectorOverlay
    {
        private const float PortraitSize = 300f;
        private const float Pad = 12f;
        private const float NameHeight = 26f;
        private const float PanelWidth = PortraitSize + Pad * 2f;
        private const float TypewriterCharsPerSecond = 30f;

        public static void Draw(GameComponent_RWrdDirector comp)
        {
            if (comp == null || comp.session == null || Find.CurrentMap == null) return;
            DirectorSession s = comp.session;
            if (s.current == null) return;

            GameFont oldFont = Text.Font;
            TextAnchor oldAnchor = Text.Anchor;
            Text.Font = GameFont.Small;

            StorytellerDef current = s.currentSpeaker ?? comp.ResolveSpeaker(s.scene, s.current);
            StorytellerDef primary = comp.PrimaryPersona == null ? null : comp.PrimaryPersona.def;
            StorytellerDef secondary = comp.SecondaryPersona == null ? null : comp.SecondaryPersona.def;
            Rect activePanel;

            if (s.HasTwoSpeakers)
            {
                Rect leftSlot = Quadrant(false);
                Rect rightSlot = Quadrant(true);
                bool leftActive = current == secondary;
                bool rightActive = current == primary;
                Rect leftPanel = PanelAt(leftSlot, PanelHeight(leftActive ? s.current : null));
                Rect rightPanel = PanelAt(rightSlot, PanelHeight(rightActive ? s.current : null));

                DrawSpeaker(leftPanel, comp.GetPersona(secondary), s.GetEmotion(secondary),
                    leftActive, leftActive ? s.current : null,
                    leftActive ? ShownText(s, s.current) : null, true);
                DrawSpeaker(rightPanel, comp.GetPersona(primary), s.GetEmotion(primary),
                    rightActive, rightActive ? s.current : null,
                    rightActive ? ShownText(s, s.current) : null, false);

                activePanel = leftActive ? leftPanel : rightPanel;
            }
            else
            {
                Rect panel = PanelAt(Quadrant(true), PanelHeight(s.current));
                DrawSpeaker(panel, comp.GetPersona(current), s.GetEmotion(current),
                    true, s.current, ShownText(s, s.current), false);
                activePanel = panel;
            }

            HandleChoices(comp, s, activePanel);
            Text.Font = oldFont;
            Text.Anchor = oldAnchor;
        }

        // 以立绘中心对齐屏幕四分之一区域的中心；面板整体可以不对称
        private static Rect Quadrant(bool right)
        {
            Rect screen = new Rect(0f, 0f, Verse.UI.screenWidth, Verse.UI.screenHeight);
            Rect top = screen.TakeTopPart(screen.height / 2f);
            return right ? top.TakeRightPart(top.width / 2f) : top.TakeLeftPart(top.width / 2f);
        }

        private static Rect PanelAt(Rect quadrant, float height)
        {
            return new Rect(
                quadrant.center.x - Pad - PortraitSize / 2f,
                Mathf.Max(12f, quadrant.center.y - Pad - PortraitSize / 2f),
                PanelWidth, height);
        }

        private static float PanelHeight(DirectorBeat beat)
        {
            if (beat == null) return Pad + PortraitSize + Pad;
            float textWidth = Mathf.Max(80f, PanelWidth - Pad * 2f);
            float textHeight = Text.CalcHeight(TextOf(beat), textWidth);
            return Pad + PortraitSize + Pad + NameHeight + textHeight + Pad;
        }

        private static void DrawSpeaker(Rect panel, Storyteller persona, DirectorEmotion emotion,
        bool active, DirectorBeat beat, string text, bool flip)
        {
            if (persona == null) return;
            Widgets.DrawBoxSolid(panel, new Color(0.07f, 0.07f, 0.09f, active ? 0.92f : 0.70f));
            if (active) Widgets.DrawBox(panel, 2);

            GUI.color = active ? Color.white : new Color(1f, 1f, 1f, 0.55f);
            Rect portrait = new Rect(panel.x + Pad, panel.y + Pad, PortraitSize, PortraitSize);
            Texture2D art = DirectorPersonaArt.Get(persona.def, emotion);
            DrawPortrait(portrait, art, flip);

            if (active && beat != null)
            {
                Rect dialog = new Rect(panel.x + Pad, portrait.yMax + Pad,
                    panel.width - Pad * 2f,
                    panel.height - PortraitSize - Pad * 3f);
                Widgets.DrawBoxSolid(dialog, new Color(0.12f, 0.12f, 0.15f, 0.92f));
                Widgets.DrawBox(dialog, 1);

                Widgets.Label(new Rect(dialog.x + 8f, dialog.y + 2f,
                    dialog.width - 16f, NameHeight), persona.def.LabelCap);

                Rect body = new Rect(dialog.x + 8f, dialog.y + NameHeight,
                    dialog.width - 16f, dialog.height - NameHeight);
                Widgets.Label(body, text ?? "");
            }
            GUI.color = Color.white;
        }

        // 副叙述者在双人场景里水平翻转；只翻立绘图，不翻对话框
        private static void DrawPortrait(Rect portrait, Texture2D art, bool flip)
        {
            if (art == null) return;
            if (!flip)
            {
                GUI.DrawTexture(portrait, art, ScaleMode.ScaleToFit);
                return;
            }
            Matrix4x4 old = GUI.matrix;
            GUIUtility.ScaleAroundPivot(new Vector2(-1f, 1f), portrait.center);
            GUI.DrawTexture(portrait, art, ScaleMode.ScaleToFit);
            GUI.matrix = old;
        }

        // 打字机只截显示文本；布局高度始终用全文，避免框随打字抖动
        private static string ShownText(DirectorSession s, DirectorBeat beat)
        {
            string full = TextOf(beat);
            if (!RWrdSettings.DirectorTypewriter || s == null || full.Length == 0) return full;
            float elapsed = Mathf.Max(0f, Time.realtimeSinceStartup - s.beatStartedAt);
            int count = Mathf.Clamp(Mathf.FloorToInt(elapsed * TypewriterCharsPerSecond), 0, full.Length);
            return full.Substring(0, count);
        }

        private static void HandleChoices(GameComponent_RWrdDirector comp, DirectorSession s, Rect anchor)
        {
            if (s.state != DirectorSessionState.Choosing || s.current == null) return;
            if (s.current.choices == null || s.current.choices.Count == 0) return;

            float now = Time.realtimeSinceStartup;
            if (now < s.choicesAt) return;
            if (now >= s.choiceDeadline)
            {
                s.Timeout(comp, now);
                return;
            }

            float width = anchor.width;
            float inner = width - 24f;
            float total = 12f;
            for (int i = 0; i < s.current.choices.Count; i++)
            {
                total += ChoiceHeight(s.current.choices[i].labelKey.TranslateSimple(), inner) + 8f;
            }

            float y = anchor.yMax + 8f;
            if (y + total > Verse.UI.screenHeight - 24f)
            {
                y = Mathf.Max(24f, anchor.y - total - 8f);
            }
            Rect box = new Rect(anchor.x, y, width, total);
            Widgets.DrawBoxSolid(box, new Color(0.08f, 0.08f, 0.10f, 0.88f));

            float yy = box.y + 8f;
            for (int i = 0; i < s.current.choices.Count; i++)
            {
                DirectorChoice choice = s.current.choices[i];
                string label = choice.labelKey.TranslateSimple();
                float h = ChoiceHeight(label, inner);
                if (DrawWrappedButton(new Rect(box.x + 12f, yy, inner, h), label))
                {
                    s.Choose(i, comp, now);
                    return;
                }
                yy += h + 8f;
            }
        }

        private static float ChoiceHeight(string label, float width)
        {
            return Mathf.Max(34f, Text.CalcHeight(label, width - 16f) + 16f);
        }

        private static bool DrawWrappedButton(Rect rect, string label)
        {
            bool over = Mouse.IsOver(rect);
            Widgets.DrawBoxSolid(rect, over
                ? new Color(0.25f, 0.25f, 0.30f, 0.95f)
                : new Color(0.12f, 0.12f, 0.15f, 0.95f));
            Widgets.DrawBox(rect, 1);
            TextAnchor old = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect.ContractedBy(8f), label);
            Text.Anchor = old;
            return Widgets.ButtonInvisible(rect);
        }

        private static string TextOf(DirectorBeat beat)
        {
            return beat == null || string.IsNullOrEmpty(beat.textKey)
                ? "" : beat.textKey.TranslateSimple();
        }
    }
}
