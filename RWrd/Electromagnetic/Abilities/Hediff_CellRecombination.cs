using Electromagnetic.Core;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Electromagnetic.Abilities
{
    public class Hediff_CellRecombination : HediffWithComps
    {
        public override void PostTick()
        {
            base.PostTick();
            bool flag = Find.TickManager.TicksGame % 125 == 0;
            bool flag2 = flag;
            if (flag2)
            {
                bool flag3 = false;
                List<Hediff_Injury> list = this.pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>().ToList<Hediff_Injury>();
                bool flag4 = list.Any<Hediff_Injury>();
                bool flag5 = flag4;
                if (flag5)
                {
                    list.RandomElement<Hediff_Injury>().Heal(30f);
                    flag3 = true;
                }
                else
                {
                    List<BodyPartRecord> nonMissingParts = this.pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null).ToList<BodyPartRecord>();
                    List<BodyPartRecord> list2 = (from x in this.pawn.def.race.body.AllParts
                                                  where this.pawn.health.hediffSet.PartIsMissing(x) && nonMissingParts.Contains(x.parent) && !this.pawn.health.hediffSet.AncestorHasDirectlyAddedParts(x)
                                                  select x).ToList<BodyPartRecord>();
                    bool flag6 = list2.Any<BodyPartRecord>();
                    bool flag7 = flag6;
                    if (flag7)
                    {
                        BodyPartRecord part = list2.RandomElement<BodyPartRecord>();
                        List<Hediff_MissingPart> source = this.pawn.health.hediffSet.hediffs.OfType<Hediff_MissingPart>().ToList<Hediff_MissingPart>();
                        this.pawn.health.RestorePart(part, null, true);
                        List<Hediff_MissingPart> currentMissingHediffs2 = this.pawn.health.hediffSet.hediffs.OfType<Hediff_MissingPart>().ToList<Hediff_MissingPart>();
                        IEnumerable<Hediff_MissingPart> enumerable = from x in source
                                                                     where !currentMissingHediffs2.Contains(x)
                                                                     select x;
                        foreach (Hediff_MissingPart hediff_MissingPart in enumerable)
                        {
                            Hediff hediff = HediffMaker.MakeHediff(RWrd_DefOf.RWrd_Regenerating, this.pawn, hediff_MissingPart.Part);
                            hediff.Severity = hediff_MissingPart.Part.def.GetMaxHealth(this.pawn) - 1f;
                            this.pawn.health.AddHediff(hediff, null, null, null);
                        }
                        flag3 = true;
                    }
                }
                bool flag8 = flag3;
                bool flag9 = flag8;
                if (flag9)
                {
                    FleckMaker.ThrowMetaIcon(this.pawn.Position, this.pawn.Map, FleckDefOf.HealingCross, 0.42f);
                }
            }
        }
    }
}
