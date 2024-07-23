using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Electromagnetic.Core
{
    /// <summary>
    /// 工具箱
    /// </summary>
    [StaticConstructorOnStartup]
    public class Tools
    {
        /// <summary>
        /// 检测中文语言
        /// </summary>
        public static bool IsChineseLanguage
        {
            get
            {
                return LanguageDatabase.activeLanguage.ToString() == "Simplified Chinese" || LanguageDatabase.activeLanguage.ToString() == "Traditional Chinese";
            }
        }
        /// <summary>
        /// 角度工具
        /// </summary>
        /// <param name="p1">目标点</param>
        /// <param name="p2">原点</param>
        /// <returns></returns>
        public static double PointsAngleTool(IntVec3 p1, IntVec3 p2)
        {
            return Math.Atan2((double)(p2.x - p1.x), (double)(p2.z - p1.z)) * 180.0 / 3.141592653589793;
        }
    }
}
