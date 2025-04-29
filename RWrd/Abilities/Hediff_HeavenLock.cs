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
            this.casterLevel = root.energy.level + root.energy.FinalLevel;
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
            PowerRootUtillity.lockedCacheMap.Remove(this.pawn);
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

                if (dead)
                {
                    this.RemoveThis();
                }
                if (Find.TickManager.TicksGame % 180 == 0)
                {
                    bool ultimate = energy1.IsUltimate;
                    bool breakthrough = (energy1.level + energy1.FinalLevel) >= this.casterLevel && energy1.completerealm >= this.casterCompleteRealm && !this.surpriseAttack;
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
            bool flag = Scribe.mode == LoadSaveMode.PostLoadInit;
            if (flag)
            {
                if (PowerRootUtillity.lockedCacheMap == default(Dictionary<Pawn, Hediff_HeavenLock>))
                {
                    PowerRootUtillity.lockedCacheMap = new Dictionary<Pawn, Hediff_HeavenLock>();
                }
                if (!PowerRootUtillity.lockedCacheMap.ContainsKey(pawn))
                {
                    PowerRootUtillity.lockedCacheMap.Add(this.pawn, this);
                }
            }
        }

        public bool surpriseAttack;
        public int casterLevel;
        public float casterCompleteRealm;
    }
}
