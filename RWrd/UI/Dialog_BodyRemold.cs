using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Electromagnetic.Core;
using System.ComponentModel.Design;
using Verse.AI;
using System.Reflection;

namespace Electromagnetic.UI
{
    public class Dialog_BodyRemold : Window
    {
        public Dialog_BodyRemold(Pawn pawn, Hediff_RWrd_PowerRoot root)
        {
            doCloseButton = false;
            doCloseX = true;
            closeOnClickedOutside = false;
            absorbInputAroundWindow = false;
            forcePause = true;
            this.pawn = pawn;
            this.root = root;
        }
        public override Vector2 InitialSize
        {
            get
            {
                if (ThreeColumns) return new Vector2(700f, 300f);
                else return new Vector2(500f, 300f);
            }
        }
        private bool ThreeColumns
        {
            get
            {
                bool gene = ModsConfig.BiotechActive && this.pawn.genes != null;
                return gene || ModDetector.RJWIsLoaded;
            }
        }
        public override void DoWindowContents(Rect inRect)
        {
            float num = inRect.width / 3f;
            Rect rect = ThreeColumns ? new Rect(inRect.x, inRect.y, num, inRect.height) : inRect.LeftHalf().Rounded();
            Rect rect2 = ThreeColumns ? new Rect(inRect.x + num, inRect.y, num, inRect.height) : inRect.RightHalf().Rounded();
            Rect rect3 = new Rect(inRect.x + num * 2f, inRect.y, num, inRect.height);
            this.DrawLeft(rect.ContractedBy(4f));
            this.DrawCenter(rect2.ContractedBy(4f), this.pawn);
            if (ThreeColumns) this.DrawRight(rect3.ContractedBy(4f));
        }
        protected void DrawLeft(Rect rect)
        {
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(rect);
            // 头发
            Rect rectHaare = listing_Standard.GetRect(24);
            UIPrimitives.NavSelectorImageBox(rectHaare, null, null, null, null, "RWrd_HAIR".Translate(), null, null, null, ColorTool.colBeige);
            // 发型
            Rect rectHairSelector = listing_Standard.GetRect(24);
            UIPrimitives.NavSelectorImageBox(rectHairSelector, () => HairTool.AChooseHairCustom(this.pawn), new Action(this.ASetPrevHair), new Action(this.ASetNextHair), null, "RWrd_FRISUR".Translate() + " - " + this.pawn.GetHairName());
            // 胡须
            if (this.pawn != null && this.pawn.style != null)
            {
                Rect rectBeardSelector = listing_Standard.GetRect(24);
                UIPrimitives.NavSelectorImageBox(rectBeardSelector, () => StyleTool.AChooseBeardCustom(this.pawn), new Action(this.ASetPrevBeard), new Action(this.ASetNextBeard), null, "RWrd_BEARD".Translate() + " - " + this.pawn.GetBeardName());
            }
            // 发色
            Rect rectHairColorSelector = listing_Standard.GetRect(24);
            UIPrimitives.NavSelectorImageBox(rectHairColorSelector, new Action(this.AChangeHairUI), new Action(this.AToggleColor), new Action(this.AToggleColor), null, "RWrd_HAIRCOLOR".Translate() + " - " + (this.isPrimaryColor ? "RWrd_COLORA".Translate() : "RWrd_COLORB".Translate()));
            // 脑袋
            if (this.pawn != null && this.pawn.story != null)
            {
                Rect rectHead = listing_Standard.GetRect(24);
                UIPrimitives.NavSelectorImageBox(rectHead, null, null, null, null, BodyPartDefOf.Head.label.CapitalizeFirst(), null, null, null, ColorTool.colBeige);
                // 脸型
                Rect rectHeadSelector = listing_Standard.GetRect(24);
                UIPrimitives.NavSelectorImageBox(rectHeadSelector, new Action(this.AChooseHeadCustom), new Action(this.ASetPrevHead), new Action(this.ASetNextHead), null, BodyPartDefOf.Head.label.CapitalizeFirst() + ": " + this.pawn.GetHeadName(null));
                // 面纹
                if (this.pawn != null && this.pawn.style != null)
                {
                    Rect rectFaceTattooSelector = listing_Standard.GetRect(24);
                    UIPrimitives.NavSelectorImageBox(rectFaceTattooSelector, new Action(this.AChooseFaceTattooCustom), new Action(this.ASetPrevFaceTattoo), new Action(this.ASetNextFaceTattoo), null, "RWrd_FACETATTOO".Translate() + this.pawn.GetFaceTattooName());
                }
            }
            // 身体
            if (this.pawn != null && this.pawn.story != null)
            {
                Rect rectBody = listing_Standard.GetRect(24);
                UIPrimitives.NavSelectorImageBox(rectBody, null, null, null, null, BodyPartDefOf.Torso.label.CapitalizeFirst(), null, null, null, ColorTool.colBeige);
                // 体型
                Rect rectBodySelector = listing_Standard.GetRect(24);
                UIPrimitives.NavSelectorImageBox(rectBodySelector, new Action(this.AChooseBodyCustom), new Action(this.ASetPrevBody), new Action(this.ASetNextBody), null, "RWrd_FORM".Translate() + this.pawn.GetBodyTypeName());
                // 肤色
                Rect rectSkinColorSelector = listing_Standard.GetRect(24);
                UIPrimitives.NavSelectorImageBox(rectSkinColorSelector, new Action(this.AChangeSkinUI), new Action(this.AToggleSkinColor), new Action(this.AToggleSkinColor), null, "RWrd_SKIN".Translate() + (this.isPrimaryColor ? "RWrd_COLORA".Translate() : "RWrd_COLORB".Translate()));
                // 纹身
                Rect rectBodyTattooSelector = listing_Standard.GetRect(24);
                UIPrimitives.NavSelectorImageBox(rectBodyTattooSelector, new Action(this.AChooseBodyTattooCustom), new Action(this.ASetPrevBodyTattoo), new Action(this.ASetNextBodyTattoo), null, "RWrd_TATTOO".Translate() + this.pawn.GetBodyTattooName());
            }
            listing_Standard.End();
        }
        protected void DrawCenter(Rect rect, Pawn pawn)
        {
            Rect rect2 = new Rect(rect.x + rect.width / 4f, rect.y + (rect.height - rect.width / 1.5f) / 2, rect.width / 2f, rect.width / 1.5f);
            RenderTexture image = PortraitsCache.Get(pawn, rect2.size, Rot4.South, default(Vector3), 1f, true, true, false, false, null, null, false, null);
            GUI.DrawTexture(rect2, image);
            pawn.Drawer.renderer.SetAllGraphicsDirty();
            Rect genderTrans = new Rect(rect.x + rect.width / 4f, rect2.yMax + 10, rect.width / 2f, 28f);
            Rect male = genderTrans.LeftHalf().MiddlePartPixels(28f, 28f);
            Rect female = genderTrans.RightHalf().MiddlePartPixels(28f, 28f);
            if (root.energy.AvailableLevel >= 75)
            {
                if (Widgets.ButtonImage(male, ContentFinder<Texture2D>.Get("UI/Icons/bmale", true), true, null)) this.pawn.SetPawnGender(Gender.Male);
                if (Widgets.ButtonImage(female, ContentFinder<Texture2D>.Get("UI/Icons/bfemale", true), true, null)) this.pawn.SetPawnGender(Gender.Female);
            }
        }
        protected void DrawRight(Rect rect)
        {
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(rect);
            if (this.pawn.genes != null && ModsConfig.BiotechActive)
            {
                Rect rectViewGenes = listing_Standard.GetRect(30f).LeftPartPixels(30f).Rounded();
                UIPrimitives.ButtonTextureTextHighlight(rectViewGenes, "", this.pawn.genes.XenotypeIcon, Color.white, new Action(this.AShowXenoType), "ViewGenes".Translate());
                Rect rectXenotypeEditor = new Rect(rectViewGenes.xMax + 4, rectViewGenes.y, 250, 30f);
                UIPrimitives.ButtonTextureTextHighlight2(rectXenotypeEditor, this.pawn.genes.XenotypeLabelCap, null, Color.white, root.energy.AvailableLevel >= 75 ? new Action(this.AConfigXenoType) : null, "XenotypeEditor".Translate() + "\n\n" + this.pawn.genes.XenotypeDescShort, true, 10f);
            }
            RJWReflectionHelper.DrawRJWButtons(listing_Standard, this.pawn);
            listing_Standard.End();
        }
        private void AShowXenoType()
        {
            WindowTool.Open(new Dialog_ViewGenes(this.pawn));
        }
        private void AConfigXenoType()
        {
            WindowTool.Open(new DialogXenoType(this.pawn));
        }
        private void ASetBodyTattooCustom(TattooDef tattooDef)
        {
            this.pawn.SetBodyTattoo(tattooDef);
            this.pawn.SetDirty();
        }

