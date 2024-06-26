using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using System.Threading.Tasks;

namespace Electromagnetic.Core
{
    public class ModDetector
    {
        public static bool EBFIsLoaded
        {
            get
            {
                return ModDetector.RunningActiveMods.Any((ModContentPack pack) => pack.Name.Contains("Elite Bionics Framework"));
            }
        }
        internal static IEnumerable<ModContentPack> RunningActiveMods = from pack in LoadedModManager.RunningMods
                                                                        where pack != null && pack.ModMetaData.Active
                                                                        select pack;
    }
}
