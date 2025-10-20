using Electromagnetic.Core;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Electromagnetic.Abilities
{
    public class CompAbilityEffect_LifeDrain : CompAbilityEffect_Electromagnetic
    {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            List<Hediff_Injury> listOutside = (from x in Caster.health.hediffSet.hediffs.OfType<Hediff_Injury>()
                                               where x.Part.depth == BodyPartDepth.Outside
                                               select x).ToList();
            List<Hediff_Injury> listInside = (from x in Caster.health.hediffSet.hediffs.OfType<Hediff_Injury>()
                                               where x.Part.depth == BodyPartDepth.Inside
                                              select x).ToList();
            float injuryOutside = 0;
            float injuryInside = 0;
            for (int i = 0; i < listOutside.Count; i++)
            {
                injuryOutside += listOutside[i].Severity;
            }
            for (int i = 0; i < listInside.Count; i++)
            {
                injuryInside += listInside[i].Severity;
            }
            float targetHP = 0;
            if (target.Thing.def.category == ThingCategory.Pawn)
            {
                /*Log.Warning("Target is animal");*/
                Pawn pawn = (Pawn)target.Thing;
                for (int i = 0; i < pawn.def.race.body.AllParts.Count; i++)
                {
                    targetHP += pawn.def.race.body.AllParts[i].def.GetMaxHealth(pawn);
                }
                targetHP *= pawn.health.summaryHealth.SummaryHealthPercent;
                float animalReduceHP = 0;
                for (; targetHP > 0 && injuryInside > 0;)
                {
                    /*Log.Warning("Heal inside");*/
                    Hediff_Injury injuryToHeal = listInside.RandomElement<Hediff_Injury>();
                    if (targetHP > injuryToHeal.Severity)
                    {
                        animalReduceHP += injuryToHeal.Severity;
                        injuryInside -= injuryToHeal.Severity;
                        targetHP -= injuryToHeal.Severity;
                        injuryToHeal.Heal(injuryToHeal.Severity);
                    }
                    else
                    {
                        animalReduceHP += targetHP;
                        injuryInside -= targetHP;
                        injuryToHeal.Heal(targetHP);
                        targetHP -= targetHP;
                    }
                    DamageInfo dinfo = new DamageInfo(
                        def: RWrd_DefOf.RWrd_LifeDrain,
                        amount: animalReduceHP,
                        armorPenetration: 2,
                        instigator: Caster,
                        instigatorGuilty: false);
                    pawn.TakeDamage(dinfo);
                    animalReduceHP = 0;
                }
                for (; targetHP > 0 && injuryOutside > 0;)
                {
                    /*Log.Warning("Heal outside");*/
                    Hediff_Injury injuryToHeal = listOutside.RandomElement<Hediff_Injury>();
                    if (targetHP > injuryToHeal.Severity)
                    {
                        /*Log.Warning($"Injury name: {injuryToHeal}, Heal HP: {injuryToHeal.Severity}");*/
                        animalReduceHP += injuryToHeal.Severity;
                        injuryOutside -= injuryToHeal.Severity;
                        targetHP -= injuryToHeal.Severity;
                        injuryToHeal.Heal(injuryToHeal.Severity);
                    }
                    else
                    {
                        /*Log.Warning($"Injury name: {injuryToHeal}, Heal HP: {targetHP}");*/
                        animalReduceHP += targetHP;
                        injuryOutside -= targetHP;
                        injuryToHeal.Heal(targetHP);
                        targetHP -= targetHP;
                    }
                    /*Log.Warning($"Target Surplus HP: {targetHP}, Now Reduce HP: {animalReduceHP}");*/
                    DamageInfo dinfo = new DamageInfo(
                        def: RWrd_DefOf.RWrd_LifeDrain,
                        amount: animalReduceHP,
                        armorPenetration: 2,
                        instigator: Caster,
                        instigatorGuilty: false);
                    pawn.TakeDamage(dinfo);
                    animalReduceHP = 0;
                }
            }
            if (target.Thing.def.category == ThingCategory.Plant)
            {
                /*Log.Warning("Target is plant");*/
                Plant plant = (Plant)target.Thing;
                targetHP = plant.HitPoints;
                /*Log.Warning($"Plant HP: {plant.HitPoints} = {targetHP}");*/
                float plantReduceHP = 0;
                for (;targetHP > 0 && injuryInside > 0;)
                {
                    /*Log.Warning("Heal inside");*/
                    Hediff_Injury injuryToHeal = listInside.RandomElement<Hediff_Injury>();
                    if (targetHP > injuryToHeal.Severity)
                    {
                        plantReduceHP += injuryToHeal.Severity;
                        injuryInside -= injuryToHeal.Severity;
                        targetHP -= injuryToHeal.Severity;
                        injuryToHeal.Heal(injuryToHeal.Severity);
                    }
                    else
                    {
                        plantReduceHP += targetHP;
                        injuryInside -= targetHP;
                        injuryToHeal.Heal(targetHP);
                        targetHP -= targetHP;
                    }
                }
                for (; targetHP > 0 && injuryOutside > 0;)
                {
                    /*Log.Warning("Heal outside");*/
                    Hediff_Injury injuryToHeal = listOutside.RandomElement<Hediff_Injury>();
                    if (targetHP > injuryToHeal.Severity)
                    {
                        /*Log.Warning($"Injury name: {injuryToHeal}, Heal HP: {injuryToHeal.Severity}");*/
                        plantReduceHP += injuryToHeal.Severity;
                        injuryOutside -= injuryToHeal.Severity;
                        targetHP -= injuryToHeal.Severity;
                        injuryToHeal.Heal(injuryToHeal.Severity);
                    }
                    else
                    {
                        /*Log.Warning($"Injury name: {injuryToHeal}, Heal HP: {targetHP}");*/
                        plantReduceHP += targetHP;
                        injuryOutside -= targetHP;
                        injuryToHeal.Heal(targetHP);
                        targetHP -= targetHP;
                    }
                    /*Log.Warning($"Target Surplus HP: {targetHP}, Now Reduce HP: {plantReduceHP}");*/
                }
                /*Log.Warning($"Plant name: {plant}, Reduce HP: {plantReduceHP}");*/
                DamageInfo dinfo = new DamageInfo(
                        def: RWrd_DefOf.RWrd_LifeDrain,
                        amount: plantReduceHP,
                        instigator: Caster,
                        instigatorGuilty: false);
                plant.TakeDamage(dinfo);
            }
        }
    }
}
