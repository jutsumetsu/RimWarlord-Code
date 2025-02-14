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
        private Pawn Caster
        {
            get
            {
                return this.parent.pawn;
            }
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn pawn = (Pawn)((Thing)target);
            if (!Caster.IsLockedByEMPower())
            {
                //判断目标是否是磁场强者
                if (pawn.IsHavePowerRoot())
                {
                    Hediff_RWrd_PowerRoot root1 = Caster.GetPowerRoot();
                    Hediff_RWrd_PowerRoot root2 = pawn.GetPowerRoot();
                    // 判断是否为终极强者
                    if (!root2.energy.IsUltimate)
                    {
                        //双方磁场力量差距
                        int level1 = root1.energy.level + root1.energy.FinalLevel;
                        int level2 = root2.energy.level + root2.energy.FinalLevel;
                        int num = level1 - level2;
                        float num2 = root1.energy.completerealm - root2.energy.completerealm;
                        //判断自己是否强于目标
                        if (num > 0 && num2 > 0)
                        {
                            Find.WindowStack.Add(new Dialog_HeavenLock(Caster, pawn));
                        }
                        else if (pawn.Downed)
                        {
                            Find.WindowStack.Add(new Dialog_HeavenLock(Caster, pawn, true));
                        }
                        else
                        {
                            Messages.Message("RWrd_LockedErrorWeak".Translate(), pawn, MessageTypeDefOf.PositiveEvent, true);
                        }
                    }
                    else
                    {
                        Messages.Message("RWrd_LockedErrorUltimate".Translate(), pawn, MessageTypeDefOf.PositiveEvent, true);
                    }
                }
                else
                {
                    Messages.Message("RWrd_LockedErrorNon".Translate(), pawn, MessageTypeDefOf.PositiveEvent, true);
                }
            }
            else
            {
                Messages.Message("RWrd_LockedErrorRestricted".Translate(), pawn, MessageTypeDefOf.PositiveEvent, true);
            }
        }
    }
}
