using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Electromagnetic.UI
{
    internal static class WindowTool
    {
        internal static void Open(Window w)
        {
            w.layer = WindowLayer.Dialog;
            Find.WindowStack.Add(w);
        }
        internal static void TryRemove<T>() where T : Window
        {
            Find.WindowStack.TryRemove(typeof(T), true);
        }
        internal static void SimpleCloseButton(Window w)
        {
            Dialog_​BodyRemold.ButtonText(WindowTool.RAcceptButton(w), "Close".Translate(), delegate ()
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
