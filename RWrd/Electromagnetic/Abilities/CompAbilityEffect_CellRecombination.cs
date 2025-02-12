using Electromagnetic.Core;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Electromagnetic.Abilities
{
    public class CompAbilityEffect_CellRecombination : CompAbilityEffect
    {
        public new CompProperties_AbilityCellRecombination Props
        {
            get
            {
                return (CompProperties_AbilityCellRecombination)this.props;
            }
        }
        private Pawn Pawn
        {
            get
            {
                return this.parent.pawn;
            }
        }
        //技能接口
        private RWrd_PsyCastBase Ability
        {
            get
            {
                return (RWrd_PsyCastBase)this.parent;
            }
        }
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn pawn = target.Pawn;
            float maxQuality = 1.3f;
            float quality;
            if (Pawn != null)
            {
                quality = Pawn.GetStatValue(StatDefOf.MedicalTendQuality, true, -1);
            }
            else
            {
                quality = 0.75f;
            }
            quality *= 1.6f;
            Building_Bed building_Bed = (pawn != null) ? pawn.CurrentBed() : null;
            if (building_Bed != null)
            {
                quality += building_Bed.GetStatValue(StatDefOf.MedicalTendQualityOffset, true, -1);
            }
            if (Pawn != pawn && Pawn != null)
            {
                quality *= 0.7f;
            }
            quality = Mathf.Clamp(quality, 0f, maxQuality);
            tendableHediffsInTendPriorityOrder.Clear();
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                bool flag = hediffs[i].TendableNow(false);
                if (flag)
                {
                    tendableHediffsInTendPriorityOrder.Add(hediffs[i]);
                }
            }
            bool flag2 = tendableHediffsInTendPriorityOrder.Count != 0;
            if (flag2)
            {
                TendUtility.SortByTendPriority(tendableHediffsInTendPriorityOrder);
                TendUtility.GetOptimalHediffsToTendWithSingleTreatment(pawn, true, tmpHediffs, tendableHediffsInTendPriorityOrder);
                for (int j = 0; j < tendableHediffsInTendPriorityOrder.Count; j++)
                {
                    tendableHediffsInTendPriorityOrder[j].Tended(quality, maxQuality, j);
                }
                tendableHediffsInTendPriorityOrder.Clear();
                tmpHediffs.Clear();
            }
        }

        private static List<Hediff> tendableHediffsInTendPriorityOrder = new List<Hediff>();
        private static List<Hediff> tmpHediffs = new List<Hediff>();
    }
}
