using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace Electromagnetic
{
    /// <summary>
    /// 警报提示：显示当前地图的经验池信息
    /// </summary>
    public class Alert_ExpPoolInfo : Alert
    {
        private MapComponent_ExpPool CompExp
        {
            get
            {
                return Find.CurrentMap?.GetComponent<MapComponent_ExpPool>();
            }
        }

        private const float TicksPerDay = 60000f;

        public Alert_ExpPoolInfo()
        {
            this.defaultLabel = "经验池";
            this.defaultPriority = AlertPriority.Medium;
        }

        public override AlertReport GetReport()
        {
            if (CompExp == null || CompExp.GetCurrentExp() <= 0f)
            {
                return AlertReport.Inactive;
            }
            return AlertReport.Active;
        }

        public override string GetLabel()
        {
            float cur = CompExp.GetCurrentExp();
            float max = CompExp.maxExp;
            float percent = Mathf.Clamp01(cur / max) * 100f;
            return $"经验池：{cur:F0}/{max:F0}（{percent:F1}%）";
        }

        public override TaggedString GetExplanation()
        {
            StringBuilder sb = new StringBuilder();

            float cur = CompExp.GetCurrentExp();
            float max = CompExp.maxExp;
            float percent = Mathf.Clamp01(cur / max) * 100f;

            sb.AppendLine($"当前经验：{cur:F1}");
            sb.AppendLine($"最大容量：{max:F1}");
            sb.AppendLine($"储存比例：{percent:F1}%");

            if (CompExp.passiveGainPerDay > 0f)
            {
                float perTick = CompExp.passiveGainPerDay / TicksPerDay;
                sb.AppendLine($"自然增长速度：每天 +{CompExp.passiveGainPerDay:F1} 经验（≈每秒 +{perTick * 60f:F3}）");
            }
            else
            {
                sb.AppendLine("自然增长：无");
            }

            sb.AppendLine();
            sb.AppendLine("经验可被用于强化或升级。");
            sb.AppendLine("通过与强者打交、击杀敌人、完成事件等方式可获得经验。");

            return sb.ToString();
        }
    }
}
