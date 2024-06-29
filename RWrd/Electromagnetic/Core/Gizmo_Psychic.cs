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
                bool flag = this.pawn ==null || this.energy == null;
                return !flag;
            }
        }
        public override float GetWidth(float maxWidth)
        {
            return 176f;
        }
        public Gizmo_Psychic(Pawn pawn, Hediff_RWrd_PowerRoot energy)
        {
            this.Order = -110f;
            this.pawn = pawn;
            this.energy = energy;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new Rect(topLeft.x - 4, topLeft.y, this.GetWidth(maxWidth), 89f);
            Rect rect2 = rect.ContractedBy(2f);
            Widgets.DrawWindowBackground(rect);
            Rect rect3 = rect2;
            rect3.width = 180f;
            rect3.height = rect.height / 2f;
            this.DrawUI(rect3, this.energy.energy.energy, this.energy.energy.currentRWrd.def.MaxEnergy, this.EnergyLabel.Translate());
            Rect rect4 = rect2;
            rect4.width = 180f;
            rect4.yMin = rect2.y + 30;
            this.DrawUI2(rect4, this.energy.energy.Exp, this.energy.energy.currentRWrd.def.EXP, this.energy.energy.currentRWrd.def.level, this.energy.energy.currentRWrd.def.label, this.ExpLabel.Translate());
            Rect rect5 = rect2;
            rect5.width = 180f;
            rect5.yMin = rect4.y + 18;
            this.DrawUI3(rect5, this.energy.energy.CompleteRealm, this.energy.energy.AvailableCompleteRealm(), this.CompleteRealmLabel.Translate());
            Rect rect6 = rect2;
            rect6.width = 180f;
            rect6.yMin = rect5.y + 18;
            this.DrawUI4(rect6, this.energy.energy.PowerFlow, this.PowerFlowLabel.Translate());
            float num = this.energy.energy.energy / this.energy.energy.currentRWrd.def.MaxEnergy;
            Rect position = new Rect(rect2.x + 125f, rect2.y + 8f, 50f, 60f);
            bool flag = (double)num <= 0.25;
            Text.Anchor = TextAnchor.UpperLeft;
            return new GizmoResult(GizmoState.Clear);
        }

        private void DrawUI(Rect rect, float a, float b, string label)
        {
            Text.Font = GameFont.Small;
            Rect rect2 = new Rect(rect.x, rect.y, 60f, 25f);
            Rect rect3 = new Rect(rect.x + 60f, rect.y, 100f, 25f);
            Rect rect4 = new Rect(rect.x, rect.y + 20f, 172f, 10f);
            Widgets.Label(rect2, label);
            Widgets.Label(rect3, a.ToString("F0") + " / " + b.ToString("F0"));
            Widgets.FillableBar(rect4, a / b, Gizmo_Psychic.FullBarTex, Gizmo_Psychic.EmptyBarTex, false);
        }
        private void DrawUI2(Rect rect, float a, float b, int c, string label, string label2)
        {
            Text.Font = GameFont.Small;
            Rect rect2 = new Rect(rect.x, rect.y, 90f, 25f);
            Rect rect3 = new Rect(rect.x + 85f, rect.y, 40f, 25f);
            Rect rect4 = new Rect(rect.x + 140f, rect.y, 40f, 25f);
            Rect rect5 = new Rect(rect.x + 65f, rect.y, 40f, 25f);
            Widgets.Label(rect2, label);
            Widgets.Label(rect4, label2);
            float num = a;
            bool flag = num >= b;
            if (flag)
            {
                num = b - 1;
            }
            if (c != 0)
            {
                Widgets.Label(rect3, num.ToString("F0"));
            }
            else
            {
                Widgets.Label(rect5, num.ToString("F0"));
            }
        }

        private void DrawUI3(Rect rect, float a, int b, string label)
        {
            Text.Font = GameFont.Small;
            Rect rect2 = new Rect(rect.x, rect.y, 60f, 25f);
            Rect rect3 = new Rect(rect.x + 90f, rect.y, 40f, 25f);
            Rect rect4 = new Rect(rect.x + 140f, rect.y, 40f, 25f);
            Rect rect5 = new Rect(rect.x + 95f, rect.y, 40f, 25f);
            Rect rect6 = new Rect(rect.x + 75f, rect.y, 60f, 25f);
            Rect rect7 = new Rect(rect.x + 85f, rect.y, 40f, 25f);
            Rect rect8 = new Rect(rect.x + 80f, rect.y, 40f, 25f);
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
                Widgets.Label(rect5, b.ToString("F0"));
                Widgets.Label(rect4, "成");
            }
            else if (flag)
            {
                Widgets.Label(rect6, a.ToString("最后境界"));
            }
            else if (flag1)
            {
                Widgets.Label(rect5, c.ToString("F0"));
                Widgets.Label(rect4, "级");
            }
            else if (flag2)
            {
                Widgets.Label(rect3, c.ToString("F0"));
                Widgets.Label(rect4, "级");
            }
            else if (flag3)
            {
                Widgets.Label(rect7, c.ToString("F0"));
                Widgets.Label(rect4, "级");
            }
            else if (flag4)
            {
                Widgets.Label(rect8, c.ToString("F0"));
                Widgets.Label(rect4, "级");
            }
            else
            {
                Widgets.Label(rect9, c.ToString("F0"));
                Widgets.Label(rect4, "级");
            }
        }

        private void DrawUI4(Rect rect, int a, string label)
        {
            Text.Font = GameFont.Small;
            Rect rect2 = new Rect(rect.x, rect.y, 60f, 25f);
            Rect rect3 = new Rect(rect.x + 65f, rect.y, 70f, 25f);
            Rect rect4 = new Rect(rect.x + 140f, rect.y, 40f, 25f);
            Widgets.Label(rect2, label);
            Widgets.Label(rect3, a.ToString("F0"));
            Widgets.Label(rect4, "度");
        }

        public Pawn pawn;
        public Hediff_RWrd_PowerRoot energy;
        public string ExpLabel;
        public string EnergyLabel;
        public string CompleteRealmLabel;
        public string PowerFlowLabel;

        private static readonly Texture2D FullBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 0.84f, 0f));
        private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);
    }
}
