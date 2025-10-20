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
using rjw;
using Verse.AI;
using rjw.Modules.Interactions;
using System.Reflection;

namespace Electromagnetic.UI
{
    public class Dialog_​BodyRemold : Window
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
            Dialog_​BodyRemold.NavSelectorImageBox(rectHaare, null, null, null, null, "RWrd_HAIR".Translate(), null, null, null, ColorTool.colBeige);
            // 发型
            Rect rectHairSelector = listing_Standard.GetRect(24);
            Dialog_​BodyRemold.NavSelectorImageBox(rectHairSelector, () => HairTool.AChooseHairCustom(this.pawn), new Action(this.ASetPrevHair), new Action(this.ASetNextHair), null, "RWrd_FRISUR".Translate() + " - " + this.pawn.GetHairName());
            // 胡须
            if (this.pawn != null && this.pawn.style != null)
            {
                Rect rectBeardSelector = listing_Standard.GetRect(24);
                Dialog_​BodyRemold.NavSelectorImageBox(rectBeardSelector, () => StyleTool.AChooseBeardCustom(this.pawn), new Action(this.ASetPrevBeard), new Action(this.ASetNextBeard), null, "RWrd_BEARD".Translate() + " - " + this.pawn.GetBeardName());
            }
            // 发色
            Rect rectHairColorSelector = listing_Standard.GetRect(24);
            Dialog_​BodyRemold.NavSelectorImageBox(rectHairColorSelector, new Action(this.AChangeHairUI), new Action(this.AToggleColor), new Action(this.AToggleColor), null, "RWrd_HAIRCOLOR".Translate() + " - " + (this.isPrimaryColor ? "RWrd_COLORA".Translate() : "RWrd_COLORB".Translate()));
            // 脑袋
            if (this.pawn != null && this.pawn.story != null)
            {
                Rect rectHead = listing_Standard.GetRect(24);
                Dialog_​BodyRemold.NavSelectorImageBox(rectHead, null, null, null, null, BodyPartDefOf.Head.label.CapitalizeFirst(), null, null, null, ColorTool.colBeige);
                // 脸型
                Rect rectHeadSelector = listing_Standard.GetRect(24);
                Dialog_​BodyRemold.NavSelectorImageBox(rectHeadSelector, new Action(this.AChooseHeadCustom), new Action(this.ASetPrevHead), new Action(this.ASetNextHead), null, BodyPartDefOf.Head.label.CapitalizeFirst() + ": " + this.pawn.GetHeadName(null));
                // 面纹
                if (this.pawn != null && this.pawn.style != null)
                {
                    Rect rectFaceTattooSelector = listing_Standard.GetRect(24);
                    Dialog_​BodyRemold.NavSelectorImageBox(rectFaceTattooSelector, new Action(this.AChooseFaceTattooCustom), new Action(this.ASetPrevFaceTattoo), new Action(this.ASetNextFaceTattoo), null, "RWrd_FACETATTOO".Translate() + this.pawn.GetFaceTattooName());
                }
            }
            // 身体
            if (this.pawn != null && this.pawn.story != null)
            {
                Rect rectBody = listing_Standard.GetRect(24);
                Dialog_​BodyRemold.NavSelectorImageBox(rectBody, null, null, null, null, BodyPartDefOf.Torso.label.CapitalizeFirst(), null, null, null, ColorTool.colBeige);
                // 体型
                Rect rectBodySelector = listing_Standard.GetRect(24);
                Dialog_​BodyRemold.NavSelectorImageBox(rectBodySelector, new Action(this.AChooseBodyCustom), new Action(this.ASetPrevBody), new Action(this.ASetNextBody), null, "RWrd_FORM".Translate() + this.pawn.GetBodyTypeName());
                // 肤色
                Rect rectSkinColorSelector = listing_Standard.GetRect(24);
                Dialog_​BodyRemold.NavSelectorImageBox(rectSkinColorSelector, new Action(this.AChangeSkinUI), new Action(this.AToggleSkinColor), new Action(this.AToggleSkinColor), null, "RWrd_SKIN".Translate() + (this.isPrimaryColor ? "RWrd_COLORA".Translate() : "RWrd_COLORB".Translate()));
                // 纹身
                Rect rectBodyTattooSelector = listing_Standard.GetRect(24);
                Dialog_​BodyRemold.NavSelectorImageBox(rectBodyTattooSelector, new Action(this.AChooseBodyTattooCustom), new Action(this.ASetPrevBodyTattoo), new Action(this.ASetNextBodyTattoo), null, "RWrd_TATTOO".Translate() + this.pawn.GetBodyTattooName());
            }
            listing_Standard.End();
        }
        protected void DrawCenter(Rect rect, Pawn pawn)
        {
            Rect rect2 = new Rect(rect.x + rect.width / 4f, rect.y + (rect.height - rect.width / 1.5f) / 2, rect.width / 2f, rect.width / 1.5f);
            GUI.DrawTexture(rect2, PortraitsCache.Get(pawn, rect2.size, Rot4.South, default(Vector3), 1f, true, true, false, false, null, null, false, null));
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
                Dialog_​BodyRemold.ButtonTextureTextHighlight(rectViewGenes, "", this.pawn.genes.XenotypeIcon, Color.white, new Action(this.AShowXenoType), "ViewGenes".Translate());
                Rect rectXenotypeEditor = new Rect(rectViewGenes.xMax + 4, rectViewGenes.y, 250, 30f);
                Dialog_​BodyRemold.ButtonTextureTextHighlight2(rectXenotypeEditor, this.pawn.genes.XenotypeLabelCap, null, Color.white, root.energy.AvailableLevel >= 75 ? new Action(this.AConfigXenoType) : null, "XenotypeEditor".Translate() + "\n\n" + this.pawn.genes.XenotypeDescShort, true, 10f);
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
        internal static void ButtonTextureTextHighlight(Rect rect, string text, Texture2D icon, Color color, Action action, string toolTip = "")
        {
            bool flag = Mouse.IsOver(rect);
            if (flag)
            {
                Widgets.DrawHighlight(rect);
            }
            GUI.color = color;
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.height, rect.height), icon);
            GUI.color = Color.white;
            Text.Font = GameFont.Small;
            float num = rect.height - 10f - rect.height / 2f;
            Widgets.Label(new Rect(rect.x + rect.height + 5f, rect.y + num, rect.width - rect.height - 5f, rect.height), text);
            Dialog_​BodyRemold.ButtonInvisible(rect, action, toolTip);
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
            Dialog_​BodyRemold.FloatMenuOnRect<TattooDef>(StyleTool.GetBodyTattooList(null), (TattooDef s) => s.LabelCap, new Action<TattooDef>(this.ASetBodyTattooCustom), null, true);
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
            Dialog_​BodyRemold.FloatMenuOnRect<BodyTypeDef>(this.pawn.GetBodyDefList(false), (BodyTypeDef s) => s.defName.Translate(), new Action<BodyTypeDef>(this.ASetBodyCustom), null, true);
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
            Dialog_​BodyRemold.FloatMenuOnRect<HeadTypeDef>(this.pawn.GetHeadDefList(false), (HeadTypeDef s) => s.label.NullOrEmpty() ? s.defName : s.label.ToString(), new Action<HeadTypeDef>(this.ASetHeadDefCustom), null, true);
        }
        private void ASetHeadDefCustom(HeadTypeDef val)
        {
            this.pawn.SetHeadTypeDef(val);
            this.pawn.SetDirty();
        }
        private void AChooseFaceTattooCustom()
        {
            Dialog_​BodyRemold.FloatMenuOnRect<TattooDef>(StyleTool.GetFaceTattooList(null), (TattooDef s) => s.LabelCap, new Action<TattooDef>(this.ASetFaceTattooCustom), null, true);
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
        internal static void ButtonText(Rect rect, string label, Action action, string toolTip = "")
        {
            bool flag = Widgets.ButtonText(rect, label, true, true, true, null);
            if (flag)
            {
                bool flag2 = action != null;
                if (flag2)
                {
                    action();
                }
            }
            bool flag3 = !toolTip.NullOrEmpty();
            if (flag3)
            {
                TooltipHandler.TipRegion(rect, toolTip);
            }
        }
        internal static void Image(Rect rect, string texPath)
        {
            GUI.DrawTexture(rect, ContentFinder<Texture2D>.Get(texPath, true));
        }
        internal static void ButtonHighlight(Rect rect, string texPath, Action<Color> action, Color color, string toolTip = "")
        {
            bool flag = texPath.NullOrEmpty();
            if (!flag)
            {
                bool flag2 = Mouse.IsOver(rect);
                if (flag2)
                {
                    Widgets.DrawBoxSolid(rect, new Color(color.r, color.g, color.b, 0.4f));
                }
                bool flag3 = Widgets.ButtonImage(rect, ContentFinder<Texture2D>.Get(texPath, true), color, color, true, null);
                if (flag3)
                {
                    bool flag4 = action != null;
                    if (flag4)
                    {
                        action(color);
                    }
                }
                bool flag5 = !toolTip.NullOrEmpty();
                if (flag5)
                {
                    TooltipHandler.TipRegion(rect, toolTip);
                }
            }
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
        internal static void ButtonImageCol(float x, float y, float w, float h, string texPath, Action<Color> action, Color color, string toolTip = "")
        {
            bool flag = texPath.NullOrEmpty();
            if (!flag)
            {
                Rect rect = new Rect(x, y, w, h);
                bool flag2 = Widgets.ButtonImage(rect, ContentFinder<Texture2D>.Get(texPath, true), color, true, null);
                if (flag2)
                {
                    bool flag3 = action != null;
                    if (flag3)
                    {
                        action(color);
                    }
                }
                bool flag4 = !toolTip.NullOrEmpty();
                if (flag4)
                {
                    TooltipHandler.TipRegion(rect, toolTip);
                }
            }
        }
        internal static void ButtonTextureTextHighlight2(Rect rect, string text, string texPath, Color color, Action action, string toolTip = "", bool withButton = true, float textOffset = 10f)
        {
            bool flag = Mouse.IsOver(rect);
            if (flag)
            {
                Widgets.DrawHighlight(rect);
            }
            bool flag2 = texPath != null;
            if (flag2)
            {
                GUI.color = color;
                GUI.DrawTexture(new Rect(rect.x, rect.y, rect.height, rect.height), ContentFinder<Texture2D>.Get(texPath, true));
                GUI.color = Color.white;
            }
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;
            float num = rect.height - textOffset - rect.height / 2f;
            bool flag3 = texPath == null;
            if (flag3)
            {
                Widgets.Label(rect, text);
            }
            else
            {
                Widgets.Label(new Rect(rect.x + rect.height + 5f, rect.y + num, rect.width - rect.height - 5f, rect.height), text);
            }
            Text.Anchor = TextAnchor.UpperLeft;
            if (withButton)
            {
                Dialog_​BodyRemold.ButtonInvisible(rect, action, toolTip);
            }
        }
        internal static void ButtonInvisible(Rect rect, Action action, string toolTip = "")
        {
            bool flag = Widgets.ButtonInvisible(rect, true);
            if (flag)
            {
                bool flag2 = action != null;
                if (flag2)
                {
                    action();
                }
            }
            bool flag3 = !toolTip.NullOrEmpty();
            if (flag3)
            {
                TooltipHandler.TipRegion(rect, toolTip);
            }
        }
        internal static void NavSelectorImageBox(Rect rect, Action onClicked, Action onPrev, Action onNext, Action onTextureClick, string label, string tipLabel = null, string tipTexture = null, string texturePath = null, Color colTex = default(Color))
        {
            bool flag = onPrev != null;
            if (flag)
            {
                Dialog_​BodyRemold.ButtonImage(Dialog_​BodyRemold.RectPrevious(rect), "bbackward", onPrev, "");
            }
            bool flag2 = onNext != null;
            if (flag2)
            {
                Dialog_​BodyRemold.ButtonImage(Dialog_​BodyRemold.RectNext(rect), "bforward", onNext, "");
            }
            Text.Font = GameFont.Small;
            bool flag3 = texturePath != null;
            Dialog_​BodyRemold.LabelBackground(Dialog_​BodyRemold.RectSolid(rect, true), label, new Color(0.115f, 0.115f, 0.115f), flag3 ? 25 : 0, "", colTex);
            Dialog_​BodyRemold.ButtonImage(Dialog_​BodyRemold.RectTexture(rect), texturePath, onTextureClick, tipTexture);
            int num = 0;
            Dialog_​BodyRemold.ButtonInvisibleMouseOver(Dialog_​BodyRemold.RectOnClick(rect, flag3, num, true), onClicked, tipLabel);
        }
        internal static List<FloatMenuOption> FloatMenuOnRect<T>(ICollection<T> l, Func<T, string> labelGetter, Action<T> action, Selected s = null, bool doWindow = true)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            bool flag = !l.EnumerableNullOrEmpty<T>();
            if (flag)
            {
                using (IEnumerator<T> enumerator = l.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        T element = enumerator.Current;
                        string label = labelGetter(element);
                        FloatMenuOption floatMenuOption = new FloatMenuOption(label, delegate ()
                        {
                            bool flag3 = action != null;
                            if (flag3)
                            {
                                action(element);
                            }
                        }, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                        bool flag2 = element != null;
                        if (flag2)
                        {
                            floatMenuOption.SetFMOIcon(element.GetTIcon(s));
                            floatMenuOption.iconColor = element.GetTColor(null);
                            floatMenuOption.tooltip = new TipSignal?(element.STooltip<T>());
                        }
                        list.Add(floatMenuOption);
                    }
                }
                if (doWindow)
                {
                    WindowTool.Open(new FloatMenu(list));
                }
            }
            return list;
        }
        internal static Texture2D IconForStyleCustom(Selected s, ThingStyleDef style)
        {
            return (s != null && s.thingDef != null && s.stuff != null && style != null) ? Widgets.GetIconFor(s.thingDef, s.stuff, style, null) : null;
        }
        internal static void LabelBackground(Rect rect, string text, Color col, int offset = 0, string tooltip = "", Color colText = default(Color))
        {
            Widgets.DrawBoxSolid(rect, col);
            bool flag = text == null;
            if (flag)
            {
                text = "";
            }
            Rect rect2 = new Rect(rect.x + 3f + (float)offset, rect.y, rect.width - 3f, rect.height);
            bool flag2 = rect.height > 20f && text.Length <= 22;
            if (flag2)
            {
                rect2.y += (rect.height - 20f) / 2f;
            }
            bool flag3 = colText != default(Color);
            if (flag3)
            {
                Color color = GUI.color;
                GUI.color = colText;
                Widgets.Label(rect2, text);
                GUI.color = color;
            }
            else
            {
                Widgets.Label(rect2, text);
            }
            bool flag4 = !tooltip.NullOrEmpty();
            if (flag4)
            {
                TooltipHandler.TipRegion(rect2, tooltip);
            }
        }
        internal static void ButtonInvisibleMouseOver(Rect rect, Action action, string toolTip = "")
        {
            bool flag = Mouse.IsOver(rect);
            if (flag)
            {
                Widgets.DrawHighlight(rect);
            }
            bool flag2 = Widgets.ButtonInvisible(rect, true);
            if (flag2)
            {
                bool flag3 = action != null;
                if (flag3)
                {
                    action();
                }
            }
            bool flag4 = !toolTip.NullOrEmpty();
            if (flag4)
            {
                TooltipHandler.TipRegion(rect, toolTip);
            }
        }
        internal static void ButtonImage(Rect rect, string texPath, Action action, string tooolTip = "")
        {
            bool flag = texPath.NullOrEmpty();
            if (!flag)
            {
                bool flag2 = Widgets.ButtonImage(rect, ContentFinder<Texture2D>.Get(texPath, true), true, null);
                if (flag2)
                {
                    bool flag3 = action != null;
                    if (flag3)
                    {
                        action();
                    }
                }
                bool flag4 = !tooolTip.NullOrEmpty();
                if (flag4)
                {
                    TooltipHandler.TipRegion(rect, tooolTip);
                }
            }
        }
        private static Rect RectSolid(Rect rect, bool showEdit = true)
        {
            return new Rect(rect.x + (float)(showEdit ? 21 : 0), rect.y, rect.width - (float)(showEdit ? 40 : 19), 24f);
        }
        private static Rect RectTexture(Rect rect)
        {
            return new Rect(rect.x + 25f, rect.y, 24f, 24f);
        }
        private static Rect RectPrevious(Rect rect)
        {
            return new Rect(rect.x, rect.y + 2f, 22f, 22f);
        }
        private static Rect RectOnClick(Rect rect, bool hasTexture, int offset = 0, bool showEdit = true)
        {
            return new Rect(rect.x + (float)(showEdit ? 21 : 0) + (float)(hasTexture ? 25 : 0), rect.y, rect.width - (float)(showEdit ? 40 : 19) - (float)(hasTexture ? 25 : 0) - (float)offset, 24f);
        }
        private static Rect RectNext(Rect rect)
        {
            return new Rect(rect.x + rect.width - 22f, rect.y + 2f, 22f, 22f);
        }
        private static Rect RectToggle(Rect rect)
        {
            return new Rect(rect.x + rect.width - 42f, rect.y, 22f, 22f);
        }
        public Pawn pawn;
        public Hediff_RWrd_PowerRoot root;
        internal static string tDefName = null;
        private bool isPrimaryColor;
        private bool isPrimarySkinColor;
    }
}
