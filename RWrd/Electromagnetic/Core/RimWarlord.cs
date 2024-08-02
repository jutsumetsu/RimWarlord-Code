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
            listingStandard.CheckboxLabeled("RWrd_NoFoodDrinkRequired".Translate(), ref RWrdSettings.NoFoodDrinkRequired, "RWrd_NoFoodDrinkRequiredDesc".Translate());
            listingStandard.CheckboxLabeled("RWrd_PowerfulPersonFragmentsEnabled".Translate(), ref RWrdSettings.PowerfulPersonFragments, "RWrd_PowerfulPersonFragmentsEnabledDesc".Translate());

            listingStandard.NewColumn();
            string text = RWrdSettings.ExpMultiplier.ToString();
            Rect rect = listingStandard.GetRect(Text.LineHeight, 1f);
            Rect rect1 = rect.LeftHalf().Rounded();
            Rect rect2 = rect.RightHalf().Rounded();
            Rect rect3 = rect2.RightPartPixels(Text.LineHeight);
            rect2 = rect2.LeftPartPixels(rect2.width - Text.LineHeight);
            Widgets.DrawHighlightIfMouseover(rect);
            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect1, "RWrd_ExpMultiplierLabel".Translate());
            Text.Anchor = anchor;
            Widgets.TextFieldNumeric<int>(rect2, ref RWrdSettings.ExpMultiplier, ref text, 0f, 2000f);
            Widgets.Label(rect3, "%");
            listingStandard.End();
        }
    }
}