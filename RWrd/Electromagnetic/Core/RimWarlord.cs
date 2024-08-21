using System;
using System.Collections.Generic;
using Electromagnetic.Setting;
using Electromagnetic.UI;
using RimWorld;
using UnityEngine;
using Verse;
using static HarmonyLib.Code;

namespace Electromagnetic.Core
{
    public class RimWarlord : Mod
    {
        RWrdSettings settings;
        public RimWarlord(ModContentPack content) : base(content)
        {
            this.settings = base.GetSettings<RWrdSettings>();
        }
        public override string SettingsCategory()
        {
            return "RimWarlord".Translate();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.ColumnWidth = inRect.width * 0.48f;
            listingStandard.Begin(inRect);
            //启用经验补正因子
            listingStandard.CheckboxLabeled("RWrd_EnableExpCorrectionFactor".Translate(), ref RWrdSettings.EnableExpCorrectionFactor);
            //强者无需饮食
            listingStandard.CheckboxLabeled("RWrd_NoFoodDrinkRequired".Translate(), ref RWrdSettings.NoFoodDrinkRequired, "RWrd_NoFoodDrinkRequiredDesc".Translate());
            //强者碎片
            listingStandard.CheckboxLabeled("RWrd_PowerfulPersonFragmentsEnabled".Translate(), ref RWrdSettings.PowerfulPersonFragments, "RWrd_PowerfulPersonFragmentsEnabledDesc".Translate());
            //余波
            listingStandard.CheckboxLabeled("RWrd_PowerfulEnergyWave".Translate(), ref RWrdSettings.PowerfulEnergyWave, "RWrd_PowerfulEnergyWaveDesc".Translate());
            if (RWrdSettings.PowerfulEnergyWave)
            {
                listingStandard.CheckboxLabeled("RWrd_DoVisualWaveEffect".Translate(), ref RWrdSettings.DoVisualWaveEffect, "RWrd_DoVisualWaveEffectDesc".Translate());
            }

            listingStandard.NewColumn();
            //全局匹数上限
            Rect gllRect = listingStandard.GetRect(Text.LineHeight, 1f);
            string globalLevelLimitNumber = RWrdSettings.GlobalLevelLimit.ToString();
            Widgets.Label(gllRect.LeftHalf().Rounded(), "RWrd_GlobalLevelLimit".Translate());
            Widgets.DrawHighlightIfMouseover(gllRect);
            TooltipHandler.TipRegion(gllRect, "RWrd_GlobalLevelLimitDesc".Translate());
            Widgets.TextFieldNumeric<int>(gllRect.RightHalf().LeftPartPixels(60).Rounded(), ref RWrdSettings.GlobalLevelLimit, ref globalLevelLimitNumber, 0, 99);
            //经验倍率
            string expMultiplierNumber = RWrdSettings.ExpMultiplier.ToString();
            Rect rect = listingStandard.GetRect(Text.LineHeight, 1f);
            Rect rect1 = rect.LeftHalf().Rounded();
            Rect rect2 = rect.RightHalf().LeftPartPixels(50).Rounded();
            rect2.width -= Text.LineHeight / 6;
            Rect rect3 = new Rect(rect2.xMax, rect2.y, Text.LineHeight, Text.LineHeight);
            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect1, "RWrd_ExpMultiplierLabel".Translate());
            Text.Anchor = anchor;
            Widgets.TextFieldNumeric<int>(rect2, ref RWrdSettings.ExpMultiplier, ref expMultiplierNumber, 0f, 2000f);
            Widgets.Label(rect3, "%");
            //余波范围系数
            if (RWrdSettings.PowerfulEnergyWave)
            {
                Rect wrfRect = listingStandard.GetRect(Text.LineHeight, 1f);
                string waveRangeFactorNumber = RWrdSettings.WaveRangeFactor.ToString();
                Widgets.Label(wrfRect.LeftHalf().Rounded(), "RWrd_WaveRangeFactor".Translate());
                Widgets.TextFieldNumeric<float>(wrfRect.RightHalf().LeftPartPixels(60).Rounded(), ref RWrdSettings.WaveRangeFactor, ref waveRangeFactorNumber, 0.01f, 100);
            }
            listingStandard.End();
        }
    }
}