        private void ASetNextBodyTattoo()
        {
            this.pawn.SetBodyTattoo(true, false);
            this.pawn.SetDirty();
        }

        private void ASetPrevBodyTattoo()
        {
            this.pawn.SetBodyTattoo(false, false);
            this.pawn.SetDirty();
        }
        private void AChooseBodyTattooCustom()
        {
            UIPrimitives.FloatMenuOnRect<TattooDef>(StyleTool.GetBodyTattooList(null), (TattooDef s) => s.LabelCap, new Action<TattooDef>(this.ASetBodyTattooCustom), null, true);
        }
        private void AToggleSkinColor()
        {
            this.isPrimarySkinColor = !this.isPrimarySkinColor;
        }
        private void AChangeSkinUI()
        {
            Find.WindowStack.Add(new DialogColorPicker(this.pawn, this.root, ColorType.SkinColor, this.isPrimaryColor, null, null, null));
        }
        private void ASetBodyCustom(BodyTypeDef b)
        {
            this.pawn.SetBody(b);
            this.pawn.SetDirty();
        }
        private void ASetNextBody()
        {
            this.pawn.SetBody(true, false);
            this.pawn.SetDirty();
        }
        private void ASetPrevBody()
        {
            this.pawn.SetBody(false, false);
            this.pawn.SetDirty();
        }
        private void AChooseBodyCustom()
        {
            UIPrimitives.FloatMenuOnRect<BodyTypeDef>(this.pawn.GetBodyDefList(false), (BodyTypeDef s) => s.defName.Translate(), new Action<BodyTypeDef>(this.ASetBodyCustom), null, true);
        }
        private void ASetNextHead()
        {
            this.pawn.SetHead(true, false);
            this.pawn.SetDirty();
        }
        private void ASetPrevHead()
        {
            this.pawn.SetHead(false, false);
            this.pawn.SetDirty();
        }
        private void AChooseHeadCustom()
        {
            UIPrimitives.FloatMenuOnRect<HeadTypeDef>(this.pawn.GetHeadDefList(false), (HeadTypeDef s) => s.label.NullOrEmpty() ? s.defName : s.label.ToString(), new Action<HeadTypeDef>(this.ASetHeadDefCustom), null, true);
        }
        private void ASetHeadDefCustom(HeadTypeDef val)
        {
            this.pawn.SetHeadTypeDef(val);
            this.pawn.SetDirty();
        }
        private void AChooseFaceTattooCustom()
        {
            UIPrimitives.FloatMenuOnRect<TattooDef>(StyleTool.GetFaceTattooList(null), (TattooDef s) => s.LabelCap, new Action<TattooDef>(this.ASetFaceTattooCustom), null, true);
        }
        private void ASetNextFaceTattoo()
        {
            this.pawn.SetFaceTattoo(true, false);
            this.pawn.Drawer.renderer.SetAllGraphicsDirty();
        }
        private void ASetPrevFaceTattoo()
        {
            this.pawn.SetFaceTattoo(false, false);
            this.pawn.Drawer.renderer.SetAllGraphicsDirty();
        }
        private void ASetFaceTattooCustom(TattooDef tattooDef)
        {
            this.pawn.SetFaceTattoo(tattooDef);
            this.pawn.Drawer.renderer.SetAllGraphicsDirty();
        }
        private void AChangeHairUI()
        {
            Find.WindowStack.Add(new DialogColorPicker(this.pawn, this.root, ColorType.HairColor, this.isPrimaryColor, null, null, null));
        }
        private void ASetNextBeard()
        {
            this.pawn.SetBeard(true, false);
        }
        private void ASetPrevBeard()
        {
            this.pawn.SetBeard(false, false);
        }
        private void ASetNextHair()
        {
            this.pawn.SetHair(true, false, null);
        }
        private void ASetPrevHair()
        {
            this.pawn.SetHair(false, false, null);
        }
        private void AToggleColor()
        {
            this.isPrimaryColor = !this.isPrimaryColor;
        }
        public Pawn pawn;
        public Hediff_RWrd_PowerRoot root;
        internal static string tDefName = null;
        private bool isPrimaryColor;
        private bool isPrimarySkinColor;
    }
}
