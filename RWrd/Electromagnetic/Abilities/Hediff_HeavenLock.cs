using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Electromagnetic.Abilities
{
    public class Hediff_HeavenLock : HediffWithComps
    {
        public override void PostMake()
        {
            base.PostMake();
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
                if (dead)
                {
                    this.RemoveThis();
                }
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
        }
    }
}
