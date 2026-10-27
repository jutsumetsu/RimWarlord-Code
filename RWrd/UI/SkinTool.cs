using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

// ============================================================================
// SkinTool.cs —— 借自「人物编辑器」(Character Editor) 的皮肤色辅助
// 用途：读写 pawn 皮肤色；写入走 story.skinColorOverride，写后刷新渲染。
// 说明：代码主体来自人物编辑器，命名空间已改为 Electromagnetic.UI；
//       primary 参数是为「人物编辑器」的多部位配色预留的，这里人类只用到一套皮肤色。
// ============================================================================
namespace Electromagnetic.UI
{
    /// <summary>
    /// 皮肤颜色的读写辅助。静态类，无状态。
    /// </summary>
    internal static class SkinTool
    {
        // 参数 primary 目前不参与分支：人类只有一套皮肤色，直接写 skinColorOverride。
        // ensureVisible=true 时会拒绝全透明或纯黑，避免把小人刷成看不见的状态。
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
        // 取「最终生效」的皮肤色（含 skinColorOverride 与基因/年龄修正），无数据时返回白色。
        // 这里判空的是 p.style、取值却来自 p.story，是人物编辑器原写法，保留不改。
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
