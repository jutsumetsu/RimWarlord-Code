﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Electromagnetic.Abilities
{
    public class RWrd_AbilityBase : Ability
    {
        public RWrd_AbilityBase(Pawn pawn) : base(pawn) { }
        public RWrd_AbilityBase(Pawn pawn, AbilityDef def) : base(pawn, def) { }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref this.mastery, "mastery", 0f, false);
            Scribe_Values.Look<float>(ref this.outputPower, "outputpowerability", 0f, false);
        }
        /// <summary>
        /// 设置精通值
        /// </summary>
        /// <param name="num">数值</param>
        public void SetMastery(float num)
        {
            if (num > 0)
            {
                float num2 = this.mastery + num;
                this.mastery = (num2 > this.MaxMastery ? this.MaxMastery : num2);
            }
        }
        public float mastery = 0;
        public float outputPower = 1;
        public float MaxMastery = 100;
    }
}
