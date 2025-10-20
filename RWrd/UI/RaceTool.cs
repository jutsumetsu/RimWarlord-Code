using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Electromagnetic.UI
{
    internal static class RaceTool
    {
        public static bool IsBodySizeGene(this Gene g)
        {
            return g != null && g.def.IsBodySizeGene();
        }
    }
}
