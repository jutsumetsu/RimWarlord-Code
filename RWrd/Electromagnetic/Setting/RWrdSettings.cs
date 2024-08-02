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
        public static float XpFactor
        {
            get
            {
                return ExpMultiplier / 100;
            }
        }

        public static bool NoFoodDrinkRequired;
        public static bool PowerfulPersonFragments;
        public static int ExpMultiplier;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref NoFoodDrinkRequired, "nofooddrinkrequired", true);
            Scribe_Values.Look(ref PowerfulPersonFragments, "powerfulpersonfragments");

            Scribe_Values.Look(ref ExpMultiplier, "expmultiplier", 100);
            base.ExposeData();
        }
    }
}
