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
                return ModDetect("Elite Bionics Framework", "v1024.ebframework");
            }
        }
        /// <summary>
        /// Save Our Ship 2加载检测
        /// </summary>
        public static bool SOSIsLoaded
        {
            get
            {
                return ModDetect("Save Our Ship", "kentington.saveourship2");
            }
        }
        /// <summary>
        /// 寻路框架加载检测
        /// </summary>
        public static bool PFIsLoaded
        {
            get
            {
                return ModDetect("Pathfinding Framework", "pathfinding.framework");
            }
        }
        /// <summary>
        /// 人物编辑器加载检测
        /// </summary>
        public static bool CEIsLoaded
        {
            get
            {
                return ModDetect("Character Editor", "void.charactereditor");
            }
        }
        /// <summary>
        /// 卫生模组加载检测
        /// </summary>
        public static bool DBHIsLoaded
        {
            get
            {
                return ModDetect("Dubs Bad Hygiene", "dubwise.dubsbadhygiene", "dubwise.dubsbadhygiene.lite");
            }
        }
        /// <summary>
        /// 卫生模组口渴启用
        /// </summary>
        public static bool DBHThirstExist
        {
            get
            {
                return ModDetect(null, "dubwise.dubsbadhygiene", "dubwise.dubsbadhygiene.thirst");
            }
        }
        public static bool ModDetect(string packName = null, params string[] packIDs)
        {
            bool nameFlag = false;
            bool idFlag = false;
            if (packName != null)
            {
                nameFlag = ModDetector.RunningActiveMods.Any((ModContentPack pack) => pack.Name.Contains(packName));
            }
            if (packIDs != null && packIDs.Length > 0)
            {
                idFlag = ModDetector.RunningActiveMods.Any(pack => packIDs.Contains(pack.PackageId));
            }
            bool result = nameFlag || idFlag;
            return result;
        }
        internal static IEnumerable<ModContentPack> RunningActiveMods = from pack in LoadedModManager.RunningMods
                                                                        where pack != null && pack.ModMetaData.Active
                                                                        select pack;
    }
}
