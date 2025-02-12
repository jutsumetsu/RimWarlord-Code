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
    public class Hediff_CellRecombination : Hediff_TargetBase
    {
        public override void PostTick()
        {
            base.PostTick();
            int level = 0;
            float multiper = 1;
            if (this.root != null)
            {
                level = this.root.energy.level;
                multiper = 1.25f - level * 0.01f;
            }
            int timeInterval = (int)Math.Ceiling(125 * multiper);
            bool flag = Find.TickManager.TicksGame % timeInterval == 0;
            bool flag2 = flag;
            if (flag2)
            {
                bool flag3 = false;
                //伤势列表
                List<Hediff_Injury> list = this.pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>().ToList<Hediff_Injury>();
                bool flag4 = list.Any();
                bool flag5 = flag4;
                if (flag5)
                {
                    if (this.root != null)
                    {
                        //随机治愈伤势
                        int lf1 = Math.Max(level - 25, 0);
                        if (this.root.energy.IsUltimate)
                        {
                            lf1 += (int)Math.Floor(this.root.energy.PowerEnergy);
                        }
                        int lf2 = lf1 * 2;
                        int healPoint = (int)Math.Ceiling(UnityEngine.Random.Range(40f + lf1, 41f + lf2));
                        if (level < 25)
                        {
                            list.RandomElement<Hediff_Injury>().Heal((int)Math.Ceiling(UnityEngine.Random.Range(40f + lf2, 41f + lf1)));
                        }
                        if (level >= 25)
                        {
                            list.RandomElement<Hediff_Injury>().Heal(healPoint);
                            list.RandomElement<Hediff_Injury>().Heal(healPoint);
                        }
                        if (level >= 50)
                        {
                            list.RandomElement<Hediff_Injury>().Heal(healPoint);
                        }
                        if (level >= 75)
                        {
                            list.RandomElement<Hediff_Injury>().Heal(healPoint);
                        }
                    }
                    else
                    {
                        list.RandomElement<Hediff_Injury>().Heal(30f);
                    }
                    flag3 = true;
                }
                else if (!flag5 && level >= 25)
                {
                    //缺失部位列表
                    List<BodyPartRecord> nonMissingParts = this.pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null).ToList<BodyPartRecord>();
                    List<BodyPartRecord> list2 = new List<BodyPartRecord>();
                    if (level >= 50 || this.root.energy.completerealm >= 0.5f)
                    {
                        list2 = (from x in this.pawn.def.race.body.AllParts
                                 where this.pawn.health.hediffSet.PartIsMissing(x) && nonMissingParts.Contains(x.parent) && !this.pawn.health.hediffSet.AncestorHasDirectlyAddedParts(x)
                                 select x).ToList<BodyPartRecord>();
                    }
                    else
                    {
                        list2 = (from x in this.pawn.def.race.body.AllParts
                                 where this.pawn.health.hediffSet.PartIsMissing(x) && nonMissingParts.Contains(x.parent) && !this.pawn.health.hediffSet.AncestorHasDirectlyAddedParts(x) && (x.depth != BodyPartDepth.Inside) && !x.IsInGroup(BodyPartGroupDefOf.Torso) && !x.IsInGroup(BodyPartGroupDefOf.UpperHead)
                                 select x).ToList<BodyPartRecord>();
                    }
                    bool flag6 = list2.Any<BodyPartRecord>();
                    bool flag7 = flag6;
                    if (flag7)
                    {
                        //再生缺失部位
                        BodyPartRecord part = list2.RandomElement<BodyPartRecord>();
                        List<Hediff_MissingPart> source = this.pawn.health.hediffSet.hediffs.OfType<Hediff_MissingPart>().ToList<Hediff_MissingPart>();
                        this.pawn.health.RestorePart(part, null, true);
                        List<Hediff_MissingPart> currentMissingHediffs2 = this.pawn.health.hediffSet.hediffs.OfType<Hediff_MissingPart>().ToList<Hediff_MissingPart>();
                        IEnumerable<Hediff_MissingPart> enumerable = from x in source
                                                                     where !currentMissingHediffs2.Contains(x)
                                                                     select x;
                        foreach (Hediff_MissingPart hediff_MissingPart in enumerable)
                        {
                            //赋予再生Hediff
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
                    //生成治愈十字特效
                    FleckMaker.ThrowMetaIcon(this.pawn.Position, this.pawn.Map, FleckDefOf.HealingCross, 0.42f);
                }
            }
        }
    }
}
