using Electromagnetic.Core;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Electromagnetic.UI
{
    internal static class GeneTool
    {
        internal static GeneDef ClosestColorGene(Color color, bool hair)
        {
            List<GeneDef> list = hair ? GeneTool.GetAllHairGenes() : GeneTool.GetAllSkinGenes();
            Dictionary<GeneDef, Color> dictionary = new Dictionary<GeneDef, Color>();
            foreach (GeneDef geneDef in list)
            {
                dictionary.Add(geneDef, geneDef.IconColor);
            }
            GeneDef geneDef2 = null;
            GeneDef geneDef3 = null;
            GeneDef geneDef4 = null;
            GeneDef geneDef5 = null;
            GeneDef geneDef6 = null;
            GeneDef geneDef7 = null;
            GeneDef geneDef8 = null;
            double num = 0.10000000149011612;
            double num2 = 0.20000000298023224;
            double num3 = 0.30000001192092896;
            double num4 = 0.4000000059604645;
            double num5 = 0.5;
            double num6 = 0.6000000238418579;
            double num7 = 0.699999988079071;
            foreach (GeneDef geneDef9 in dictionary.Keys)
            {
                Color color2 = dictionary[geneDef9];
                double num8 = Math.Pow(Convert.ToDouble(color2.r) - (double)color.r, 2.0);
                double num9 = Math.Pow(Convert.ToDouble(color2.g) - (double)color.g, 2.0);
                double num10 = Math.Pow(Convert.ToDouble(color2.b) - (double)color.b, 2.0);
                double num11 = Math.Sqrt(num10 + num9 + num8);
                bool flag = num11 == 0.0;
                if (flag)
                {
                    geneDef2 = geneDef9;
                    break;
                }
                bool flag2 = num11 < num;
                if (flag2)
                {
                    num = num11;
                    geneDef2 = geneDef9;
                }
                else
                {
                    bool flag3 = num11 < num2;
                    if (flag3)
                    {
                        num2 = num11;
                        geneDef3 = geneDef9;
                    }
                    else
                    {
                        bool flag4 = num11 < num3;
                        if (flag4)
                        {
                            num3 = num11;
                            geneDef4 = geneDef9;
                        }
                        else
                        {
                            bool flag5 = num11 < num4;
                            if (flag5)
                            {
                                num4 = num11;
                                geneDef5 = geneDef9;
                            }
                            else
                            {
                                bool flag6 = num11 < num5;
                                if (flag6)
                                {
                                    num5 = num11;
                                    geneDef6 = geneDef9;
                                }
                                else
                                {
                                    bool flag7 = num11 < num6;
                                    if (flag7)
                                    {
                                        num6 = num11;
                                        geneDef7 = geneDef9;
                                    }
                                    else
                                    {
                                        bool flag8 = num11 < num7;
                                        if (flag8)
                                        {
                                            num7 = num11;
                                            geneDef8 = geneDef9;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            GeneDef result;
            if ((result = geneDef2) == null && (result = geneDef3) == null && (result = geneDef4) == null && (result = geneDef5) == null && (result = geneDef6) == null && (result = geneDef7) == null)
            {
                result = (geneDef8 ?? list.First<GeneDef>());
            }
            return result;
        }

        internal static void SetPawnXenotype(this Pawn p, CustomXenotype c, bool toXenogene)
        {
            bool flag = (p == null && p.genes == null) || c == null;
            if (!flag)
            {
                p.genes.SetXenotypeDirect(XenotypeDefOf.Baseliner);
                p.genes.xenotypeName = c.name;
                p.genes.iconDef = c.iconDef;
                bool flag2 = !Current.Game.customXenotypeDatabase.customXenotypes.Contains(c);
                if (flag2)
                {
                    Current.Game.customXenotypeDatabase.customXenotypes.Add(c);
                }
                p.SetGenesFromList(c.genes, toXenogene, Event.current.control);
                p.SetDirty();
            }
        }
        internal static void SetGenesFromList(this Pawn p, List<GeneDef> l, bool asXeno, bool keepOld = false)
        {
            bool flag = !keepOld;
            if (flag)
            {
                if (asXeno)
                {
                    p.ClearXenogenes();
                }
                else
                {
                    p.ClearEndogenes();
                }
            }
            foreach (GeneDef geneDef in l)
            {
                bool flag2 = geneDef != null;
                if (flag2)
                {
                    p.genes.AddGene(geneDef, asXeno);
                }
            }
        }

        internal static void ClearXenogenes(this Pawn p)
        {
            bool flag = (p == null && p.genes == null);
            if (!flag)
            {
                for (int i = p.genes.Xenogenes.Count - 1; i >= 0; i--)
                {
                    p.genes.RemoveGene(p.genes.Xenogenes[i]);
                }
            }
        }
        internal static void ClearEndogenes(this Pawn p)
        {
            bool flag = (p == null && p.genes == null);
            if (!flag)
            {
                for (int i = p.genes.Endogenes.Count - 1; i >= 0; i--)
                {
                    p.genes.RemoveGene(p.genes.Endogenes[i]);
                }
            }
        }
        internal static List<GeneDef> GetAllSkinGenes()
        {
            return DefTool.ListBy<GeneDef>((GeneDef x) => x.IsSkinGene()).ToList<GeneDef>();
        }
        internal static List<GeneDef> GetAllHairGenes()
        {
            return DefTool.ListBy<GeneDef>((GeneDef x) => x.IsHairGene()).ToList<GeneDef>();
        }
        internal static bool IsSkinGene(this GeneDef g)
        {
            return g != null && (g.endogeneCategory == EndogeneCategory.Melanin || (!g.defName.NullOrEmpty() && g.defName.StartsWith("Skin_")));
        }
        internal static bool IsHairGene(this GeneDef g)
        {
            return g != null && g.endogeneCategory == EndogeneCategory.HairColor;
        }
        internal static List<Gene> GetHairGenes(this Pawn p)
        {
            List<Gene> result;
            if (p != null && p.genes != null)
            {
                result = (from td in p.genes.GenesListForReading
                          where td.IsHairGene()
                          orderby !td.Overridden && td.Active descending
                          select td).ToList<Gene>();
            }
            else
            {
                result = new List<Gene>();
            }
            return result;
        }
        internal static List<Gene> GetSkinGenes(this Pawn p)
        {
            List<Gene> result;
            if (p != null && p.genes != null)
            {
                result = (from td in p.genes.GenesListForReading
                          where td.IsSkinGene()
                          orderby !td.Overridden && td.Active descending
                          select td).ToList<Gene>();
            }
            else
            {
                result = new List<Gene>();
            }
            return result;
        }
        internal static bool IsHairGene(this Gene g)
        {
            return g.def.IsHairGene();
        }
        internal static bool IsSkinGene(this Gene g)
        {
            return g.def.IsSkinGene();
        }
        internal static Gene RemoveGeneKeepFirst(this Pawn p, Gene gene)
        {
            List<Gene> list = gene.IsHairGene() ? p.GetHairGenes() : (gene.IsSkinGene() ? p.GetSkinGenes() : (from x in p.genes.GenesListForReading
                                                                                                              where x.def.displayCategory == gene.def.displayCategory
                                                                                                              select x).ToList<Gene>());
            bool flag = !list.NullOrEmpty<Gene>();
            Gene result;
            if (flag)
            {
                Gene gene2 = list.First<Gene>();
                Gene gene3 = (gene2.def == gene.def) ? list.At(list.NextOrPrevIndex(0, true, false)) : null;
                Gene gene4 = gene3 ?? gene2;
                p.genes.RemoveGene(gene);
                p.genes.CallMethod("Notify_GenesChanged", new object[]
                {
                    gene.def
                });
                p.MakeGeneFirst(gene4);
                result = gene4;
            }
            else
            {
                result = null;
            }
            return result;
        }
        internal static void MakeGeneFirst(this Pawn p, Gene g)
        {
            p.genes.GenesListForReading.Remove(g);
            p.genes.GenesListForReading.Insert(0, g);
            p.genes.OverrideAllConflictingGenes(g);
            bool flag = g.IsHairGene();
            if (flag)
            {
                p.SetHairColor(true, g.def.hairColorOverride ?? g.def.IconColor);
            }
            else
            {
                bool flag2 = g.IsSkinGene();
                if (flag2)
                {
                    p.SetSkinColor(true, g.def.skinColorOverride ?? (g.def.skinColorBase ?? g.def.IconColor), false);
                }
            }
            bool flag3 = g.def.forcedHair != null;
            if (flag3)
            {
                p.SetHair(g.def.forcedHair);
            }
            bool flag4 = g.IsBodySizeGene();
            if (flag4)
            {
                p.Drawer.renderer.SetAllGraphicsDirty();
            }
        }
        internal static void OverrideAllConflictingGenes(this Pawn_GeneTracker genes, Gene gene)
        {
            bool flag = !ModLister.BiotechInstalled;
            if (!flag)
            {
                gene.OverrideBy(null);
                foreach (Gene gene2 in genes.GenesListForReading)
                {
                    bool flag2 = gene2 != gene && gene2.def.ConflictsWith(gene.def);
                    if (flag2)
                    {
                        gene2.OverrideBy(gene);
                    }
                }
            }
        }
        internal static bool IsBodySizeGene(this GeneDef g)
        {
            return g != null && !g.defName.NullOrEmpty() && g.defName.StartsWith("SZBodySize_");
        }
        internal static void AddGeneAsFirst(this Pawn p, GeneDef geneDef, bool xeno)
        {
            foreach (Gene gene in p.genes.Endogenes)
            {
                bool flag = gene.def == geneDef;
                if (flag)
                {
                    Messages.Message("", MessageTypeDefOf.RejectInput);
                    return;
                }
            }
            Gene g = p.genes.AddGene(geneDef, xeno);
            p.MakeGeneFirst(g);
        }
    }
}
