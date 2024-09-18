using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Electromagnetic.Core;
using Verse;

namespace Electromagnetic.Abilities
{

    public class HediffComp_AntiGravity : HediffComp
    {
        public float EnergyMax
        {
            get
            {
                Pawn pawn = Pawn;
                if (pawn.IsHavePowerRoot())
                {
                    Hediff_RWrd_PowerRoot root = pawn.GetPowerRoot();
                    return root.energy.PowerFlow;
                }

                return 100;
            }
        }
        public float Energy
        {
            get
            {
                Pawn pawn = Pawn;
                if (pawn.IsHavePowerRoot())
                {
                    Hediff_RWrd_PowerRoot root = pawn.GetPowerRoot();
                    return root.energy.energy;
                }

                return 0;
            }
        }
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            AntiGravityUtils.AntiGravityPawns.Add(parent.pawn);
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            parent.pawn.pather.TryRecoverFromUnwalkablePosition(false);
            AntiGravityUtils.AntiGravityPawns.Remove(parent.pawn);
        }
    }
}
