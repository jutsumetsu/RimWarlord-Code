using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Electromagnetic.UI
{
    internal class Listing_X : Listing
    {
        internal float CurY
        {
            get
            {
                return this.curY;
            }
            set
            {
                this.curY = value;
            }
        }
        internal float Slider(float val, float min, float max, Color color)
        {
            GUI.color = color;
            float result = Widgets.HorizontalSlider(base.GetRect(22f, 1f), val, min, max, false, null, null, null, -1f);
            base.Gap(this.verticalSpacing);
            return result;
        }
        internal float Slider(float val, float min, float max)
        {
            float result = Widgets.HorizontalSlider(base.GetRect(22f, 1f), val, min, max, false, null, null, null, -1f);
            base.Gap(this.verticalSpacing);
            return result;
        }
        internal void Label(string text, float width = -1f, float yGap = -1f, float maxHeight = -1f, string tooltip = null)
        {
            float num = Text.CalcHeight(text, base.ColumnWidth);
            bool flag = false;
            bool flag2 = maxHeight >= 0f && num > maxHeight;
            if (flag2)
            {
                num = maxHeight;
                flag = true;
            }
            Rect rect = base.GetRect(num, 1f);
            bool flag3 = width >= 0f;
            if (flag3)
            {
                rect.width = width;
            }
            bool flag4 = flag;
            if (flag4)
            {
                Vector2 labelScrollbarPosition = this.GetLabelScrollbarPosition(this.curX, this.curY);
                Widgets.LabelScrollable(rect, text, ref labelScrollbarPosition, false, true, false);
                this.SetLabelScrollbarPosition(this.curX, this.curY, labelScrollbarPosition);
            }
            else
            {
                Widgets.Label(rect, text);
            }
            bool flag5 = tooltip != null;
            if (flag5)
            {
                TooltipHandler.TipRegion(rect, tooltip);
            }
            bool flag6 = yGap == -1f;
            if (flag6)
            {
                base.Gap(this.verticalSpacing);
            }
            else
            {
                base.Gap(yGap);
            }
        }
        private Vector2 GetLabelScrollbarPosition(float x, float y)
        {
            bool flag = this.labelScrollbarPositions == null;
            Vector2 zero;
            if (flag)
            {
                zero = Vector2.zero;
            }
            else
            {
                for (int i = 0; i < this.labelScrollbarPositions.Count; i++)
                {
                    Vector2 first = this.labelScrollbarPositions[i].First;
                    bool flag2 = first.x == x && first.y == y;
                    if (flag2)
                    {
                        return this.labelScrollbarPositions[i].Second;
                    }
                }
                zero = Vector2.zero;
            }
            return zero;
        }
        private void SetLabelScrollbarPosition(float x, float y, Vector2 scrollbarPosition)
        {
            bool flag = this.labelScrollbarPositions == null;
            if (flag)
            {
                this.labelScrollbarPositions = new List<Pair<Vector2, Vector2>>();
                this.labelScrollbarPositionsSetThisFrame = new List<Vector2>();
            }
            this.labelScrollbarPositionsSetThisFrame.Add(new Vector2(x, y));
            for (int i = 0; i < this.labelScrollbarPositions.Count; i++)
            {
                Vector2 first = this.labelScrollbarPositions[i].First;
                bool flag2 = first.x == x && first.y == y;
                if (flag2)
                {
                    this.labelScrollbarPositions[i] = new Pair<Vector2, Vector2>(new Vector2(x, y), scrollbarPosition);
                    return;
                }
            }
            this.labelScrollbarPositions.Add(new Pair<Vector2, Vector2>(new Vector2(x, y), scrollbarPosition));
        }
        private List<Pair<Vector2, Vector2>> labelScrollbarPositions;
        private List<Vector2> labelScrollbarPositionsSetThisFrame;
    }
}
