using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Electromagnetic.UI
{
    public static class CompatibilityTool
    {
        public static string GetFavoriteColorTooltip(Pawn pawn)
        {
            string result = "";
            try
            {
                string arg = string.Empty;
                bool flag = pawn.Ideo != null && !pawn.Ideo.hidden;
                if (flag)
                {
                    arg = "OrIdeoColor".Translate(pawn.Named("PAWN"));
                }
                result = "FavoriteColorTooltip".Translate(pawn.Named("PAWN"), pawn.story.favoriteColor.label.Named("COLOR"), 0.6f.ToStringPercent().Named("PERCENTAGE"), arg.Named("ORIDEO")).Resolve();
            }
            catch
            {
            }
            return result;
        }
    }
}
