using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Electromagnetic.UI
{
    internal static class TextureTool
    {
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
