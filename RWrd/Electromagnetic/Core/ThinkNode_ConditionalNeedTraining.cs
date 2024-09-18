using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Electromagnetic.Core
{
    public class ThinkNode_ConditionalNeedTraining : ThinkNode_Conditional
    {
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            ThinkNode_ConditionalNeedTraining thinkNode_ConditionalNeedTraining = (ThinkNode_ConditionalNeedTraining)base.DeepCopy(resolve);
            thinkNode_ConditionalNeedTraining.need = this.need;
            thinkNode_ConditionalNeedTraining.threshold = this.threshold;
            return thinkNode_ConditionalNeedTraining;
        }
        protected override bool Satisfied(Pawn pawn)
        {
            Need need = pawn.needs.TryGetNeed(this.need);
            if (need == null)
            {
                return false;
            }
            if (!pawn.IsHavePowerRoot())
            {
                return false;
            }
            if (need is Need_Seeker)
            {
                return need.CurInstantLevelPercentage > this.threshold;
            }
            return need.CurLevelPercentage > this.threshold;
        }
        private NeedDef need;
        private float threshold;
    }
}
