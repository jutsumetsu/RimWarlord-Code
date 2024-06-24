using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Electromagnetic.Core
{
    public class Need_Training : Need
    {
        public override int GUIChangeArrow
        {
            get
            {
                return 0;
            }
        }
        public override bool ShowOnNeedList
        {
            get
            {
                return !this.Disabled;
            }
        }
        private bool Disabled
        {
            get
            {
                return !this.pawn.IsHaveRoot();
            }
        }
        public Need_Training(Pawn pawn) : base(pawn)
        {
            this.threshPercents = new List<float>();
            this.threshPercents.Add(0.1f);
        }
        public override void SetInitialLevel()
        {
            this.CurLevel = 0.5f;
        }
        public override void NeedInterval()
        {
            if (this.Disabled)
            {
                this.SetInitialLevel();
                return;
            }
            if (this.IsFrozen)
            {
                return;
            }
        }
    }
}
