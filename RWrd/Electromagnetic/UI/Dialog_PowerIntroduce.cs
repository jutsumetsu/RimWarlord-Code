using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Electromagnetic.UI
{
    public class Dialog_PowerIntroduce : Window
    {
        public Dialog_PowerIntroduce()
        {
            doCloseButton = false;
            doCloseX = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;
        }
        public override void DoWindowContents(Rect inRect)
        {
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);
            listing_Standard.Label("RWrd_IntroduceMessage".Translate() + "RWrd_IntroduceMessage1".Translate());
            listing_Standard.End();
        }
    }
}
