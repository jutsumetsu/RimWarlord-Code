using Electromagnetic.Core;
using Electromagnetic.UI;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Electromagnetic.Abilities
{
    public class CompAbilityEffect_​BodyRemold : CompAbilityEffect_Electromagnetic
    {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn pawn = (Pawn)((Thing)target);
            if (pawn == Caster)
            {
                Find.WindowStack.Add(new Dialog_BodyRemold(pawn, this.Caster.GetPowerRoot()));
            }
            else
            {
                Hediff_RWrd_PowerRoot root1 = Caster.GetPowerRoot();
                if (root1.energy.AvailableLevel < 90)
                {
                    Messages.Message("RWrd_RemoldErrorWeak".Translate(), pawn, MessageTypeDefOf.PositiveEvent, true);
                }
                else
                {
                    Hediff_RWrd_PowerRoot root2;
                    if (pawn.IsHavePowerRoot())
                    {
                        root2 = pawn.GetPowerRoot();
                    }
                    else
                    {
                        root2 = null;
                    }
                     if (pawn.Faction != Caster.Faction)
                    {
                        if (root2 == null)
                        {
                            Messages.Message("RWrd_RemoldErrorNon".Translate(), pawn, MessageTypeDefOf.PositiveEvent, true);
                        }
                        else if (root1.energy.AvailableLevel <= root2.energy.AvailableLevel)
                        {
                            Messages.Message("RWrd_LockedErrorWeak".Translate(), pawn, MessageTypeDefOf.PositiveEvent, true);
                        }
                        else
                        {
                            Find.WindowStack.Add(new Dialog_BodyRemold(pawn, this.Caster.GetPowerRoot()));
                        }
                    }
                    else
                    {
                        if (root2 == null)
                        {
                            Find.WindowStack.Add(new Dialog_BodyRemold(pawn, this.Caster.GetPowerRoot()));
                        }
                        else if (root1.energy.AvailableLevel <= root2.energy.AvailableLevel)
                        {
                            Messages.Message("RWrd_LockedErrorWeak".Translate(), pawn, MessageTypeDefOf.PositiveEvent, true);
                        }
                        else
                        {
                            Find.WindowStack.Add(new Dialog_BodyRemold(pawn, this.Caster.GetPowerRoot()));
                        }
                    }
                }
            }
        }
    }
}
