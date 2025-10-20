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
    public class CompAbilityEffect_CellRecombination : CompAbilityEffect_Electromagnetic
    {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn pawn = target.Pawn;
            foreach(Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                if (badHediffs.Contains(hediff.def)) pawn.health.RemoveHediff(hediff);
            }
            float maxQuality = 1.3f;
            float quality;
            if (Caster != null)
            {
                quality = Caster.GetStatValue(StatDefOf.MedicalTendQuality, true, -1);
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
            if (Caster != pawn && Caster != null)
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
        private static List<HediffDef> badHediffs = new List<HediffDef>()
        {
            HediffDefOf.BloodLoss,
            HediffDefOf.CryptosleepSickness,
            HediffDefOf.DrugOverdose,
            HediffDefOf.FoodPoisoning,
            RWrd_DefOf.HeartAttack,
            HediffDefOf.ToxicBuildup,
            RWrd_DefOf.Flu,
            RWrd_DefOf.Animal_Flu,
            HediffDefOf.Plague,
            RWrd_DefOf.Animal_Plague,
            RWrd_DefOf.GutWorms,
            HediffDefOf.LungRot,
            HediffDefOf.LungRotExposure,
            RWrd_DefOf.Malaria,
            RWrd_DefOf.MuscleParasites,
            HediffDefOf.ScariaInfection,
            RWrd_DefOf.SleepingSickness,
            HediffDefOf.WoundInfection,
            RWrd_DefOf.ChemicalDamageModerate,
            RWrd_DefOf.ChemicalDamageSevere,
            RWrd_DefOf.Cirrhosis,
            HediffDefOf.ResurrectionPsychosis,
            HediffDefOf.Scaria,
            RWrd_DefOf.TraumaSavant,
            RWrd_DefOf.Alzheimers,
            RWrd_DefOf.Asthma,
            RWrd_DefOf.BadBack,
            HediffDefOf.Blindness,
            HediffDefOf.Carcinoma,
            RWrd_DefOf.Cataract,
            HediffDefOf.Dementia,
            RWrd_DefOf.Frail,
            RWrd_DefOf.HearingLoss,
            RWrd_DefOf.HeartArteryBlockage,
            HediffDefOf.OrganDecay,
            HediffDefOf.Malnutrition,
            HediffDefOf.Heatstroke,
            HediffDefOf.Hypothermia
        };
    }
}
