using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Electromagnetic.UI
{
    public static class UIUtility
    {
        /// <summary>
        /// 取矩形顶部
        /// </summary>
        /// <param name="rect">目标矩形</param>
        /// <param name="pixels">高度</param>
        /// <returns></returns>
        public static Rect TakeTopPart(this Rect rect, float pixels)
        {
            Rect result = rect.TopPartPixels(pixels);
            rect.yMin += pixels;
            return result;
        }
        /// <summary>
        /// 取矩形底部
        /// </summary>
        /// <param name="rect">目标矩形</param>
        /// <param name="pixels">高度</param>
        /// <returns></returns>
        public static Rect TakeBottomPart(this Rect rect, float pixels)
        {
            Rect result = rect.BottomPartPixels(pixels);
            rect.yMax -= pixels;
            return result;
        }
        /// <summary>
        /// 取矩形右部
        /// </summary>
        /// <param name="rect">目标矩形</param>
        /// <param name="pixels">宽度</param>
        /// <returns></returns>
        public static Rect TakeRightPart(this Rect rect, float pixels)
        {
            Rect result = rect.RightPartPixels(pixels);
            rect.xMax -= pixels;
            return result;
        }
        /// <summary>
        /// 取矩形左部
        /// </summary>
        /// <param name="rect">目标矩形</param>
        /// <param name="pixels">宽度</param>
        /// <returns></returns>
        public static Rect TakeLeftPart(this Rect rect, float pixels)
        {
            Rect result = rect.LeftPartPixels(pixels);
            rect.xMin += pixels;
            return result;
        }

        // Token: 0x06000A52 RID: 2642 RVA: 0x0004A07C File Offset: 0x0004827C
        public static void DrawCountAdjuster(ref int value, Rect inRect, ref string buffer, int min, int max, bool readOnly = false, int? setToMin = null, int? setToMax = null)
        {
            int num = value;
            Rect rect = inRect.ContractedBy(50f, 0f);
            Rect rect2 = rect.LeftPartPixels(30f);
            rect.xMin += 30f;
            Rect rect3 = rect.LeftPartPixels(30f);
            rect.xMin += 30f;
            Rect rect4 = rect.RightPartPixels(30f);
            rect.xMax -= 30f;
            Rect rect5 = rect.RightPartPixels(30f);
            rect.xMax -= 30f;
            int num2 = GenUI.CurrentAdjustmentMultiplier();
            bool flag = !readOnly && ((setToMin != null) ? (value > setToMin.Value) : (value != min)) && Widgets.ButtonText(rect2, "<<", true, true, true, null);
            if (flag)
            {
                value = (setToMin ?? min);
            }
            bool flag2 = !readOnly && value - num2 >= min && Widgets.ButtonText(rect3, "<", true, true, true, null);
            if (flag2)
            {
                value -= num2;
            }
            bool flag3 = !readOnly && ((setToMax != null) ? (value < setToMax.Value) : (value != max)) && Widgets.ButtonText(rect4, ">>", true, true, true, null);
            if (flag3)
            {
                value = (setToMax ?? max);
            }
            bool flag4 = !readOnly && value + num2 <= max && Widgets.ButtonText(rect5, ">", true, true, true, null);
            if (flag4)
            {
                value += num2;
            }
            bool flag5 = value < min;
            if (flag5)
            {
                value = min;
            }
            bool flag6 = value > max;
            if (flag6)
            {
                value = max;
            }
            bool flag7 = value != num || readOnly;
            if (flag7)
            {
                buffer = value.ToString();
            }
            Widgets.TextFieldNumeric<int>(rect.ContractedBy(3f, 0f), ref num, ref buffer, (float)min, (float)max);
            bool flag8 = !readOnly;
            if (flag8)
            {
                value = num;
            }
        }

        // Token: 0x06000A53 RID: 2643 RVA: 0x0004A2B3 File Offset: 0x000484B3
        public static IEnumerable<Rect> Divide(Rect rect, int items, int columns = 0, int rows = 0, bool drawLines = true)
        {
            bool flag = columns == 0 && rows == 0;
            if (flag)
            {
                bool flag2 = !Mathf.Approximately(rect.width, rect.height);
                if (flag2)
                {
                    throw new ArgumentException("Provided rect is not square!");
                }
                int perSide = (int)Math.Ceiling(Math.Sqrt((double)items));
                rows = perSide;
                columns = perSide;
            }
            bool flag3 = rows == 0;
            if (flag3)
            {
                rows = (int)Math.Ceiling((double)items / (double)columns);
            }
            else
            {
                bool flag4 = columns == 0;
                if (flag4)
                {
                    columns = (int)Math.Ceiling((double)items / (double)rows);
                }
            }
            Vector2 curLoc = new Vector2(rect.xMin, rect.yMin);
            Vector2 size = new Vector2(rect.width / (float)columns, rect.height / (float)rows);
            Color color = Color.gray;
            int num;
            for (int i = 0; i < columns; i = num + 1)
            {
                for (int j = 0; j < rows; j = num + 1)
                {
                    yield return new Rect(curLoc, size).ContractedBy(1f);
                    curLoc.y += size.y;
                    bool flag5 = drawLines && i == 0 && j < rows - 1;
                    if (flag5)
                    {
                        Widgets.DrawLine(curLoc, new Vector2(rect.xMax, curLoc.y), color, 1f);
                    }
                    num = j;
                }
                curLoc.x += size.x;
                curLoc.y = rect.yMin;
                bool flag6 = drawLines && i < columns - 1;
                if (flag6)
                {
                    Widgets.DrawLine(new Vector2(curLoc.x, curLoc.y + 2f), new Vector2(curLoc.x, rect.yMax), color, 1f);
                }
                num = i;
            }
            yield break;
        }
    }
}
