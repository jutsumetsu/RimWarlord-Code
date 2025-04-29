using System;
using System.Reflection;
using Electromagnetic.Core;
using HarmonyLib;
using Verse;

namespace Electromagnetic.HarmonyPatchs
{
    internal class HarmonyInit
    {
        [StaticConstructorOnStartup]
        public class PatchMain
        {
            static PatchMain()
            {
                HarmonyInit.PatchMain.instance.PatchAll(Assembly.GetExecutingAssembly());
                AntiGravityPatches.Do(PatchMain.instance);
            }
            public static Harmony instance = new Harmony("RWrd.Harmony");
        }
    }
}
