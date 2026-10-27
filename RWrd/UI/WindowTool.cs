using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

// ============================================================================
// WindowTool.cs —— 借自「人物编辑器」(Character Editor) 的窗口辅助
// 用途：统一打开/关闭窗口、把窗口提到最前、计算标准按钮位置与窗口最大高度。
// 注意：SimpleCloseButton 依赖 Dialog_BodyRemold.ButtonText（借来时共用的一套
//       UI 原语），所以本文件与 Dialog_BodyRemold.cs 是相互引用的，改签名要两边一起改。
// ============================================================================
namespace Electromagnetic.UI
{
    /// <summary>
    /// 窗口/布局静态辅助。静态类，无状态。
    /// </summary>
    internal static class WindowTool
    {
        // 统一以 Dialog 层打开，避免窗口被其它面板挡住；调用方不必自己设 layer。
        internal static void Open(Window w)
        {
            w.layer = WindowLayer.Dialog;
            Find.WindowStack.Add(w);
        }
        // 其余成员说明：
        //   TryRemove<T>()          —— 从 WindowStack 移除指定类型窗口（force=true 立即关闭）；
        //   SimpleCloseButton(w)    —— 在标准按钮位置画「关闭」，点击后 Close(true)；
        //   RAcceptButton(w)        —— 由窗口 InitialSize 推算按钮矩形（右下角内缩 136/66）；
        //   X_Accept / Y_Accept     —— 上面的两个坐标；
        //   MaxH                    —— 按屏幕高度给窗口高度封顶（<768 用屏高，<1200 用 768，否则 800）；
        //   TopLayerForWindowOf<T>  —— 找到指定类型窗口并提到最前；
        //   BringToFront(w, force)  —— layer 不是 Dialog 时先改层再提；force 时无条件提。
        internal static void TryRemove<T>() where T : Window
        {
            Find.WindowStack.TryRemove(typeof(T), true);
        }
        internal static void SimpleCloseButton(Window w)
        {
            UIPrimitives.ButtonText(WindowTool.RAcceptButton(w), "Close".Translate(), delegate ()
            {
                w.Close(true);
            }, "");
        }
        internal static Rect RAcceptButton(Window w)
        {
            return new Rect((float)WindowTool.X_Accept(w), (float)WindowTool.Y_Accept(w), 100f, 30f);
        }
        internal static int X_Accept(Window w)
        {
            return (int)w.InitialSize.x - 136;
        }
        internal static int Y_Accept(Window w)
        {
            return (int)w.InitialSize.y - 66;
        }
        internal static int MaxH
        {
            get
            {
                return (Verse.UI.screenHeight < 768) ? Verse.UI.screenHeight : ((Verse.UI.screenHeight < 1200) ? 768 : 800);
            }
        }
        internal static void TopLayerForWindowOf<T>(bool force) where T : Window
        {
            Window w = Find.WindowStack.WindowOfType<T>();
            WindowTool.BringToFront(w, force);
        }
        internal static void BringToFront(Window w, bool force)
        {
            bool flag = w != null;
            if (flag)
            {
                bool flag2 = w.layer != WindowLayer.Dialog;
                if (flag2)
                {
                    w.layer = WindowLayer.Dialog;
                    GUI.BringWindowToFront(w.ID);
                }
                else if (force)
                {
                    GUI.BringWindowToFront(w.ID);
                }
            }
        }
    }
}
