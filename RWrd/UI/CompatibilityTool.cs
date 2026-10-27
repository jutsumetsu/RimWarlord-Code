using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

// ============================================================================
// CompatibilityTool.cs —— 借自「人物编辑器」(Character Editor) 的兼容性辅助
// 用途：生成「最喜欢的颜色」提示文本（有意识形态时附带意识形态颜色）。
// 说明：try/catch 是为了兼容没有该字段/翻译键的旧版本或其它数据源；
//       取不到时返回空串，调用方按「无提示」处理即可。
// ============================================================================
namespace Electromagnetic.UI
{
    /// <summary>
    /// 与游戏版本/数据差异相关的兼容性辅助。静态类，无状态。
    /// </summary>
    public static class CompatibilityTool
    {
        /// <summary>
        /// 拼出「最喜欢的颜色」tooltip 文本。
        /// 使用原版翻译键 <c>FavoriteColorTooltip</c>；若 pawn 有可见的意识形态，
        /// 追加 <c>OrIdeoColor</c> 一段。任何异常都吞掉并返回空串，避免 UI 因缺字段报错。
        /// </summary>
        public static string GetFavoriteColorTooltip(Pawn pawn)
        {
            string result = "";
            try
            {
                string arg = string.Empty;
                bool flag = pawn.Ideo != null && !pawn.Ideo.hidden;
                if (flag)
                {
                    arg = "OrIdeoColor".Translate(pawn.Named("PAWN"));
                }
                result = "FavoriteColorTooltip".Translate(pawn.Named("PAWN"), pawn.story.favoriteColor.label.Named("COLOR"), 0.6f.ToStringPercent().Named("PERCENTAGE"), arg.Named("ORIDEO")).Resolve();
            }
            catch
            {
            }
            return result;
        }
    }
}
