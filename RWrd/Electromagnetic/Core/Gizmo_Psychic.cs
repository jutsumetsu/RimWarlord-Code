using Electromagnetic.Abilities;
using Electromagnetic.UI;
using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace Electromagnetic.Core
{
    /// <summary>
    /// 磁场力量Gizmo
    /// </summary>
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
        /// <summary>
        /// UI宽度
        /// </summary>
        /// <param name="maxWidth">最大宽度</param>
        /// <returns></returns>
        public override float GetWidth(float maxWidth)
        {
            return 176f;
        }
        /// <summary>
        /// 获取信息
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="root">力量之源</param>
        public Gizmo_Psychic(Pawn pawn, Hediff_RWrd_PowerRoot root)
        {
            this.Order = -110f;
            this.pawn = pawn;
            this.root = root;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new Rect(topLeft.x - 4, topLeft.y, this.GetWidth(maxWidth), 89f);
            Rect rect2 = rect.ContractedBy(2f);
            Widgets.DrawWindowBackground(rect);
            MainTabWindow_Inspect mainTabWindow_Inspect = (MainTabWindow_Inspect)MainButtonDefOf.Inspect.TabWindow;
            Command_Electromagnetic command_Electromagnetic = ((mainTabWindow_Inspect != null) ? mainTabWindow_Inspect.LastMouseoverGizmo : null) as Command_Electromagnetic;
            //闪烁效果
            float num = Mathf.Repeat(Time.time, 0.85f);
            float num2 = 1f;
            if (num < 0.1f)
            {
                num2 = num / 0.1f;
            }
            else if (num >= 0.25f)
            {
                num2 = 1f - (num - 0.25f) / 0.6f;
            }
            Rect rect3 = rect2;
            //能量显示
            rect3.width = 180f;
            rect3.height = rect.height / 2f;
            this.DrawUI(rect3, this.root.energy.energy, this.root.energy.MaxEnergy, this.EnergyLabel);
            //能量消耗预览
            if (command_Electromagnetic != null)
            {
                //获取ReduceEnergy
                CompAbilityEffect_ReduceEnergy compAbilityEffect_ReduceEnergy = command_Electromagnetic.Ability.CompOfType<CompAbilityEffect_ReduceEnergy>();
                //获取能量条矩形
                Rect rect31 = new Rect(rect3.x, rect3.y + 20f, 172f, 10f);
                float delta = Math.Max((this.root.energy.energy + compAbilityEffect_ReduceEnergy.EnergyReduce) / this.root.energy.MaxEnergy, 0f);
                float fillPercentage = this.root.energy.energy / this.root.energy.MaxEnergy;
                //绘制
                if (compAbilityEffect_ReduceEnergy.EnergyReduce < 0)
                {
                    float width = rect31.width;
                    rect31.xMin = UIScaling.AdjustCoordToUIScalingFloor(rect31.xMin + delta * width);
                    rect31.width = UIScaling.AdjustCoordToUIScalingCeil((fillPercentage - delta) * width);
                    GUI.color = new Color(1f, 1f, 1f, num2);
                    GenUI.DrawTextureWithMaterial(rect31, Gizmo_Psychic.EnergyBarTexReduce, null, default);
                    GUI.color = Color.white;
                }
            }
            //等级显示
            Rect rect4 = rect2;
            rect4.width = 180f;
            rect4.yMin = rect2.y + 30;
            this.DrawUI2(rect4, this.root.energy.Exp, this.root.energy.MaxExp, this.root.energy.level, this.PowerLabel.Translate(), this.ExpLabel.Translate());
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
            Text.Anchor = TextAnchor.UpperLeft;
            return new GizmoResult(GizmoState.Clear);
        }
        /// <summary>
        /// 绘制能量显示及能量条
        /// </summary>
        /// <param name="rect">矩形区域</param>
        /// <param name="a">当前能量值</param>
        /// <param name="b">最大能量值</param>
        /// <param name="label">能量值标签</param>
        private void DrawUI(Rect rect, float a, float b, string label)
        {
            if (!Tools.IsChineseLanguage)
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
            Widgets.FillableBar(rect4, a / b, Gizmo_Psychic.EnergyBarTex, Gizmo_Psychic.EmptyBarTex, false);
        }
        /// <summary>
        /// 绘制等级显示
        /// </summary>
        /// <param name="rect">矩形区域</param>
        /// <param name="a">当前经验值</param>
        /// <param name="b">最大经验值</param>
        /// <param name="c">当前等级</param>
        /// <param name="label">等级标签</param>
        /// <param name="label2">经验值标签</param>
        private void DrawUI2(Rect rect, float a, float b, int c, string label, string label2)
        {
            //等级标签位置
            Rect rect2 = new Rect(rect.x, rect.y, 120f, 25f);
            //磁场转动匹数位置
            Rect rect3 = new Rect(rect.x + 85f, rect.y, 40f, 25f);
            //经验值标签位置
            Rect rect4 = new Rect(rect.x + 140f, rect.y, 40f, 25f);
            //电推伏特位置
            Rect rect5 = new Rect(rect.x + 65f, rect.y, 40f, 25f);
            if (!Tools.IsChineseLanguage)
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
                if (!Tools.IsChineseLanguage)
                {
                    num = a / b;
                    rect3 = new Rect(rect.x + 123f, rect.y, 40f, 25f);
                    Widgets.Label(rect3, num.ToString("P0"));
                }
                else
                {
                    Widgets.Label(rect3, num.ToString("F0"));
                }
            }
            else
            {
                if (!Tools.IsChineseLanguage)
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
        /// <summary>
        /// 绘制完全境界显示
        /// </summary>
        /// <param name="rect">矩形区域</param>
        /// <param name="a">完全境界数值</param>
        /// <param name="b">有效完全境界数值</param>
        /// <param name="label">完全境界标签</param>
        private void DrawUI3(Rect rect, float a, int b, string label)
        {
            if (!Tools.IsChineseLanguage)
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
                if (!Tools.IsChineseLanguage)
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
                if (!Tools.IsChineseLanguage)
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
                if (!Tools.IsChineseLanguage)
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
                if (!Tools.IsChineseLanguage)
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
                if (!Tools.IsChineseLanguage)
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
                if (!Tools.IsChineseLanguage)
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
                if (!Tools.IsChineseLanguage)
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
        /// <summary>
        /// 绘制力量流量显示
        /// </summary>
        /// <param name="rect">矩形区域</param>
        /// <param name="a">力量流量数值</param>
        /// <param name="label">单位标签</param>
        private void DrawUI4(Rect rect, int a, string label)
        {
            if (!Tools.IsChineseLanguage)
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
        /// <summary>
        /// 力量之源
        /// </summary>
        public Hediff_RWrd_PowerRoot root;
        /// <summary>
        /// 力量标签
        /// </summary>
        public string PowerLabel;
        /// <summary>
        /// 经验标签
        /// </summary>
        public string ExpLabel;
        /// <summary>
        /// 能量标签
        /// </summary>
        public string EnergyLabel;
        /// <summary>
        /// 完全境界标签
        /// </summary>
        public string CompleteRealmLabel;
        /// <summary>
        /// 力量流量标签
        /// </summary>
        public string PowerFlowLabel;
        /// <summary>
        /// 能量条材质
        /// </summary>
        private static readonly Texture2D EnergyBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 0.84f, 0f));
        /// <summary>
        /// 能量条减少材质
        /// </summary>
        private static readonly Texture2D EnergyBarTexReduce = SolidColorMaterials.NewSolidColorTexture(new Color(0.73f, 0.65f, 0.24f));
        /// <summary>
        /// 能量条空材质
        /// </summary>
        private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);
    }
}
