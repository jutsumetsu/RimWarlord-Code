using Electromagnetic.Core;
using Electromagnetic.HarmonyPatchs;
using Electromagnetic.UI;
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
            AtomSplitInitialize();
        }
        private void AtomSplitInitialize()
        {
            PowerRootUtillity.spawnableThings = Dialog_SelectThings.AllThingsDefs.ToList();
        }
    }
}
