using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Electromagnetic.UI
{
    public class Dialog_EditAbilitySets : Window
    {
        public Dialog_EditAbilitySets(ITab_Pawn_RWrd parent)
        {
            this.parent = parent;
            doCloseX = true;
            closeOnClickedOutside = true;
            forcePause = true;
            doCloseButton = true;
        }
        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(this.parent.Size.x * 0.3f, Mathf.Max(300f, this.NeededHeight));
            }
        }
        private float NeededHeight
        {
            get
            {
                return this.parent.RequestedAbilitySetsHeight + this.Margin * 2f;
            }
        }
        public override void DoWindowContents(Rect inRect)
        {
            this.parent.DoAbilitySets(inRect);
            bool flag = this.windowRect.height < this.NeededHeight;
            if (flag)
            {
                this.windowRect.height = this.NeededHeight;
            }
        }
        private readonly ITab_Pawn_RWrd parent;
    }
}
