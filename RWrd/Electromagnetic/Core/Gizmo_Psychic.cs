using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Electromagnetic.Core
{
    public class Gizmo_Psychic : Gizmo
    {
        public override bool Visible
        {
            get
            {
                bool flag = this.pawn ==null || this.root == null;
                return !flag;
            }
        }
        //UI宽度
        public override float GetWidth(float maxWidth)
        {
            return 176f;
        }
        //获取信息
        public Gizmo_Psychic(Pawn pawn, Hediff_RWrd_PowerRoot energy)
        {
            this.Order = -110f;
            this.pawn = pawn;
            this.root = energy;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new Rect(topLeft.x - 4, topLeft.y, this.GetWidth(maxWidth), 89f);
            Rect rect2 = rect.ContractedBy(2f);
            Widgets.DrawWindowBackground(rect);
            Rect rect3 = rect2;
            //能量显示
            rect3.width = 180f;
            rect3.height = rect.height / 2f;
            this.DrawUI(rect3, this.root.energy.energy, this.root.energy.CurrentDef.MaxEnergy, this.EnergyLabel);
            //等级显示
            Rect rect4 = rect2;
            rect4.width = 180f;
            rect4.yMin = rect2.y + 30;
            this.DrawUI2(rect4, this.root.energy.Exp, this.root.energy.CurrentDef.EXP, this.root.energy.CurrentDef.level, this.root.energy.CurrentDef.label, this.ExpLabel.Translate());
            //完全境界显示
            Rect rect5 = rect2;
            rect5.width = 180f;
            rect5.yMin = rect4.y + 18;
            this.DrawUI3(rect5, this.root.energy.CompleteRealm, this.root.energy.AvailableCompleteRealm(), this.CompleteRealmLabel.Translate());
            //力量流量显示
            Rect rect6 = rect2;
            rect6.width = 180f;
            rect6.yMin = rect5.y + 18;
            this.DrawUI4(rect6, this.root.energy.PowerFlow, this.PowerFlowLabel.Translate());
            float num = this.root.energy.energy / this.root.energy.CurrentDef.MaxEnergy;
            Rect position = new Rect(rect2.x + 125f, rect2.y + 8f, 50f, 60f);
            bool flag = (double)num <= 0.25;
            Text.Anchor = TextAnchor.UpperLeft;
            return new GizmoResult(GizmoState.Clear);
        }
        //绘制能量显示及能量条
        private void DrawUI(Rect rect, float a, float b, string label)
        {
            bool lflag = LanguageDatabase.activeLanguage.ToString() != "Simplified Chinese";
            bool lflag2 = LanguageDatabase.activeLanguage.ToString() != "Traditional Chinese";
            if (lflag && lflag2)
            {
                Text.Font = GameFont.Tiny;
            }
            else
            {
                Text.Font = GameFont.Small;
            }
            //能量值标签位置
            Rect rect2 = new Rect(rect.x, rect.y, 60f, 25f);
            //数值位置
            Rect rect3 = new Rect(rect.x + 60f, rect.y, 100f, 25f);
            //能量条位置
            Rect rect4 = new Rect(rect.x, rect.y + 20f, 172f, 10f);
            Widgets.Label(rect2, label);
            Widgets.Label(rect3, a.ToString("F0") + " / " + b.ToString("F0"));
            Widgets.FillableBar(rect4, a / b, Gizmo_Psychic.FullBarTex, Gizmo_Psychic.EmptyBarTex, false);
        }
        //绘制等级显示
        private void DrawUI2(Rect rect, float a, float b, int c, string label, string label2)
        {
            bool lflag = LanguageDatabase.activeLanguage.ToString() != "Simplified Chinese";
            bool lflag2 = LanguageDatabase.activeLanguage.ToString() != "Traditional Chinese";
            //等级标签位置
            Rect rect2 = new Rect(rect.x, rect.y, 120f, 25f);
            //磁场转动匹数位置
            Rect rect3 = new Rect(rect.x + 85f, rect.y, 40f, 25f);
            //经验值标签位置
            Rect rect4 = new Rect(rect.x + 140f, rect.y, 40f, 25f);
            //电推伏特位置
            Rect rect5 = new Rect(rect.x + 65f, rect.y, 40f, 25f);
            if (lflag && lflag2)
            {
                Text.Font = GameFont.Tiny;
                rect4 = new Rect(rect.x + 150f, rect.y, 40f, 25f);
                Widgets.Label(rect4, label2);
            }
            else
            {
                Text.Font = GameFont.Small;
                Widgets.Label(rect4, label2);
            }
            Widgets.Label(rect2, label);
            float num = a;
            bool flag = num >= b;
            if (flag)
            {
                num = b - 1;
            }
            if (c != 0)
            {
                if (lflag && lflag2)
                {
                    num = a / b;
                    rect3 = new Rect(rect.x + 127f, rect.y, 40f, 25f);
                    Widgets.Label(rect3, num.ToString("P0"));
                }
                else
                {
                    Widgets.Label(rect3, num.ToString("F0"));
                }
            }
            else
            {
                if (lflag && lflag2)
                {
                    num = a / b;
                    rect5 = new Rect(rect.x + 110f, rect.y, 40f, 25f);
                    Widgets.Label(rect5, num.ToString("P2"));
                }
                else
                {
                    Widgets.Label(rect5, num.ToString("F0"));
                }
            }
        }
        //绘制完全境界显示
        private void DrawUI3(Rect rect, float a, int b, string label)
        {
            bool lflag = LanguageDatabase.activeLanguage.ToString() != "Simplified Chinese";
            bool lflag2 = LanguageDatabase.activeLanguage.ToString() != "Traditional Chinese";
            if (lflag && lflag2)
            {
                Text.Font = GameFont.Tiny;
            }
            else
            {
                Text.Font = GameFont.Small;
            }
            //完全境界标签位置
            Rect rect2 = new Rect(rect.x, rect.y, 90f, 25f);
            //百级以内完全境界数值位置
            Rect rect3 = new Rect(rect.x + 90f, rect.y, 40f, 25f);
            //完全境界级数标签位置
            Rect rect4 = new Rect(rect.x + 140f, rect.y, 40f, 25f);
            //最后境界前及十以内完全境界数值位置
            Rect rect5 = new Rect(rect.x + 95f, rect.y, 40f, 25f);
            //最后境界数值位置
            Rect rect6 = new Rect(rect.x + 75f, rect.y, 60f, 25f);
            //千级以内完全境界数值位置
            Rect rect7 = new Rect(rect.x + 85f, rect.y, 40f, 25f);
            //万级以内完全境界数值位置
            Rect rect8 = new Rect(rect.x + 80f, rect.y, 40f, 25f);
            //一万级完全境界数值位置
            Rect rect9 = new Rect(rect.x + 75f, rect.y, 40f, 25f);
            int c = (int)Math.Floor(a);
            bool flag = b >= 10 && b < 20;
            bool flag1 = c >= 2 && c < 10;
            bool flag2 = c >= 10 && c < 100;
            bool flag3 = c >= 100 && c < 1000;
            bool flag4 = c >= 1000 && c < 10000;
            Widgets.Label(rect2, label);
            if (b < 10)
            {
                if (lflag && lflag2)
                {
                    float num = b * 10;
                    Widgets.Label(rect5, num.ToString("F0") + "%");
                }
                else
                {
                    Widgets.Label(rect5, b.ToString("F0"));
                    Widgets.Label(rect4, "成");
                }
            }
            else if (flag)
            {
                if (lflag && lflag2)
                {
                    rect6 = new Rect(rect.x + 100f, rect.y, 80f, 25f);
                    Widgets.Label(rect6, "RWrd_FinalRealm".Translate());
                }
                else
                {
                    Widgets.Label(rect6, "RWrd_FinalRealm".Translate());
                }
            }
            else if (flag1)
            {
                if (lflag && lflag2)
                {
                    Widgets.Label(rect5, "Lv." + c.ToString("F0"));
                }
                else
                {
                    Widgets.Label(rect5, c.ToString("F0"));
                    Widgets.Label(rect4, "级");
                }
            }
            else if (flag2)
            {
                if (lflag && lflag2)
                {
                    Widgets.Label(rect5, "Lv." + c.ToString("F0"));
                }
                else
                {
                    Widgets.Label(rect3, c.ToString("F0"));
                    Widgets.Label(rect4, "级");
                }
            }
            else if (flag3)
            {
                if (lflag && lflag2)
                {
                    Widgets.Label(rect5, "Lv." + c.ToString("F0"));
                }
                else
                {
                    Widgets.Label(rect7, c.ToString("F0"));
                    Widgets.Label(rect4, "级");
                }
            }
            else if (flag4)
            {
                if (lflag && lflag2)
                {
                    Widgets.Label(rect5, "Lv." + c.ToString("F0"));
                }
                else
                {
                    Widgets.Label(rect8, c.ToString("F0"));
                    Widgets.Label(rect4, "级");
                }
            }
            else
            {
                if (lflag && lflag2)
                {
                    Widgets.Label(rect5, "Lv." + c.ToString("F0"));
                }
                else
                {
                    Widgets.Label(rect9, c.ToString("F0"));
                    Widgets.Label(rect4, "级");
                }
            }
        }
        //绘制力量流量显示
        private void DrawUI4(Rect rect, int a, string label)
        {
            bool lflag = LanguageDatabase.activeLanguage.ToString() != "Simplified Chinese";
            bool lflag2 = LanguageDatabase.activeLanguage.ToString() != "Traditional Chinese";
            if (lflag && lflag2)
            {
                Text.Font = GameFont.Tiny;
            }
            else
            {
                Text.Font = GameFont.Small;
            }
            //力量流量标签位置
            Rect rect2 = new Rect(rect.x, rect.y, 60f, 25f);
            //力量流量数值位置
            Rect rect3 = new Rect(rect.x + 65f, rect.y, 70f, 25f);
            //力量流量单位标签位置
            Rect rect4 = new Rect(rect.x + 140f, rect.y, 40f, 25f);
            Widgets.Label(rect2, label);
            Widgets.Label(rect3, a.ToString("F0"));
            Widgets.Label(rect4, "RWrd_KWH".Translate());
        }

        public Pawn pawn;
        public Hediff_RWrd_PowerRoot root;
        public string ExpLabel;
        public string EnergyLabel;
        public string CompleteRealmLabel;
        public string PowerFlowLabel;

        private static readonly Texture2D FullBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 0.84f, 0f));
        private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);
    }
}
