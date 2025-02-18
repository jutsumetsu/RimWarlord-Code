using Electromagnetic.Core;
using Electromagnetic.HarmonyPatchs;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static Electromagnetic.HarmonyPatchs.HarmonyInit;

namespace Electromagnetic.Effect
{
    public class GameComponent_MFEnvironment : GameComponent
    {
        public GameComponent_MFEnvironment(Game game) 
        {
        }
        public override void FinalizeInit()
        {
            base.FinalizeInit();
            if (ModDetector.CEIsLoaded)
            {
                DoCEPatch();
            }
        }
        private void DoCEPatch()
        {
            PatchMain.instance.Patch(
                AccessTools.Method("ay:a", new[]
                {
                    typeof(Pawn),
                    typeof(TraitDef),
                    typeof(TraitDegreeData),
                    typeof(bool),
                    typeof(bool),
                    typeof(Trait)
                }),
                postfix: new HarmonyMethod(typeof(Pawn_Patch), "AddTrait_Postfix"));
        }
    }
}
