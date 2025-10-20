using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Electromagnetic.Core;
using UnityEngine;
using Verse;

namespace Electromagnetic.UI
{
    internal class DialogColorPicker : Window
    {
        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(288f, (Verse.UI.screenHeight < 768) ? Verse.UI.screenHeight : ((Verse.UI.screenHeight < 1200) ? 768 : 800));
            }
        }
        private int GetWindowWidth()
        {
            return (this.colorType == ColorType.SkinColor || this.colorType == ColorType.FavColor || this.colorType == ColorType.EyeColor || this.colorType == ColorType.GeneColorHair || this.colorType == ColorType.GeneColorSkinBase || this.colorType == ColorType.GeneColorSkinOverride) ? 288 : 528;
        }
        internal DialogColorPicker(Pawn pawn, Hediff_RWrd_PowerRoot root, ColorType _colorType, bool _primaryColor = true, Apparel a = null, ThingWithComps w = null, GeneDef g = null) : base(null)
        {
            this.doCloseX = true;
            this.absorbInputAroundWindow = false;
            this.draggable = true;
            this.layer = WindowLayer.Dialog;
            this.doOnce = true;
            this.isPrimaryColor = _primaryColor;
            this.isColor1Choosen = this.isPrimaryColor;
            this.regexColor = new Regex("^[0-9,]*");
            this.regexHex = new Regex("^[0-9A-F]*");
            this.scrollPos = default(Vector2);
            this.colorType = _colorType;
            this.root = root;
            this.tempPawn = pawn;
            this.Init(a, w, g);
        }
        private void Init(Apparel a, ThingWithComps w, GeneDef g)
        {
            this.selectedApparel = null;
            this.selectedDef = null;
            this.selectedGeneDef = null;
            this.Xwidth = this.GetWindowWidth();
            this.bInstantClose = false;
            this.doOnce = true;
            bool flag = !this.Preselect(a, w, g);
            if (flag)
            {
                this.bInstantClose = true;
            }
            else
            {
                this.bInstantClose = false;
                this.fMinBright = 0f;
                this.fMaxBright = 1f;
                this.fRed = this.selectedColor.r;
                this.fBlue = this.selectedColor.b;
                this.fGreen = this.selectedColor.g;
                this.fAlpha = this.selectedColor.a;
                this.part = 1.0 / 255.0;
                this.TextValuesFromSelectedColor();
            }
        }
        private void TextValuesFromSelectedColor()
        {
            double num = (double)this.selectedColor.r / this.part;
            double num2 = (double)this.selectedColor.g / this.part;
            double num3 = (double)this.selectedColor.b / this.part;
            double num4 = (double)this.selectedColor.a / this.part;
            this.iRed = (int)num;
            this.iGreen = (int)num2;
            this.iBlue = (int)num3;
            this.iAlpha = (int)num4;
            this.sRGB = string.Concat(new string[]
            {
                this.iRed.ToString(),
                ",",
                this.iGreen.ToString(),
                ",",
                this.iBlue.ToString(),
                ",",
                this.iAlpha.ToString()
            });
            this.sHex = string.Concat(new string[]
            {
                this.iRed.ToString("X2"),
                ",",
                this.iGreen.ToString("X2"),
                ",",
                this.iBlue.ToString("X2"),
                ",",
                this.iAlpha.ToString("X2")
            });
            this.oldsRGB = this.sRGB;
            this.oldHex = this.sHex;
            this.oldSelected = this.selectedColor;
            bool flag2 = this.colorType == ColorType.HairColor;
            if (flag2)
            {
                this.tempPawn.SetHairColor(this.isColor1Choosen, this.selectedColor);
            }
            else
            {
                bool flag3 = this.colorType == ColorType.ApparelColor;
                if (flag3)
                {
                    bool flag4 = this.selectedApparel != null;
                    if (flag4)
                    {
                        this.selectedApparel.DrawColor = this.selectedColor;
                    }
                }
                else
                {
                    bool flag5 = this.colorType == ColorType.WeaponColor;
                    if (flag5)
                    {
                        bool flag6 = this.selectedWeapon != null;
                        if (flag6)
                        {
                            this.selectedWeapon.DrawColor = this.selectedColor;
                        }
                    }
                    else
                    {
                        bool flag7 = this.colorType == ColorType.SkinColor;
                        if (flag7)
                        {
                            this.tempPawn.SetSkinColor(this.isColor1Choosen, this.selectedColor, false);
                        }
                        else
                        {
                            bool flag8 = this.colorType == ColorType.FavColor;
                            if (flag8)
                            {
                                this.tempPawn.story.favoriteColor.color = this.selectedColor;
                            }
                            else
                            {
                                bool flag9 = this.colorType == ColorType.GeneColorHair;
                                if (flag9)
                                {
                                    bool flag10 = this.selectedGeneDef != null;
                                    if (flag10)
                                    {
                                        this.selectedGeneDef.hairColorOverride = new Color?(this.selectedColor);
                                    }
                                }
                                else
                                {
                                    bool flag11 = this.colorType == ColorType.GeneColorSkinBase;
                                    if (flag11)
                                    {
                                        bool flag12 = this.selectedGeneDef != null;
                                        if (flag12)
                                        {
                                            this.selectedGeneDef.skinColorBase = new Color?(this.selectedColor);
                                        }
                                    }
                                    else
                                    {
                                        bool flag13 = this.colorType == ColorType.GeneColorSkinOverride;
                                        if (flag13)
                                        {
                                            bool flag14 = this.selectedGeneDef != null;
                                            if (flag14)
                                            {
                                                this.selectedGeneDef.skinColorOverride = new Color?(this.selectedColor);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            this.CalcClosestGeneColor();
            this.tempPawn.Drawer.renderer.SetAllGraphicsDirty();
        }
        internal bool Preselect(Apparel a, ThingWithComps w, GeneDef g)
        {
            bool flag = this.colorType == ColorType.SkinColor;
            if (flag)
            {
                bool flag2 = this.tempPawn == null && this.tempPawn.story == null;
                if (flag2)
                {
                    return false;
                }
                this.selectedColor = this.tempPawn.GetSkinColor(this.isPrimaryColor);
            }
            else
            {
                bool flag3 = this.colorType == ColorType.HairColor;
                if (flag3)
                {
                    bool flag4 = this.tempPawn == null && this.tempPawn.story == null;
                    if (flag4)
                    {
                        return false;
                    }
                    HairTool.ASelectedHairModName(HairTool.selectedHairModName);
                    StyleTool.ASelectedBeardModName(StyleTool.selectedBeardModName);
                    this.selectedColor = this.tempPawn.GetHairColor(this.isPrimaryColor);
                }
                else
                {
                    bool flag5 = this.colorType == ColorType.FavColor;
                    if (flag5)
                    {
                        bool flag6 = (this.tempPawn == null && this.tempPawn.story == null) || this.tempPawn.story.favoriteColor == null;
                        if (flag6)
                        {
                            return false;
                        }
                        this.selectedColor = this.tempPawn.story.favoriteColor.color;
                    }
                    else
                    {
                        bool flag7 = this.colorType == ColorType.GeneColorHair;
                        if (flag7)
                        {
                            bool flag8 = g == null;
                            if (flag8)
                            {
                                return false;
                            }
                            this.selectedGeneDef = g;
                            this.selectedColor = (g.hairColorOverride ?? Color.white);
                        }
                        else
                        {
                            bool flag9 = this.colorType == ColorType.GeneColorSkinBase;
                            if (flag9)
                            {
                                bool flag10 = g == null;
                                if (flag10)
                                {
                                    return false;
                                }
                                this.selectedGeneDef = g;
                                this.selectedColor = (g.skinColorBase ?? Color.white);
                            }
                            else
                            {
                                bool flag11 = this.colorType == ColorType.GeneColorSkinOverride;
                                if (flag11)
                                {
                                    bool flag12 = g == null;
                                    if (flag12)
                                    {
                                        return false;
                                    }
                                    this.selectedGeneDef = g;
                                    this.selectedColor = (g.skinColorOverride ?? Color.white);
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }
        private void CalcClosestGeneColor()
        {
            bool flag = !ModsConfig.BiotechActive || (this.tempPawn == null && this.tempPawn.genes == null);
            if (!flag && this.root.energy.AvailableLevel >= 75)
            {
                this.closestGeneDef = GeneTool.ClosestColorGene(this.selectedColor, this.colorType == ColorType.HairColor);
                this.closestGeneColr = this.closestGeneDef.IconColor;
            }
        }
        public override void DoWindowContents(Rect inRect)
        {
            bool flag = this.bInstantClose;
            if (flag)
            {
                this.Close(true);
            }
            else
            {
                int num = 0;
                this.DrawRadioButtons(0, num);
                this.DrawTitle(0, num, 252, 30);
                num += 40;
                this.DrawColorTable(0, num, 252, 20);
                num += 280;
                this.DrawDerivedColors(0, num, 252, 18);
                num += 37;
                this.DrawColorSlider(0, num, 252, 230);
                num += 190;
                this.DrawTextFields(0, num, 252, 24);
                num += 72;
                this.DrawGeneColors(0, num, 252, 30);
                this.DrawLists(268, 0, 240, (int)this.InitialSize.y - 110);
                WindowTool.SimpleCloseButton(this);
                WindowTool.TopLayerForWindowOf<Dialog_MessageBox>(true);
            }
        }
        private void DrawLists(int x, int y, int w, int h)
        {
            Text.Font = GameFont.Small;
            Rect rect = new Rect((float)x, (float)y, (float)w, (float)h);
            bool flag = this.colorType == ColorType.HairColor;
            if (flag)
            {
                Dialog_​BodyRemold.ButtonTextureTextHighlight2(new Rect((float)(x + 5), (float)y, (float)(w - 26), 28f), HairTool.onMouseover ? "RWrd_SETONMOUSEOVER".Translate() : "RWrd_SETONCLICK".Translate(), null, Color.white, delegate
                {
                    HairTool.onMouseover = !HairTool.onMouseover;
                }, "RWrd_TOGGLESELECTIONONMOUSEOVER".Translate(), true, 10f);
                y += 30;
                this.DrawHairSelector(x - 16, ref y, w + 11, h);
                this.DrawBeardSelector(x - 16, ref y, w + 11, h);
            }
        }
        private void DrawHairSelector(int x, ref int y, int w, int h)
        {
            bool flag = (this.tempPawn == null && this.tempPawn.story == null);
            if (!flag)
            {
                Rect rect = new Rect((float)x, (float)y, (float)w, 24f);
                Dialog_​BodyRemold.NavSelectorImageBox(rect, () => HairTool.AChooseHairCustom(this.tempPawn), null, null, null, "RWrd_FRISUR".Translate() + " - " + this.tempPawn.GetHairName());
                y += 30;
            }
        }
        private void DrawBeardSelector(int x, ref int y, int w, int h)
        {
            bool flag = (this.tempPawn == null && this.tempPawn.style == null);
            if (!flag)
            {
                Rect rect = new Rect((float)x, (float)y, (float)w, 24f);
                Dialog_​BodyRemold.NavSelectorImageBox(rect, () => StyleTool.AChooseBeardCustom(this.tempPawn), null, null, null, "RWrd_BEARD".Translate() + " - " + this.tempPawn.GetBeardName());
                y += 30;
            }
        }
        private void DrawGeneColors(int x, int y, int w, int h)
        {
            bool flag = !ModsConfig.BiotechActive || (this.tempPawn == null && this.tempPawn.genes == null);
            if (!flag)
            {
                List<Gene> list = (this.colorType == ColorType.HairColor) ? this.tempPawn.GetHairGenes() : this.tempPawn.GetSkinGenes();
                int count = list.Count;
                for (int i = 0; i < 8; i++)
                {
                    bool flag2 = count > i;
                    if (flag2)
                    {
                        Gene g = list[i];
                        bool flag3 = this.tempPawn.genes.Xenogenes.Contains(g);
                        Color col = g.def.IconColor;
                        Rect rect = new Rect((float)x, (float)y, (float)h, (float)h);
                        Dialog_​BodyRemold.Image(rect, flag3 ? "UI/Icons/Genes/GeneBackground_Xenogene" : "UI/Icons/Genes/GeneBackground_Endogene");
                        Dialog_​BodyRemold.ButtonHighlight(rect, g.def.iconPath, delegate (Color a)
                        {
                            this.AColorSelectedByGene(col, g);
                        }, col, "select color from gene\n[CTRL]remove gene");
                    }
                    x += h;
                }
                bool flag4 = this.closestGeneDef != null && (this.colorType == ColorType.HairColor || this.colorType == ColorType.SkinColor);
                if (flag4)
                {
                    List<Gene> list2 = (from td in list
                                        where td.def == this.closestGeneDef
                                        select td).ToList<Gene>();
                    bool flag5 = list2.NullOrEmpty<Gene>();
                    if (flag5)
                    {
                        Rect rect2 = new Rect(0f, (float)(y + h + 5), (float)h, (float)h);
                        Dialog_​BodyRemold.Image(rect2, "UI/Icons/Genes/GeneBackground_Endogene");
                        Dialog_​BodyRemold.ButtonHighlight(rect2, this.closestGeneDef.iconPath, delegate (Color a)
                        {
                            this.AGeneSelectedByColor(this.closestGeneColr, this.closestGeneDef);
                        }, this.closestGeneColr, "add as new gene");
                    }
                }
            }
        }
        private void AGeneSelectedByColor(Color color, GeneDef geneDef)
        {
            this.selectedColor = color;
            bool flag = this.colorType == ColorType.HairColor || this.colorType == ColorType.SkinColor;
            if (flag)
            {
                this.tempPawn.AddGeneAsFirst(geneDef, false);
            }
        }
        private void AColorSelectedByGene(Color color, Gene g)
        {
            bool flag = this.colorType == ColorType.HairColor || this.colorType == ColorType.SkinColor;
            if (flag)
            {
                bool control = Event.current.control;
                if (control)
                {
                    Gene gene = this.tempPawn.RemoveGeneKeepFirst(g);
                    this.selectedColor = gene.def.IconColor;
                    this.TextValuesFromSelectedColor();
                }
                else
                {
                    this.selectedColor = color;
                    this.tempPawn.MakeGeneFirst(g);
                }
            }
            else
            {
                this.selectedColor = color;
            }
        }
        private void DrawTextFields(int x, int y, int w, int h)
        {
            bool flag = this.oldSelected != this.selectedColor;
            if (flag)
            {
                this.TextValuesFromSelectedColor();
            }
            this.sRGB = Widgets.TextField(new Rect((float)x, (float)y, (float)(w - 40), (float)h), this.sRGB, 15, this.regexColor);
            y += 37;
            GUI.color = Color.gray;
            this.sHex = Widgets.TextField(new Rect((float)x, (float)y, (float)(w - 40), (float)h), this.sHex, 15, this.regexHex);
            bool flag3 = !string.IsNullOrEmpty(this.sRGB) && this.oldsRGB != this.sRGB;
            if (flag3)
            {
                this.RGBTextToSelectedColor();
            }
            bool flag4 = !string.IsNullOrEmpty(this.sHex) && this.oldHex != this.sHex;
            if (flag4)
            {
                this.HEXTextToSelectedColor();
            }
        }
        private void HEXTextToSelectedColor()
        {
            this.oldHex = this.sHex;
            string value = this.sHex.SubstringTo(",", true);
            string value2 = this.sHex.SubstringTo(",", 2).SubstringFrom(",", true);
            string value3 = this.sHex.SubstringTo(",", 3).SubstringFrom(",", 2);
            string value4 = this.sHex.SubstringFrom(",", 3);
            bool flag = true;
            bool flag2 = true;
            bool flag3 = true;
            bool flag4 = true;
            try
            {
                this.iRed = (int)Convert.ToByte(value, 16);
            }
            catch
            {
                flag = false;
            }
            try
            {
                this.iGreen = (int)Convert.ToByte(value2, 16);
            }
            catch
            {
                flag2 = false;
            }
            try
            {
                this.iBlue = (int)Convert.ToByte(value3, 16);
            }
            catch
            {
                flag3 = false;
            }
            try
            {
                this.iAlpha = (int)Convert.ToByte(value4, 16);
            }
            catch
            {
                flag4 = false;
            }
            bool flag5 = flag && flag2 && flag3 && flag4;
            if (flag5)
            {
                this.sRGB = string.Concat(new string[]
                {
                    this.iRed.ToString(),
                    ",",
                    this.iGreen.ToString(),
                    ",",
                    this.iBlue.ToString(),
                    ",",
                    this.iAlpha.ToString()
                });
                this.oldsRGB = this.sRGB;
                this.selectedColor = new Color((float)(this.part * (double)this.iRed), (float)(this.part * (double)this.iGreen), (float)(this.part * (double)this.iBlue), (float)(this.part * (double)this.iAlpha));
            }
        }
        private void RGBTextToSelectedColor()
        {
            this.oldsRGB = this.sRGB;
            string[] array = this.sRGB.Split(new string[]
            {
                ","
            }, StringSplitOptions.RemoveEmptyEntries);
            bool flag = array.Length > 3;
            if (flag)
            {
                string s = array[0];
                string s2 = array[1];
                string s3 = array[2];
                string s4 = array[3];
                bool flag2 = false;
                bool flag3 = false;
                bool flag4 = false;
                bool flag5 = false;
                bool flag6 = int.TryParse(s, out this.iRed);
                if (flag6)
                {
                    flag2 = true;
                }
                bool flag7 = int.TryParse(s2, out this.iGreen);
                if (flag7)
                {
                    flag3 = true;
                }
                bool flag8 = int.TryParse(s3, out this.iBlue);
                if (flag8)
                {
                    flag4 = true;
                }
                bool flag9 = int.TryParse(s4, out this.iAlpha);
                if (flag9)
                {
                    flag5 = true;
                }
                bool flag10 = this.iRed > ColorTool.IMAXB;
                if (flag10)
                {
                    this.iRed = ColorTool.IMAXB;
                }
                bool flag11 = this.iGreen > ColorTool.IMAXB;
                if (flag11)
                {
                    this.iGreen = ColorTool.IMAXB;
                }
                bool flag12 = this.iBlue > ColorTool.IMAXB;
                if (flag12)
                {
                    this.iBlue = ColorTool.IMAXB;
                }
                bool flag13 = this.iAlpha > ColorTool.IMAXB;
                if (flag13)
                {
                    this.iAlpha = ColorTool.IMAXB;
                }
                bool flag14 = flag2 && flag3 && flag4 && flag5;
                if (flag14)
                {
                    this.sHex = string.Concat(new string[]
                    {
                        this.iRed.ToString("X2"),
                        ",",
                        this.iGreen.ToString("X2"),
                        ",",
                        this.iBlue.ToString("X2"),
                        ",",
                        this.iAlpha.ToString("X2")
                    });
                    this.oldHex = this.sHex;
                    this.selectedColor = new Color((float)(this.part * (double)this.iRed), (float)(this.part * (double)this.iGreen), (float)(this.part * (double)this.iBlue), (float)(this.part * (double)this.iAlpha));
                }
            }
        }
        private void ARandomColor()
        {
            bool control = Event.current.control;
            if (control)
            {
                this.selectedColor = ColorTool.RandomAlphaColor;
            }
            else
            {
                this.selectedColor = ColorTool.GetRandomColor(this.fMinBright, this.fMaxBright, false);
            }
        }
        private void DrawColorSlider(int x, int y, int w, int h)
        {
            Listing_X listing_X = new Listing_X();
            listing_X.Begin(new Rect((float)x, (float)y, (float)w, (float)h));
            this.fRed = listing_X.Slider(this.selectedColor.r, 0f, 1, Color.red);
            this.fGreen = listing_X.Slider(this.selectedColor.g, 0f, 1, Color.green);
            this.fBlue = listing_X.Slider(this.selectedColor.b, 0f, 1, Color.blue);
            this.fAlpha = listing_X.Slider(this.selectedColor.a, 0f, 1, Color.white);
            this.selectedColor = new Color(this.fRed, this.fGreen, this.fBlue, this.fAlpha);
            listing_X.Gap(2f);
            GUI.color = Color.gray;
            listing_X.Label("RWrd_MIN_RANDOM_BRIGHTNESS".Translate(), -1f, -1f, -1f, null);
            listing_X.CurY -= 5f;
            this.fMinBright = listing_X.Slider(this.fMinBright, 0f, 1f);
            listing_X.CurY -= 5f;
            listing_X.Label("RWrd_MAX_RANDOM_BRIGHTNESS".Translate(), -1f, -1f, -1f, null);
            listing_X.CurY -= 5f;
            bool flag = this.fMaxBright < this.fMinBright;
            if (flag)
            {
                this.fMaxBright = this.fMinBright;
            }
            this.fMaxBright = listing_X.Slider(this.fMaxBright, 0f, 1f);
            bool flag2 = ColorTool.offsetCX != 1f - this.fMaxBright;
            if (flag2)
            {
                ColorTool.offsetCX = 1f - this.fMaxBright;
                ColorTool.lcolors = null;
            }
            listing_X.End();
        }
        private void DrawDerivedColors(int x, int y, int w, int h)
        {
            float num = 0.15f;
            for (int i = 0; i < 14; i++)
            {
                Dialog_​BodyRemold.ButtonImageCol((float)x, (float)y, (float)h, (float)h, "bwhite", new Action<Color>(this.AColorSelected), ColorTool.GetDerivedColor(this.selectedColor, num), "");
                x += h;
                num -= 0.03f;
            }
        }
        private void DrawColorTable(int x, int y, int w, int h)
        {
            Widgets.DrawBoxSolid(new Rect((float)x, (float)y, (float)w, (float)h), this.selectedColor);
            y += h;
            int num = 0;
            int num2 = 63;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 13; j++)
                {
                    Dialog_​BodyRemold.ButtonImageCol((float)x, (float)y, (float)num2, (float)h, "bwhite", new Action<Color>(this.AColorSelected), ColorTool.ListOfColors.ElementAtOrDefault(num), "");
                    y += h;
                    num++;
                }
                x += num2;
                y -= 13 * h;
            }
        }
        private void DrawTitle(int x, int y, int w, int h)
        {
            Text.Font = GameFont.Medium;
            bool flag = this.colorType == ColorType.FavColor || this.colorType == ColorType.GeneColorHair || this.colorType == ColorType.GeneColorSkinBase || this.colorType == ColorType.GeneColorSkinOverride || (this.tempPawn == null && this.tempPawn.story == null) || this.tempPawn.story.favoriteColor == null;
            if (!flag)
            {
                Dialog_​BodyRemold.ButtonTextureTextHighlight2(new Rect((float)(w - 30), (float)y, 30f, 30f), "", "bfavcolor", this.tempPawn.story.favoriteColor.color, new Action(this.AFromFavColor), CompatibilityTool.GetFavoriteColorTooltip(this.tempPawn), true, 10f);
            }
        }
        private void AFromFavColor()
        {
            this.AColorSelected(this.tempPawn.story.favoriteColor.color);
        }
        private void AColorSelected(Color color)
        {
            this.selectedColor = color;
        }
        private void DrawRadioButtons(int x, int y)
        {
            bool flag = Widgets.RadioButtonLabeled(new Rect((float)x, (float)y, 90f, 30f), "RWrd_COLORA".Translate(), this.isColor1Choosen, false);
            if (flag)
            {
                this.isColor1Choosen = true;
                bool flag2 = this.colorType == ColorType.HairColor;
                if (flag2)
                {
                    this.selectedColor = this.tempPawn.story.HairColor;
                }
                else
                {
                    bool flag3 = this.colorType == ColorType.SkinColor;
                    if (flag3)
                    {
                        this.selectedColor = this.tempPawn.GetSkinColor(true);
                    }
                }
            }
            x += 100;
            bool flag4 = this.colorType == ColorType.SkinColor;
            if (flag4)
            {
                bool flag5 = Widgets.RadioButtonLabeled(new Rect((float)x, (float)y, 90f, 30f), "RWrd_COLORB".Translate(), !this.isColor1Choosen, false);
                if (flag5)
                {
                    this.isColor1Choosen = false;
                    bool flag6 = this.colorType == ColorType.HairColor;
                    if (flag6)
                    {
                        this.selectedColor = this.tempPawn.GetHairColor(false);
                    }
                    else
                    {
                        bool flag7 = this.colorType == ColorType.SkinColor;
                        if (flag7)
                        {
                            this.selectedColor = this.tempPawn.GetSkinColor(false);
                        }
                    }
                }
            }
        }
        private Hediff_RWrd_PowerRoot root;
        private Regex regexColor;
        private Regex regexHex;
        private Vector2 scrollPos;
        private Color selectedColor;
        private Color oldSelected;
        private Apparel selectedApparel;
        private ThingWithComps selectedWeapon;
        private ThingDef selectedDef;
        private GeneDef selectedGeneDef;
        private ColorType colorType;
        private bool isColor1Choosen;
        private bool bInstantClose;
        private bool doOnce;
        private float fRed;
        private float fBlue;
        private float fGreen;
        private float fAlpha;
        private float fMaxBright;
        private float fMinBright;
        private int iRed;
        private int iGreen;
        private int iBlue;
        private int iAlpha;
        private string sRGB;
        private string oldsRGB;
        private string sHex;
        private string oldHex;
        private bool isPrimaryColor;
        private double part;
        private Pawn tempPawn;
        private int Xwidth;
        private Color closestGeneColr = Color.white;
        private GeneDef closestGeneDef = null;
    }
}
