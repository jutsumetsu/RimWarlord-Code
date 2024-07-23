using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using System.Threading.Tasks;

namespace Electromagnetic.Core
{
    public class ModDetector
    {
        /// <summary>
        /// 精英仿生框架加载检测
        /// </summary>
        public static bool EBFIsLoaded
        {
            get
            {
                return ModDetector.RunningActiveMods.Any((ModContentPack pack) => pack.Name.Contains("Elite Bionics Framework"));
            }
        }
        /// <summary>
        /// 卫生模组加载检测
        /// </summary>
        public static bool DBHIsLoaded
        {
            get
            {
                return ModDetector.RunningActiveMods.Any((ModContentPack pack) => pack.Name.Contains("Dubs Bad Hygiene"));
            }
        }
        internal static IEnumerable<ModContentPack> RunningActiveMods = from pack in LoadedModManager.RunningMods
                                                                        where pack != null && pack.ModMetaData.Active
                                                                        select pack;
    }
}
