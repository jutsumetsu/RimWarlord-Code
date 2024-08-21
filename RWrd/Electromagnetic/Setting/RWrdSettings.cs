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

        public static bool EnableExpCorrectionFactor = true;
        public static bool NoFoodDrinkRequired = true;
        public static bool PowerfulPersonFragments = false;
        public static bool PowerfulEnergyWave = true;
        public static bool DoVisualWaveEffect = true;
        public static int ExpMultiplier = 100;
        public static int GlobalLevelLimit = 99;
        public static float WaveRangeFactor = 1;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref EnableExpCorrectionFactor, "enableexpcorrectionfactor", true);
            Scribe_Values.Look(ref NoFoodDrinkRequired, "nofooddrinkrequired", true);
            Scribe_Values.Look(ref PowerfulPersonFragments, "powerfulpersonfragments", false);
            Scribe_Values.Look(ref PowerfulEnergyWave, "powerfulenergywave", true);
            Scribe_Values.Look(ref DoVisualWaveEffect, "dovisualwaveeffect", true);

            Scribe_Values.Look(ref ExpMultiplier, "expmultiplier", 100);
            Scribe_Values.Look(ref GlobalLevelLimit, "globallevellimit", 99);
            Scribe_Values.Look(ref WaveRangeFactor, "waverangefactor", 99);
            base.ExposeData();
        }
    }
}
