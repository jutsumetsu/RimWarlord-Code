﻿using Electromagnetic.Core;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Electromagnetic.Abilities
{
    public class CompAbilityEffect_HeavenLock : CompAbilityEffect
    {
        //绑定Properties
        public new CompProperties_AbilityHeavenLock Props
        {
            get
            {
                return (CompProperties_AbilityHeavenLock)this.props;
            }
        }
        //设置天锁削弱程度
        private static IEnumerable<PawnCapacityModifier> GetPCMList(float factor)
        {
            yield return new PawnCapacityModifier
            {
                capacity = PawnCapacityDefOf.Consciousness,
                offset = factor,
            };
            yield return new PawnCapacityModifier
            {
                capacity = PawnCapacityDefOf.Moving,
                offset = factor,
            };
            yield return new PawnCapacityModifier
            {
                capacity = PawnCapacityDefOf.Sight,
                offset = factor - 1,
            };
            yield return new PawnCapacityModifier
            {
                capacity = PawnCapacityDefOf.Hearing,
                offset = factor - 1,
            };
            yield return new PawnCapacityModifier
            {
                capacity = PawnCapacityDefOf.BloodFiltration,
                offset = factor - 1,
            };
            yield return new PawnCapacityModifier
            {
                capacity = PawnCapacityDefOf.BloodPumping,
                offset = factor - 1,
            };
            yield return new PawnCapacityModifier
            {
                capacity = PawnCapacityDefOf.Breathing,
                offset = factor - 1,
            };
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn pawn = (Pawn)((Thing)target);
            //判断目标是否是磁场强者
            if (pawn.IsHavePowerRoot())
            {
                Hediff_RWrd_PowerRoot root1 = this.parent.pawn.GetPowerRoot();
                Hediff_RWrd_PowerRoot root2 = pawn.GetPowerRoot();
                //双方磁场力量差距
                int num = root1.energy.level - root2.energy.level;
                //判断自己是否强于目标
                if (num > 0)
                {
                    int num1 = root2.energy.level + 1;
                    //削弱乘数
                    int ff = Math.Min(num, num1);
                    Hediff hediff = HediffMaker.MakeHediff(RWrd_DefOf.RWrd_HeavenLock, pawn, null);
                    /*hediff.CurStage.capMods = new List<PawnCapacityModifier>();*/
                    if (hediff.CurStage.capMods.Count == 0)
                    {
                        foreach (PawnCapacityModifier pawnCapacityModifier in CompAbilityEffect_HeavenLock.GetPCMList(-ff))
                        {
                            hediff.CurStage.capMods.Add(pawnCapacityModifier);
                        }
                    }
                    else
                    {
                        foreach (PawnCapacityModifier pcm in hediff.CurStage.capMods)
                        {
                            foreach (PawnCapacityModifier pawnCapacityModifier in CompAbilityEffect_HeavenLock.GetPCMList(-ff))
                            {
                                if (pcm.capacity == pawnCapacityModifier.capacity)
                                {
                                    pcm.offset = pawnCapacityModifier.offset;
                                }
                            }
                        }
                    }
                    try
                    {
                        //给予天锁or移除天锁
                        Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(RWrd_DefOf.RWrd_HeavenLock, false);
                        if (firstHediffOfDef != null)
                        {
                            pawn.health.RemoveHediff(firstHediffOfDef);
                        }
                        else
                        {
                            pawn.health.AddHediff(hediff);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Mistake in heaven lock ability: " + ex);
                    }
                }
                else if (num == 0)
                {
                    Messages.Message("你不能同级的磁场强者使用磁场天锁！", pawn, MessageTypeDefOf.PositiveEvent, true);
                }
                else
                {
                    Messages.Message("你不能对比自己强的磁场强者使用磁场天锁！", pawn, MessageTypeDefOf.PositiveEvent, true);
                }
            }
            else
            {
                Messages.Message("你不能对非磁场强者使用磁场天锁！", pawn, MessageTypeDefOf.PositiveEvent, true);
            }
        }
    }
}
