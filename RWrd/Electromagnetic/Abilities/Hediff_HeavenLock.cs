using Electromagnetic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Electromagnetic.Abilities
{
    public class Hediff_HeavenLock : Hediff_TargetBase
    {
        public override void PostMake()
        {
            base.PostMake();
            this.casterLevel = root.energy.level;
            this.casterCompleteRealm = root.energy.completerealm;
        }
        public override void PostRemoved()
        {
            base.PostRemoved();
            this.pawn.GetPowerRoot().energy.powerLimit = 999;
            this.pawn.UpdatePowerRootStageInfo();
        }
        private void RemoveThis()
        {
            this.pawn.health.RemoveHediff(this);
        }
        public virtual void Notify_PawnDied()
        {
            this.RemoveThis();
        }
        public override void Tick()
        {
            base.Tick();
            Pawn pawn = this.pawn;
            bool flag = pawn == null;
            if (!flag)
            {
                bool dead = pawn.Dead;
                Pawn_EnergyTracker energy1 = pawn.GetPowerRoot().energy;
                Pawn_EnergyTracker energy2 = this.root.energy;

                if (dead)
                {
                    this.RemoveThis();
                }
                if (Find.TickManager.TicksGame % 180 == 0)
                {
                    bool ultimate = energy1.IsUltimate;
                    bool breakthrough = energy1.level >= energy2.level && energy1.completerealm >= energy2.completerealm && !this.surpriseAttack;
                    if (ultimate || breakthrough)
                    {
                        this.RemoveThis();
                    }
                }
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.surpriseAttack, "surpriseAttackLock", false, false);
            Scribe_Values.Look<int>(ref this.casterLevel, "casterLevel", 0, false);
            Scribe_Values.Look<float>(ref this.casterCompleteRealm, "casterCompleteRealm", 0, false);
        }

        public bool surpriseAttack;
        public int casterLevel;
        public float casterCompleteRealm;
    }
}
