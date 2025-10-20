using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Electromagnetic.UI
{
    internal static class SkinTool
    {
        internal static void SetSkinColor(this Pawn p, bool primary, Color color, bool ensureVisible = false)
        {
            try
            {
                if (ensureVisible)
                {
                    bool flag = color.a <= 0f || (color.r <= 0f && color.g <= 0f && color.b <= 0f);
                    if (flag)
                    {
                        return;
                    }
                }
                bool flag3 = p != null && p.style != null;
                if (flag3)
                {
                    p.story.skinColorOverride = new Color?(color);
                }
                p.Drawer.renderer.SetAllGraphicsDirty();
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }
        internal static Color GetSkinColor(this Pawn p, bool primary)
        {
            Color result;
            bool flag2 = p != null && p.style != null;
            if (flag2)
            {
                result = p.story.SkinColor;
            }
            else
            {
                result = Color.white;
            }
            return result;
        }
    }
}
