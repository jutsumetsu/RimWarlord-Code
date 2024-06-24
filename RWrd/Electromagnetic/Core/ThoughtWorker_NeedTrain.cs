using System;
using System.Collections.Generic;
using Verse;
using System.Text;
using System.Threading.Tasks;
using RimWorld;

namespace Electromagnetic.Core
{
    public class ThoughtWorker_NeedTrain : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (p.needs.TryGetNeed<Need_Training>() == null)
            {
                return ThoughtState.Inactive;
            }
            Need_Training need = p.needs.TryGetNeed<Need_Training>();
            switch (need.CurCategory)
            {
                case TrainingCategory.Desire:
                    return ThoughtState.ActiveAtStage(0);
                case TrainingCategory.Lack:
                    return ThoughtState.ActiveAtStage(1);
                case TrainingCategory.Normal:
                    return ThoughtState.Inactive;
                case TrainingCategory.Happy:
                    return ThoughtState.ActiveAtStage(2);
                case TrainingCategory.Enjoyable:
                    return ThoughtState.ActiveAtStage(3);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
