using Electromagnetic.Setting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Electromagnetic.UI
{
    public class Dialog_ChangeEMStats : Window
    {
        private readonly ITab_Pawn_RWrd parent;
        public Dialog_ChangeEMStats(ITab_Pawn_RWrd parent)
        {
            doCloseButton = false;
            doCloseX = true;
            closeOnClickedOutside = false;
            absorbInputAroundWindow = true;
            forcePause = true;
            this.parent = parent;
            this.level = parent.root.energy.level;
            this.levelNumber = level.ToString();
            this.exp = (int)parent.root.energy.exp;
            this.expNumber = exp.ToString();
            this.cr = parent.root.energy.completerealm;
            this.crNumber = cr.ToString();
            this.pf = parent.root.energy.powerflow;
            this.pfNumber = pf.ToString();
            this.mt = parent.root.MartialTalent;
            this.mtNumber = mt.ToString();
            this.tdf = parent.root.energy.trainDesireFactor;
            this.tdfNumber = tdf.ToString();
        }
        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(500f, 200f);
            }
        }
        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Text.Anchor = TextAnchor.MiddleLeft;
            if (parent.GetSelectedPawn() == null)
            {
                Close();
            }
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.ColumnWidth = inRect.width * 0.48f;
            listingStandard.Begin(inRect);
            //等级
            Rect levelRect = listingStandard.GetRect(Text.LineHeight, 1f);
            Widgets.Label(levelRect.LeftHalf().Rounded(), "RWrd_Level".Translate());
            Widgets.TextFieldNumeric<int>(levelRect.RightHalf().LeftPartPixels(60).Rounded(), ref level, ref levelNumber, 0, 99);
            //完全境界
            Rect crRect = listingStandard.GetRect(Text.LineHeight, 1f);
            Widgets.Label(crRect.LeftHalf().Rounded(), "RWrd_CompleteRealm".Translate());
            Widgets.TextFieldNumeric<float>(crRect.RightHalf().LeftPartPixels(60).Rounded(), ref cr, ref crNumber, 0.01f, 10000);
            //武学天赋
            Rect mtRect = listingStandard.GetRect(Text.LineHeight, 1f);
            Widgets.Label(mtRect.LeftHalf().Rounded(), "RWrd_MartialTalent".Translate());
            Widgets.TextFieldNumeric<float>(mtRect.RightHalf().LeftPartPixels(60).Rounded(), ref mt, ref mtNumber, 0.01f, 2);

            listingStandard.NewColumn();
            //经验
            Rect expRect = listingStandard.GetRect(Text.LineHeight, 1f);
            Widgets.Label(expRect.LeftHalf().Rounded(), "RWrd_EXP".Translate());
            Widgets.TextFieldNumeric<int>(expRect.RightHalf().LeftPartPixels(60).Rounded(), ref exp, ref expNumber, 0, 9999);
            //力量流量
            Rect pfRect = listingStandard.GetRect(Text.LineHeight, 1f);
            Widgets.Label(pfRect.LeftHalf().Rounded(), "RWrd_PowerFlow".Translate());
            Widgets.TextFieldNumeric<int>(pfRect.RightHalf().LeftPartPixels(60).Rounded(), ref pf, ref pfNumber, 10000, 100000000);
            //练功渴望
            Rect tdfRect = listingStandard.GetRect(Text.LineHeight, 1f);
            Widgets.Label(tdfRect.LeftHalf().Rounded(), "RWrd_TrainingDesire".Translate());
            Widgets.TextFieldNumeric<int>(tdfRect.RightHalf().LeftPartPixels(60).Rounded(), ref tdf, ref tdfNumber, 1, 50);
            listingStandard.End();
            Text.Anchor = TextAnchor.UpperLeft;
        }
        public override void PreClose()
        {
            this.parent.root.energy.ForceSetLevel(level);
            this.parent.root.energy.ForceSetExp(exp);
            this.parent.root.energy.ForceSetCompleteRealm(cr);
            this.parent.root.energy.ForceSetPowerFlow(pf);
            this.parent.root.MartialTalent = this.mt;
            this.parent.root.energy.trainDesireFactor = this.tdf;
            base.PreClose();
        }
        int level;
        int exp;
        float cr;
        int pf;
        float mt;
        int tdf;
        string levelNumber;
        string expNumber;
        string crNumber;
        string pfNumber;
        string mtNumber;
        string tdfNumber;
    }
}
