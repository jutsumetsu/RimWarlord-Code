using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

// ============================================================================
// RaceTool.cs —— 借自「人物编辑器」(Character Editor) 的种族/基因辅助
// 用途：躯体重塑对话框用它把「身体尺寸基因」从基因列表里区分出来。
// 说明：代码主体来自人物编辑器，命名空间已改为 Electromagnetic.UI，
//       逻辑未改动；本文件仅补充注释。
// ============================================================================
namespace Electromagnetic.UI
{
    /// <summary>
    /// 种族/基因相关的只读辅助。静态类，无状态。
    /// </summary>
    internal static class RaceTool
    {
        /// <summary>
        /// 判断该基因是否为「身体尺寸」类基因（会影响 pawn 体型缩放的基因）。
        /// 空引用安全：<paramref name="g"/> 为 null 时返回 false。
        /// 真正的判定规则在 <see cref="GeneDef.IsBodySizeGene"/>，这里只做转调。
        /// </summary>
        public static bool IsBodySizeGene(this Gene g)
        {
            return g != null && g.def.IsBodySizeGene();
        }
    }
}
