using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Electromagnetic.UI
{
    internal class DialogXenoType : GeneCreationDialogBase
    {
        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(1036f, (float)WindowTool.MaxH);
            }
        }

        protected override List<GeneDef> SelectedGenes
        {
            get
            {
                return this.selectedGenes;
            }
        }

        protected override string Header
        {
            get
            {
                return "CreateXenotype".Translate().CapitalizeFirst();
            }
        }

        protected override string AcceptButtonLabel
        {
            get
            {
                return "SaveAndApply".Translate().CapitalizeFirst();
            }
        }

        internal DialogXenoType(Pawn _pawn)
        {
            this.predefinedXenoDef = null;
            this.pawn = _pawn;
            bool flag = !this.pawn.genes.xenotypeName.NullOrEmpty();
            if (flag)
            {
                this.xenotypeName = this.pawn.genes.xenotypeName;
            }
            else
            {
                this.xenotypeName = "";
            }
            this.doOnce = true;
            this.doCloseX = true;
            this.absorbInputAroundWindow = true;
            this.closeOnAccept = false;
            this.closeOnCancel = true;
            this.closeOnClickedOutside = true;
            this.layer = WindowLayer.Dialog;
            this.draggable = true;
            this.alwaysUseFullBiostatsTableHeight = true;
            this.searchWidgetOffsetX = (float)((double)GeneCreationDialogBase.ButSize.x * 2.0 + 4.0);
            foreach (GeneCategoryDef key in DefDatabase<GeneCategoryDef>.AllDefs)
            {
                this.collapsedCategories.Add(key, false);
            }
            this.OnGenesChanged();
        }

        public override void PostOpen()
        {
            bool flag2 = !ModsConfig.BiotechActive;
            if (flag2)
            {
                this.Close(false);
            }
            else
            {
                base.PostOpen();
            }
            bool flag3 = (this.pawn.genes != null || this.pawn.genes.Xenotype.IsNullOrEmpty()) && DefDatabase<XenotypeDef>.AllDefs.Contains(this.pawn.genes.Xenotype);
            bool flag4 = flag3 && this.pawn.genes.Xenotype != XenotypeDefOf.Baseliner;
            if (flag4)
            {
                this.ALoadXenotypeDef(this.pawn.genes.Xenotype);
            }
            else
            {
                List<GeneDef> list = new List<GeneDef>();
                foreach (Gene gene in this.pawn.genes.Xenogenes)
                {
                    list.Add(gene.def);
                }
                CustomXenotype customXenotype = new CustomXenotype();
                CustomXenotype customXenotype2 = customXenotype;
                string xenotypeName = this.pawn.genes.xenotypeName;
                customXenotype2.name = ((xenotypeName != null) ? xenotypeName.Trim() : null);
                bool flag5 = customXenotype.name.NullOrEmpty();
                if (flag5)
                {
                    customXenotype.name = "";
                }
                customXenotype.genes.AddRange(list);
                customXenotype.inheritable = this.pawn.genes.Xenotype.inheritable;
                customXenotype.iconDef = this.pawn.genes.iconDef;
                bool flag6 = customXenotype.name.NullOrEmpty();
                if (flag6)
                {
                    this.ALoadCustomXenotype(customXenotype);
                }
                else
                {
                    this.DoFileInteraction(customXenotype.name);
                }
            }
        }

        public override void Close(bool doCloseSound = true)
        {
            base.Close(doCloseSound);
        }

        protected override void DrawGenes(Rect rect)
        {
            this.hoveredAnyGene = false;
            GUI.BeginGroup(rect);
            float num = 0f;
            this.DrawSection(new Rect(0f, 0f, rect.width, this.selectedHeight), this.selectedGenes, "SelectedGenes".Translate(), ref num, ref this.selectedHeight, false, rect, ref this.selectedCollapsed);
            bool flag = !this.selectedCollapsed.Value;
            if (flag)
            {
                num += 10f;
            }
            float num2 = num;
            Widgets.Label(0f, ref num, rect.width, "Genes".Translate().CapitalizeFirst(), default(TipSignal));
            float num3 = num + 10f;
            float height = (float)((double)num3 - (double)num2 - 4.0);
            bool flag2 = Widgets.ButtonText(new Rect((float)((double)rect.width - 150.0 - 16.0), num2, 150f, height), "CollapseAllCategories".Translate(), true, true, true, null);
            if (flag2)
            {
                SoundDefOf.TabClose.PlayOneShotOnCamera(null);
                foreach (GeneCategoryDef key in DefDatabase<GeneCategoryDef>.AllDefs)
                {
                    this.collapsedCategories[key] = true;
                }
            }
            bool flag3 = Widgets.ButtonText(new Rect((float)((double)rect.width - 300.0 - 4.0 - 16.0), num2, 150f, height), "ExpandAllCategories".Translate(), true, true, true, null);
            if (flag3)
            {
                SoundDefOf.TabOpen.PlayOneShotOnCamera(null);
                foreach (GeneCategoryDef key2 in DefDatabase<GeneCategoryDef>.AllDefs)
                {
                    this.collapsedCategories[key2] = false;
                }
            }
            float num4 = num3;
            Rect rect2 = new Rect(0f, num3, rect.width - 16f, this.scrollHeight);
            Widgets.BeginScrollView(new Rect(0f, num3, rect.width, rect.height - num3), ref this.scrollPosition, rect2, true);
            Rect containingRect = rect2;
            containingRect.y = num3 + this.scrollPosition.y;
            containingRect.height = rect.height;
            bool? flag4 = null;
            this.DrawSection(rect, GeneUtility.GenesInOrder, null, ref num3, ref this.unselectedHeight, true, containingRect, ref flag4);
            bool flag5 = Event.current.type == EventType.Layout;
            if (flag5)
            {
                this.scrollHeight = num3 - num4;
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
            bool flag6 = this.hoveredAnyGene;
            if (!flag6)
            {
                this.hoveredGene = null;
            }
        }

        private void DrawSection(Rect rect, List<GeneDef> genes, string label, ref float curY, ref float sectionHeight, bool adding, Rect containingRect, ref bool? collapsed)
        {
            float num = 4f;
            bool flag = !label.NullOrEmpty();
            if (flag)
            {
                Rect rect2 = new Rect(0f, curY, rect.width, Text.LineHeight);
                rect2.xMax -= (adding ? 16f : (Text.CalcSize("ClickToAddOrRemove".Translate()).x + 4f));
                bool flag2 = collapsed != null;
                if (flag2)
                {
                    Rect position = new Rect(rect2.x, rect2.y + (float)(((double)rect2.height - 18.0) / 2.0), 18f, 18f);
                    GUI.DrawTexture(position, collapsed.Value ? TexButton.Reveal : TexButton.Collapse);
                    bool flag3 = Widgets.ButtonInvisible(rect2, true);
                    if (flag3)
                    {
                        ref bool? ptr = ref collapsed;
                        bool? flag4 = !collapsed;
                        ptr = flag4;
                        bool value = collapsed.Value;
                        if (value)
                        {
                            SoundDefOf.TabClose.PlayOneShotOnCamera(null);
                        }
                        else
                        {
                            SoundDefOf.TabOpen.PlayOneShotOnCamera(null);
                        }
                    }
                    bool flag5 = Mouse.IsOver(rect2);
                    if (flag5)
                    {
                        Widgets.DrawHighlight(rect2);
                    }
                    rect2.xMin += position.width;
                }
                Widgets.Label(rect2, label);
                bool flag6 = !adding;
                if (flag6)
                {
                    Text.Anchor = TextAnchor.UpperRight;
                    GUI.color = ColoredText.SubtleGrayColor;
                    Widgets.Label(new Rect(rect2.xMax - 18f, curY, rect.width - rect2.width, Text.LineHeight), "ClickToAddOrRemove".Translate());
                    GUI.color = Color.white;
                    Text.Anchor = TextAnchor.UpperLeft;
                }
                curY += Text.LineHeight + 3f;
            }
            bool? flag7 = collapsed;
            bool flag8 = true;
            bool flag9 = flag7.GetValueOrDefault() == flag8 & flag7 != null;
            if (flag9)
            {
                bool flag10 = Event.current.type != EventType.Layout;
                if (!flag10)
                {
                    sectionHeight = 0f;
                }
            }
            else
            {
                float num2 = curY;
                bool flag11 = false;
                float num3 = (float)(34.0 + (double)GeneCreationDialogBase.GeneSize.x + 8.0);
                float num4 = rect.width - 16f;
                float num5 = num3 + 4f;
                float b = (float)(((double)num4 - (double)num5 * (double)Mathf.Floor(num4 / num5)) / 2.0);
                Rect rect3 = new Rect(0f, curY, rect.width, sectionHeight);
                bool flag12 = !adding;
                if (flag12)
                {
                    Widgets.DrawRectFast(rect3, Widgets.MenuSectionBGFillColor, null);
                }
                curY += 4f;
                bool flag13 = !genes.Any<GeneDef>();
                if (flag13)
                {
                    Text.Anchor = TextAnchor.MiddleCenter;
                    GUI.color = ColoredText.SubtleGrayColor;
                    Widgets.Label(rect3, "(" + "NoneLower".Translate() + ")");
                    GUI.color = Color.white;
                    Text.Anchor = TextAnchor.UpperLeft;
                }
                else
                {
                    GeneCategoryDef geneCategoryDef = null;
                    int num6 = 0;
                    for (int i = 0; i < genes.Count; i++)
                    {
                        GeneDef geneDef = genes[i];
                        bool flag14 = (!adding || !this.quickSearchWidget.filter.Active || (this.matchingGenes.Contains(geneDef) && !this.selectedGenes.Contains(geneDef)) || this.matchingCategories.Contains(geneDef.displayCategory)) && (this.ignoreRestrictions || geneDef.biostatArc <= 0);
                        if (flag14)
                        {
                            bool flag15 = false;
                            bool flag16 = (double)num + (double)num3 > (double)num4;
                            if (flag16)
                            {
                                num = 4f;
                                curY += (float)((double)GeneCreationDialogBase.GeneSize.y + 8.0 + 4.0);
                                flag15 = true;
                            }
                            bool flag17 = this.quickSearchWidget.filter.Active && (this.matchingGenes.Contains(geneDef) || this.matchingCategories.Contains(geneDef.displayCategory));
                            bool flag18 = this.collapsedCategories[geneDef.displayCategory] && !flag17;
                            bool flag19 = adding && geneCategoryDef != geneDef.displayCategory;
                            if (flag19)
                            {
                                bool flag20 = !flag15 && flag11;
                                if (flag20)
                                {
                                    num = 4f;
                                    curY += (float)((double)GeneCreationDialogBase.GeneSize.y + 8.0 + 4.0);
                                }
                                geneCategoryDef = geneDef.displayCategory;
                                Rect rect4 = new Rect(num, curY, rect.width - 8f, Text.LineHeight);
                                bool flag21 = !flag17;
                                if (flag21)
                                {
                                    Rect position2 = new Rect(rect4.x, rect4.y + (float)(((double)rect4.height - 18.0) / 2.0), 18f, 18f);
                                    GUI.DrawTexture(position2, flag18 ? TexButton.Reveal : TexButton.Collapse);
                                    bool flag22 = Widgets.ButtonInvisible(rect4, true);
                                    if (flag22)
                                    {
                                        this.collapsedCategories[geneDef.displayCategory] = !this.collapsedCategories[geneDef.displayCategory];
                                        bool flag23 = this.collapsedCategories[geneDef.displayCategory];
                                        if (flag23)
                                        {
                                            SoundDefOf.TabClose.PlayOneShotOnCamera(null);
                                        }
                                        else
                                        {
                                            SoundDefOf.TabOpen.PlayOneShotOnCamera(null);
                                        }
                                    }
                                    bool flag24 = num6 % 2 == 1;
                                    if (flag24)
                                    {
                                        Widgets.DrawLightHighlight(rect4);
                                    }
                                    bool flag25 = Mouse.IsOver(rect4);
                                    if (flag25)
                                    {
                                        Widgets.DrawHighlight(rect4);
                                    }
                                    rect4.xMin += position2.width;
                                }
                                Widgets.Label(rect4, geneCategoryDef.LabelCap);
                                curY += rect4.height;
                                bool flag26 = !flag18;
                                if (flag26)
                                {
                                    GUI.color = Color.grey;
                                    Widgets.DrawLineHorizontal(num, curY, rect.width - 8f);
                                    GUI.color = Color.white;
                                    curY += 10f;
                                }
                                num6++;
                            }
                            bool flag27 = adding && flag18;
                            if (flag27)
                            {
                                flag11 = false;
                                bool flag28 = Event.current.type == EventType.Layout;
                                if (flag28)
                                {
                                    sectionHeight = curY - num2;
                                }
                            }
                            else
                            {
                                num = Mathf.Max(num, b);
                                flag11 = true;
                                bool flag29 = this.DrawGene(geneDef, !adding, ref num, curY, num3, containingRect, flag17);
                                if (flag29)
                                {
                                    bool flag30 = this.selectedGenes.Contains(geneDef);
                                    if (flag30)
                                    {
                                        SoundDefOf.Tick_Low.PlayOneShotOnCamera(null);
                                        this.selectedGenes.Remove(geneDef);
                                    }
                                    else
                                    {
                                        SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
                                        this.selectedGenes.Add(geneDef);
                                    }
                                    bool flag31 = !this.xenotypeNameLocked;
                                    if (flag31)
                                    {
                                        this.xenotypeName = GeneUtility.GenerateXenotypeNameFromGenes(this.SelectedGenes);
                                    }
                                    this.OnGenesChanged();
                                    break;
                                }
                            }
                        }
                    }
                }
                bool flag32 = !adding || flag11;
                if (flag32)
                {
                    curY += GeneCreationDialogBase.GeneSize.y + 12f;
                }
                bool flag33 = Event.current.type != EventType.Layout;
                if (!flag33)
                {
                    sectionHeight = curY - num2;
                }
            }
        }
        private bool DrawGene(GeneDef geneDef, bool selectedSection, ref float curX, float curY, float packWidth, Rect containingRect, bool isMatch)
        {
            bool flag = false;
            Rect rect = new Rect(curX, curY, packWidth, GeneCreationDialogBase.GeneSize.y + 8f);
            bool flag2 = !containingRect.Overlaps(rect);
            bool result;
            if (flag2)
            {
                curX = rect.xMax + 4f;
                result = false;
            }
            else
            {
                bool selected = !selectedSection && this.selectedGenes.Contains(geneDef);
                bool overridden = this.leftChosenGroups.Any((GeneLeftChosenGroup x) => x.overriddenGenes.Contains(geneDef));
                Widgets.DrawOptionBackground(rect, selected);
                curX += 4f;
                GeneUIUtility.DrawBiostats(geneDef.biostatCpx, geneDef.biostatMet, geneDef.biostatArc, ref curX, curY, 4f);
                Rect rect2 = new Rect(curX, curY + 4f, GeneCreationDialogBase.GeneSize.x, GeneCreationDialogBase.GeneSize.y);
                if (isMatch)
                {
                    Widgets.DrawStrongHighlight(rect2.ExpandedBy(6f), null);
                }
                GeneUIUtility.DrawGeneDef(geneDef, rect2, this.inheritable ? GeneType.Endogene : GeneType.Xenogene, () => this.GeneTip(geneDef, selectedSection), false, false, overridden);
                curX += GeneCreationDialogBase.GeneSize.x + 4f;
                bool flag3 = Mouse.IsOver(rect);
                if (flag3)
                {
                    this.hoveredGene = geneDef;
                    this.hoveredAnyGene = true;
                }
                else
                {
                    bool flag4 = this.hoveredGene != null && geneDef.ConflictsWith(this.hoveredGene);
                    if (flag4)
                    {
                        Widgets.DrawLightHighlight(rect);
                    }
                }
                bool flag5 = Widgets.ButtonInvisible(rect, true);
                if (flag5)
                {
                    flag = true;
                }
                curX = Mathf.Max(curX, rect.xMax + 4f);
                result = flag;
            }
            return result;
        }
        private string GeneTip(GeneDef geneDef, bool selectedSection)
        {
            string text = null;
            if (selectedSection)
            {
                if (this.leftChosenGroups.Any((GeneLeftChosenGroup x) => x.leftChosen == geneDef))
                {
                    text = DialogXenoType.internalTest(this.leftChosenGroups.FirstOrDefault((GeneLeftChosenGroup x) => x.leftChosen == geneDef));
                }
                else if (this.cachedOverriddenGenes.Contains(geneDef))
                {
                    text = DialogXenoType.internalTest(this.leftChosenGroups.FirstOrDefault((GeneLeftChosenGroup x) => x.overriddenGenes.Contains(geneDef)));
                }
                else if (this.randomChosenGroups.ContainsKey(geneDef))
                {
                    text = ("GeneWillBeRandomChosen".Translate() + ":\n" + (from x in this.randomChosenGroups[geneDef]
                                                                            select x.label).ToLineList("  - ", true)).Colorize(ColoredText.TipSectionTitleColor);
                }
            }

            if (this.selectedGenes.Contains(geneDef) && geneDef.prerequisite != null && !this.selectedGenes.Contains(geneDef.prerequisite))
            {
                if (!text.NullOrEmpty())
                {
                    text += "\n\n";
                }

                text += ("MessageGeneMissingPrerequisite".Translate(geneDef.label).CapitalizeFirst() + ": " + geneDef.prerequisite.LabelCap).Colorize(ColorLibrary.RedReadable);
            }

            if (!text.NullOrEmpty())
            {
                text += "\n\n";
            }

            return text + (this.selectedGenes.Contains(geneDef) ? "ClickToRemove" : "ClickToAdd").Translate().Colorize(ColoredText.SubtleGrayColor);
        }
        protected override void PostXenotypeOnGUI(float curX, float curY)
        {
            TaggedString taggedString = "GenesAreInheritable".Translate();
            TaggedString taggedString2 = "IgnoreRestrictions".Translate();
            float width = (float)((double)Mathf.Max(Text.CalcSize(taggedString).x, Text.CalcSize(taggedString2).x) + 4.0 + 24.0);
            Rect rect = new Rect(curX, curY, width, Text.LineHeight);
            Widgets.CheckboxLabeled(rect, taggedString, ref this.inheritable, false, null, null, false, false);
            bool flag = Mouse.IsOver(rect);
            if (flag)
            {
                Widgets.DrawHighlight(rect);
                TooltipHandler.TipRegion(rect, "GenesAreInheritableDesc".Translate());
            }
            rect.y += Text.LineHeight;
            int num = this.ignoreRestrictions ? 1 : 0;
            Widgets.CheckboxLabeled(rect, taggedString2, ref this.ignoreRestrictions, false, null, null, false, false);
            int num2 = this.ignoreRestrictions ? 1 : 0;
            bool flag2 = num != num2;
            if (flag2)
            {
                bool ignoreRestrictions = this.ignoreRestrictions;
                if (ignoreRestrictions)
                {
                    bool flag3 = !DialogXenoType.ignoreRestrictionsConfirmationSent;
                    if (flag3)
                    {
                        DialogXenoType.ignoreRestrictionsConfirmationSent = true;
                        WindowTool.Open(new Dialog_MessageBox("IgnoreRestrictionsConfirmation".Translate(), "Yes".Translate(), delegate ()
                        {
                        }, "No".Translate(), delegate ()
                        {
                            this.ignoreRestrictions = false;
                        }, null, false, null, null, WindowLayer.Dialog));
                    }
                }
                else
                {
                    this.selectedGenes.RemoveAll((GeneDef x) => x.biostatArc > 0);
                    this.OnGenesChanged();
                }
            }
            bool flag4 = Mouse.IsOver(rect);
            if (flag4)
            {
                Widgets.DrawHighlight(rect);
                TooltipHandler.TipRegion(rect, "IgnoreRestrictionsDesc".Translate());
            }
            this.postXenotypeHeight += rect.yMax - curY;
        }

        protected override void OnGenesChanged()
        {
            this.selectedGenes.SortGeneDefs();
            base.OnGenesChanged();
            bool flag = this.predefinedXenoDef != null;
            if (flag)
            {
                foreach (GeneDef item in this.predefinedXenoDef.AllGenes)
                {
                    bool flag2 = !this.selectedGenes.Contains(item);
                    if (flag2)
                    {
                        this.predefinedXenoDef = null;
                        break;
                    }
                }
                int num = this.selectedGenes.CountAllowNull<GeneDef>();
                int num2 = this.predefinedXenoDef.AllGenes.CountAllowNull<GeneDef>();
                bool flag3 = num != num2;
                if (flag3)
                {
                    this.predefinedXenoDef = null;
                }
            }
        }
        private void ALoadCustomXenotype(CustomXenotype xenotype)
        {
            this.predefinedXenoDef = null;
            this.xenotypeName = xenotype.name;
            this.xenotypeNameLocked = false;
            this.selectedGenes.Clear();
            this.selectedGenes.AddRange(xenotype.genes);
            this.inheritable = xenotype.inheritable;
            this.iconDef = xenotype.IconDef;
            this.OnGenesChanged();
            this.ignoreRestrictions = (this.selectedGenes.Any((GeneDef x) => x.biostatArc > 0) || !this.WithinAcceptableBiostatLimits(false));
        }
        private void ALoadXenotypeDef(XenotypeDef xenotype)
        {
            this.predefinedXenoDef = xenotype;
            this.xenotypeName = xenotype.label;
            this.xenotypeNameLocked = false;
            this.selectedGenes.Clear();
            this.selectedGenes.AddRange(xenotype.genes);
            this.inheritable = xenotype.inheritable;
            this.iconDef = XenotypeIconDefOf.Basic;
            this.OnGenesChanged();
            this.ignoreRestrictions = (this.selectedGenes.Any((GeneDef g) => g.biostatArc > 0) || !this.WithinAcceptableBiostatLimits(false));
        }
        protected void DoFileInteraction(string fileName)
        {
            string filePath = GenFilePaths.AbsFilePathForXenotype(fileName);
            PreLoadUtility.CheckVersionAndLoad(filePath, ScribeMetaHeaderUtility.ScribeHeaderMode.Xenotype, delegate
            {
                CustomXenotype xenotype;
                bool flag = GameDataSaveLoader.TryLoadXenotype(filePath, out xenotype);
                if (flag)
                {
                    this.ALoadCustomXenotype(xenotype);
                }
            }, false);
        }
        protected override void DrawSearchRect(Rect rect)
        {
            base.DrawSearchRect(rect);
            bool flag = Widgets.ButtonText(new Rect(rect.xMax - GeneCreationDialogBase.ButSize.x, rect.y, GeneCreationDialogBase.ButSize.x, GeneCreationDialogBase.ButSize.y), "LoadCustom".Translate(), true, true, true, null);
            if (flag)
            {
                WindowTool.Open(new Dialog_XenotypeList_Load(delegate (CustomXenotype xenotype)
                {
                    this.ALoadCustomXenotype(xenotype);
                }));
            }
            bool flag2 = !Widgets.ButtonText(new Rect((float)((double)rect.xMax - (double)GeneCreationDialogBase.ButSize.x * 2.0 - 4.0), rect.y, GeneCreationDialogBase.ButSize.x, GeneCreationDialogBase.ButSize.y), "LoadPremade".Translate(), true, true, true, null);
            if (!flag2)
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (XenotypeDef xenotype2 in from c in DefDatabase<XenotypeDef>.AllDefs
                                                  orderby -c.displayPriority
                                                  select c)
                {
                    XenotypeDef xenotype = xenotype2;
                    list.Add(new FloatMenuOption(xenotype.LabelCap, delegate ()
                    {
                        this.ALoadXenotypeDef(xenotype);
                    }, xenotype.Icon, XenotypeDef.IconColor, MenuOptionPriority.Default, delegate (Rect r)
                    {
                        TooltipHandler.TipRegion(r, xenotype.descriptionShort ?? xenotype.description);
                    }, null, 0f, null, null, true, 0, HorizontalJustification.Left, false));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }
        }

        protected override void DoBottomButtons(Rect rect)
        {
            Dialog_​BodyRemold.ButtonText(new Rect(rect.xMax - GeneCreationDialogBase.ButSize.x - 10f, rect.y, GeneCreationDialogBase.ButSize.x + 10f, GeneCreationDialogBase.ButSize.y), this.AcceptButtonLabel, delegate ()
            {
                this.ACheckSaveAnd(true);
            }, "");
            Dialog_​BodyRemold.ButtonText(new Rect(rect.x, rect.y, GeneCreationDialogBase.ButSize.x, GeneCreationDialogBase.ButSize.y), "Close".Translate(), delegate ()
            {
                this.Close(true);
            }, "");
            Dialog_​BodyRemold.ButtonText(new Rect(rect.x + rect.width - 270f, rect.y, 110f, 38f), "Save".Translate(), delegate ()
            {
                this.ACheckSaveAnd(false);
            }, "");
            bool flag = !this.leftChosenGroups.Any<GeneLeftChosenGroup>();
            if (!flag)
            {
                int num = this.leftChosenGroups.Sum((GeneLeftChosenGroup x) => x.overriddenGenes.Count);
                GeneLeftChosenGroup geneLeftChosenGroup = this.leftChosenGroups[0];
                string text = "GenesConflict".Translate() + ": " + "GenesConflictDesc".Translate(geneLeftChosenGroup.leftChosen.Named("FIRST"), geneLeftChosenGroup.overriddenGenes[0].Named("SECOND")).CapitalizeFirst() + ((num > 1) ? (" +" + (num - 1).ToString()) : string.Empty);
                float x2 = Text.CalcSize(text).x;
                GUI.color = ColorLibrary.RedReadable;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(new Rect((float)((double)rect.xMax - (double)GeneCreationDialogBase.ButSize.x - (double)x2 - 4.0), rect.y, x2, rect.height), text);
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;
            }
        }

        protected override bool CanAccept()
        {
            bool flag = !base.CanAccept();
            bool result;
            if (flag)
            {
                result = false;
            }
            else
            {
                bool flag2 = !this.selectedGenes.Any<GeneDef>();
                if (flag2)
                {
                    result = true;
                }
                else
                {
                    for (int i = 0; i < this.selectedGenes.Count; i++)
                    {
                        bool flag3 = this.selectedGenes[i].prerequisite != null && !this.selectedGenes.Contains(this.selectedGenes[i].prerequisite);
                        if (flag3)
                        {
                            return false;
                        }
                    }
                    bool flag4 = GenFilePaths.AllCustomXenotypeFiles.EnumerableCount() >= 200;
                    if (flag4)
                    {
                        result = false;
                    }
                    else
                    {
                        bool flag5 = this.ignoreRestrictions || !this.leftChosenGroups.Any<GeneLeftChosenGroup>();
                        if (flag5)
                        {
                            result = true;
                        }
                        else
                        {
                            result = false;
                        }
                    }
                }
            }
            return result;
        }

        protected override void Accept()
        {
            this.ASaveAnd(true);
        }

        private void ACheckSaveAnd(bool apply)
        {
            bool flag = this.CanAccept();
            if (flag)
            {
                this.ASaveAnd(apply);
            }
        }

        private void ASaveAnd(bool use)
        {
            IEnumerable<string> warnings = this.GetWarnings();
            bool flag = warnings.Any<string>();
            if (flag)
            {
                WindowTool.Open(Dialog_MessageBox.CreateConfirmation("XenotypeBreaksLimits".Translate() + ":\n" + warnings.ToLineList("  - ", true) + "\n\n" + "SaveAnyway".Translate(), delegate ()
                {
                    this.AcceptInner(use);
                }, false, null, WindowLayer.Dialog));
            }
            else
            {
                this.AcceptInner(use);
            }
        }

        private void AcceptInner(bool saveAndUse)
        {
            bool flag = this.xenotypeName.NullOrEmpty();
            if (flag)
            {
                Messages.Message("please choose a xenotype name!", MessageTypeDefOf.SilentInput);
            }
            else
            {
                CustomXenotype customXenotype = new CustomXenotype();
                CustomXenotype customXenotype2 = customXenotype;
                string xenotypeName = this.xenotypeName;
                customXenotype2.name = ((xenotypeName != null) ? xenotypeName.Trim() : null);
                customXenotype.genes.AddRange(this.selectedGenes);
                customXenotype.inheritable = this.inheritable;
                customXenotype.iconDef = this.iconDef;
                string absPath = GenFilePaths.AbsFilePathForXenotype(GenFile.SanitizedFileName(customXenotype.name));
                LongEventHandler.QueueLongEvent(delegate ()
                {
                    GameDataSaveLoader.SaveXenotype(customXenotype, absPath);
                }, "SavingLongEvent", false, null, true, false, null);
                if (saveAndUse)
                {
                    this.pawn.SetPawnXenotype(customXenotype, !this.inheritable);
                }
                this.Close(true);
            }
        }
        private IEnumerable<string> GetWarnings()
        {
            if (this.ignoreRestrictions)
            {
                if (this.arc > 0 && this.inheritable)
                {
                    yield return "XenotypeBreaksLimits_Archites".Translate();
                }

                if (this.met > GeneTuning.BiostatRange.TrueMax)
                {
                    yield return "XenotypeBreaksLimits_Exceeds".Translate("Metabolism".Translate().ToLower().Named("STAT"), this.met.Named("VALUE"), GeneTuning.BiostatRange.TrueMax.Named("MAX"));
                }
                else if (this.met < GeneTuning.BiostatRange.TrueMin)
                {
                    yield return "XenotypeBreaksLimits_Below".Translate("Metabolism".Translate().ToLower().Named("STAT"), this.met.Named("VALUE"), GeneTuning.BiostatRange.TrueMin.Named("MIN"));
                }
            }

            yield break;
        }

        protected override void UpdateSearchResults()
        {
            this.quickSearchWidget.noResultsMatched = false;
            this.matchingGenes.Clear();
            this.matchingCategories.Clear();
            bool flag = !this.quickSearchWidget.filter.Active;
            if (!flag)
            {
                foreach (GeneDef geneDef in GeneUtility.GenesInOrder)
                {
                    bool flag2 = !this.selectedGenes.Contains(geneDef);
                    if (flag2)
                    {
                        bool flag3 = this.quickSearchWidget.filter.Matches(geneDef.label);
                        if (flag3)
                        {
                            this.matchingGenes.Add(geneDef);
                        }
                        bool flag4 = this.quickSearchWidget.filter.Matches(geneDef.displayCategory.label) && !this.matchingCategories.Contains(geneDef.displayCategory);
                        if (flag4)
                        {
                            this.matchingCategories.Add(geneDef.displayCategory);
                        }
                    }
                }
                this.quickSearchWidget.noResultsMatched = (!this.matchingGenes.Any<GeneDef>() && !this.matchingCategories.Any<GeneCategoryDef>());
            }
        }
        private static string internalTest(GeneLeftChosenGroup group)
        {
            if (group == null)
            {
                return null;
            }

            return ("GeneLeftmostActive".Translate() + ":\n  - " + group.leftChosen.LabelCap + " (" + "Active".Translate() + ")" + "\n" + (from x in @group.overriddenGenes
                                                                                                                                           select (x.label + " (" + "Suppressed".Translate() + ")").Colorize(ColorLibrary.RedReadable)).ToLineList("  - ", true)).Colorize(ColoredText.TipSectionTitleColor);
        }
        private List<GeneDef> selectedGenes = new List<GeneDef>();

        private bool inheritable;

        private bool? selectedCollapsed = new bool?(false);

        private List<GeneCategoryDef> matchingCategories = new List<GeneCategoryDef>();

        private Dictionary<GeneCategoryDef, bool> collapsedCategories = new Dictionary<GeneCategoryDef, bool>();

        private bool hoveredAnyGene;

        private GeneDef hoveredGene;

        private static bool ignoreRestrictionsConfirmationSent;

        private const int MaxCustomXenotypes = 200;

        private static readonly Color OutlineColorSelected = new Color(1f, 1f, 0.7f, 1f);

        private Pawn pawn;

        private XenotypeDef predefinedXenoDef;

        private bool doOnce;
    }
}
