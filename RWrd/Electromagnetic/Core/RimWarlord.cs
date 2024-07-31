using System;
using System.Collections.Generic;
using Electromagnetic.Setting;
using Electromagnetic.UI;
using RimWorld;
using UnityEngine;
using Verse;

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
            Rect rect1 = inRect.TakeLeftPart(inRect.width / 2f);
            listingStandard.Begin(rect1);
            listingStandard.CheckboxLabeled("RWrd_PowerfulPersonFragmentsEnabled".Translate(), ref RWrdSettings.PowerfulPersonFragments, "RWrd_PowerfulPersonFragmentsEnabledDesc".Translate());
            listingStandard.End();
        }
    }
}