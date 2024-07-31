using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Electromagnetic.Setting
{
    public class RWrdSettings : ModSettings
    {
        public static bool PowerfulPersonFragments;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref PowerfulPersonFragments, "powerfulpersonfragments");
            base.ExposeData();
        }
    }
}
