using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Electromagnetic.Core
{
    public class Need_Training : Need
    {
        public TrainingCategory CurCategory
        {
            get
            {
                if (this.CurLevel < 0.1f)
                {
                    return TrainingCategory.Desire;
                }
                if (this.CurLevel < 0.3f)
                {
                    return TrainingCategory.Lack;
                }
                if (this.CurLevel < 0.8f)
                {
                    return TrainingCategory.Normal;
                }
                if (this.CurLevel < 0.9f)
                {
                    return TrainingCategory.Happy;
                }
                return TrainingCategory.Enjoyable;
            }
        }
        public override int GUIChangeArrow
        {
            get
            {
                if (this.Disabled)
                {
                    return 0;
                }
                else if (this.IsTraining)
                {
                    return 1;
                }
                return -1;
            }
        }
        public override bool ShowOnNeedList
        {
            get
            {
                return !this.Disabled;
            }
        }
        //不可用
        private bool Disabled
        {
            get
            {
                return !this.pawn.IsHaveRoot();
            }
        }
        private bool IsTraining
        {
            get
            {
                return this.pawn.jobs.curDriver.GetType() == typeof(JobDriver_RWrd_General_Training) || this.pawn.jobs.curDriver.GetType() == typeof(JobDriver_RWrd_Specialized_Training);
            }
        }
        public Need_Training(Pawn pawn) : base(pawn)
        {
            this.threshPercents = new List<float>();
            this.threshPercents.Add(0.1f);
            this.SetInitialLevel();
        }
        //初始等级
        public override void SetInitialLevel()
        {
            this.CurLevel = 0.5f;
        }
        public override void NeedInterval()
        {
            if (this.Disabled)
            {
                return;
            }
            if (this.IsFrozen)
            {
                return;
            }
            else
            {
                if (!this.Disabled)
                {
                    if (!this.IsTraining)
                    {
                        Hediff_RWrd_PowerRoot root = this.pawn.GetRoot();
                        float num = 0.00005f * root.energy.trainDesireFactor;
                        this.CurLevel -= num;
                    }
                }
            }
        }
    }
}
