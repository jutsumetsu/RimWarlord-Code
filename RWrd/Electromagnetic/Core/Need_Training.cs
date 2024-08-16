using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

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
                else if (this.IsTraining || this.IsBattle)
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
        /// <summary>
        /// 不可用
        /// </summary>
        private bool Disabled
        {
            get
            {
                return !this.pawn.IsHaveRoot() || !this.pawn.Spawned;
            }
        }
        /// <summary>
        /// 是否在练功
        /// </summary>
        private bool IsTraining
        {
            get
            {
                return this.pawn.jobs.curDriver.GetType() == typeof(JobDriver_RWrd_General_Training) || this.pawn.jobs.curDriver.GetType() == typeof(JobDriver_RWrd_Specialized_Training);
            }
        }
        /// <summary>
        /// 是否在打交
        /// </summary>
        private bool IsBattle
        {
            get
            {
                return this.pawn.jobs.curDriver.GetType() == typeof(JobDriver_AttackMelee);
            }
        }
        public Need_Training(Pawn pawn) : base(pawn)
        {
            this.threshPercents = new List<float>
            {
                0.1f
            };
            this.SetInitialLevel();
        }
        /// <summary>
        /// 设置初始等级
        /// </summary>
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
                    JobDriver jobDriver = this.pawn.jobs.curDriver;
                    if (!this.IsTraining && !this.IsBattle)
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
