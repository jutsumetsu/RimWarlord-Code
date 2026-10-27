using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

// ============================================================================
// TextureTool.cs —— 借自「人物编辑器」(Character Editor) 的贴图/渲染辅助
// 用途：改完外观后刷新 pawn 的渲染缓存；检查贴图路径是否存在并在缺失时提示。
// 说明：代码主体来自人物编辑器，命名空间已改为 Electromagnetic.UI；
//       本文件仅补充注释，逻辑未改动。
// ============================================================================
namespace Electromagnetic.UI
{
    /// <summary>
    /// 贴图与渲染相关的静态辅助方法。
    /// </summary>
    /// <remarks>
    /// 成员：
    /// <list type="bullet">
    /// <item><description><c>SetDirty(pawn)</c> —— 让 pawn 的渲染器丢弃全部图形缓存，改完外观必须调用，否则贴图不刷新。</description></item>
    /// <item><description><c>TestTexturePath(path, showError)</c> —— 用 ContentFinder 探测贴图是否存在，缺失时按需弹红色消息。</description></item>
    /// </list>
    /// </remarks>
    internal static class TextureTool
    {
        // 通知渲染器「所有图形缓存失效」，下一帧会按当前外观重新生成贴图。
        // 这是改了 hairDef / skinColor / bodyType 等之后唯一必需的刷新动作。
        public static void SetDirty(this Pawn p)
        {
            if (p != null)
            {
                Pawn_DrawTracker drawer = p.Drawer;
                if (drawer != null)
                {
                    drawer.renderer.SetAllGraphicsDirty();
                }
            }
        }
        // 探测贴图能否加载；showError=false 时静默（批量扫描/预览用）。
        // 注意 ContentFinder 第二个参数传 false，避免原版再报一次「找不到贴图」。
        internal static bool TestTexturePath(string path, bool showError = true)
        {
            Texture2D x = ContentFinder<Texture2D>.Get(path, false);
            bool flag = x == null && showError;
            if (flag)
            {
                Messages.Message("Missing Texture=" + path, MessageTypeDefOf.RejectInput);
            }
            return x != null;
        }
    }
}
