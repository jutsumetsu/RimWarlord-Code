using RimWorld;
using Verse;
using UnityEngine;

namespace Electromagnetic
{
    /// <summary>
    /// 地图级经验池系统
    /// 可用于存储、提取和管理全局经验点。
    /// </summary>
    public class MapComponent_ExpPool : MapComponent
    {
        public float currentExp = 0f;     // 当前经验
        public float maxExp = 10000f;     // 最大经验容量
        public float passiveGainPerDay = 0f; // 每天自然恢复经验量

        private const float TicksPerDay = 60000f;

        public MapComponent_ExpPool(Map map) : base(map)
        {
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            // 自然增加经验
            // TODO：根据强者的存在开启
            if (passiveGainPerDay > 0f)
            {
                float gainPerTick = passiveGainPerDay / TicksPerDay;
                AddExp(gainPerTick);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref currentExp, "currentExp", 0f);
            Scribe_Values.Look(ref maxExp, "maxExp", 10000f);
            Scribe_Values.Look(ref passiveGainPerDay, "passiveGainPerDay", 0f);
        }

        /// <summary>
        /// 向经验池添加经验。
        /// </summary>
        public void AddExp(float amount)
        {
            if (amount <= 0f) return;

            currentExp += amount;
            if (currentExp > maxExp)
                currentExp = maxExp;
        }

        /// <summary>
        /// 从经验池提取经验。
        /// </summary>
        /// <returns>如果成功提取返回true，否则false。</returns>
        public bool ConsumeExp(float amount)
        {
            if (amount <= 0f) return false;
            if (currentExp < amount)
                return false;

            currentExp -= amount;
            return true;
        }

        /// <summary>
        /// 获取当前经验量。
        /// </summary>
        public float GetCurrentExp() => currentExp;

        /// <summary>
        /// 获取经验池的填充比例（0-1）。
        /// </summary>
        public float GetFillPercent() => Mathf.Clamp01(currentExp / maxExp);

        /// <summary>
        /// 直接设置经验值。
        /// </summary>
        public void SetExp(float value)
        {
            currentExp = Mathf.Clamp(value, 0f, maxExp);
        }

        /// <summary>
        /// 调整最大容量。
        /// </summary>
        public void SetMaxExp(float newMax)
        {
            maxExp = Mathf.Max(1f, newMax);
            if (currentExp > maxExp)
                currentExp = maxExp;
        }
    }
}
