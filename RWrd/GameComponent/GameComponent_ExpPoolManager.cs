using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace Electromagnetic
{
    /// <summary>
    /// 全局经验池
    /// </summary>
    public class GameComponent_ExpPoolManager : GameComponent
    {
        public float globalExp = 0f;              // 当前全局经验
        public float maxGlobalExp = 50000f;       // 最大上限
        public float passiveGainPerDay = 200f;    // 每天自然增加的经验量
        private const float TicksPerDay = 60000f;

        // 缓存各WorldPawn的经验贡献记录
        public Dictionary<int, float> pawnExpContribution = new Dictionary<int, float>();

        public GameComponent_ExpPoolManager(Game game) : base()
        {
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();

            // 自然增加经验
            //TODO：根据强者的存在开启
            if (Find.TickManager.TicksGame % 2500 == 0)
            {
                float perTickGain = passiveGainPerDay / TicksPerDay;
                globalExp += perTickGain * 2500f;

                if (globalExp > maxGlobalExp)
                    globalExp = maxGlobalExp;
            }
        }

        /// <summary>
        /// 增加经验
        /// </summary>
        public void AddExp(float amount, Pawn sourcePawn = null)
        {
            if (amount <= 0f) return;
            globalExp += amount;
            if (globalExp > maxGlobalExp)
                globalExp = maxGlobalExp;

            if (sourcePawn != null)
            {
                int id = sourcePawn.thingIDNumber;
                if (pawnExpContribution.ContainsKey(id))
                    pawnExpContribution[id] += amount;
                else
                    pawnExpContribution[id] = amount;
            }
        }

        /// <summary>
        /// 消耗经验
        /// </summary>
        public bool ConsumeExp(float amount)
        {
            if (amount <= 0f) return false;
            if (globalExp < amount) return false;

            globalExp -= amount;
            return true;
        }

        /// <summary>
        /// 设置当前经验
        /// </summary>
        public void SetExp(float value)
        {
            globalExp = Mathf.Clamp(value, 0f, maxGlobalExp);
        }

        /// <summary>
        /// 设置经验池最大容量
        /// </summary>
        public void SetMaxExp(float value)
        {
            if (value <= 0f) return;
            maxGlobalExp = value;
            if (globalExp > maxGlobalExp)
                globalExp = maxGlobalExp;
        }

        /// <summary>
        /// 获取当前经验
        /// </summary>
        public float GetExp()
        {
            return globalExp;
        }

        /// <summary>
        /// 清空全局经验池
        /// </summary>
        public void Clear()
        {
            globalExp = 0f;
            pawnExpContribution.Clear();
        }

        /// <summary>
        /// 获取某个Pawn的经验贡献
        /// </summary>
        public float GetPawnContribution(Pawn pawn)
        {
            if (pawn == null) return 0f;
            pawnExpContribution.TryGetValue(pawn.thingIDNumber, out float val);
            return val;
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref globalExp, "globalExp", 0f);
            Scribe_Values.Look(ref maxGlobalExp, "maxGlobalExp", 50000f);
            Scribe_Values.Look(ref passiveGainPerDay, "passiveGainPerDay", 200f);
            Scribe_Collections.Look(ref pawnExpContribution, "pawnExpContribution", LookMode.Value, LookMode.Value);

            if (pawnExpContribution == null)
                pawnExpContribution = new Dictionary<int, float>();
        }
    }
}
