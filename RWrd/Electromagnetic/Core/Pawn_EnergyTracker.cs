using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Electromagnetic.Core
{
    [StaticConstructorOnStartup]
    public class Pawn_EnergyTracker : IExposable
    {
        //判断是否满级
        public bool OnMaxLevel
        {
            get
            {
                return this.level == this.LevelMax;
            }
        }
        //初始化
        private void Init()
        {
            this.energy = 0f;
            this.exp = 0f;
            this.completerealm = 0f;
            this.powerflow = 0;
            this.canSelfDestruct = false;
            this.OnPostSetLevel();
        }
        /// <summary>
        /// 能量
        /// </summary>
        public float Energy
        {
            get
            {
                return this.energy;
            }
        }
        /// <summary>
        /// 经验
        /// </summary>
        public float Exp
        {
            get
            {
                return this.exp;
            }
        }
        public int MaxExp
        {
            get
            {
                if (this.level == 0)
                {
                    return 100000;
                }
                else
                {
                    return 10000;
                }
            }
        }
        /// <summary>
        /// 完全境界
        /// </summary>
        public float CompleteRealm
        {
            get
            {
                return this.completerealm;
            }
        }
        /// <summary>
        /// 力量流量
        /// </summary>
        public int PowerFlow
        {
            get
            {
                return this.powerflow;
            }
        }

        public Pawn_EnergyTracker(Pawn pawn)
        {
            this.pawn = pawn;
            this.Init();
        }

        public Pawn_EnergyTracker()
        {
            this.Init();
        }
        /// <summary>
        /// 设置能量
        /// </summary>
        /// <param name="num">数值</param>
        public void SetEnergy(float num)
        {
            float num2 = this.energy + num;
            bool flag = num2 >= 0f;
            if (flag)
            {
                this.energy = ((num2 < this.MaxEnergy) ? num2 : this.MaxEnergy);
            }
            else
            {
                this.energy = 0f;
            }
        }
        /// <summary>
        /// 设置经验
        /// </summary>
        /// <param name="num">数值</param>
        public void SetExp(float num)
        {
            bool flag = num <= 0f;
            if (!flag)
            {
                float num2 = this.exp + num;
                this.exp = ((num2 > this.MaxExp) ? this.MaxExp : num2);
            }
        }
        /// <summary>
        /// 设置完全境界
        /// </summary>
        /// <param name="num">数值</param>
        public void SetCompleteRealm(float num)
        {
            bool flag = num <= 0f;
            if (!flag)
            {
                float num2 = this.completerealm + num;
                this.completerealm = (num2 > this.MaxCompleteRealm ? this.MaxCompleteRealm : num2);
                this.pawn.CheckAbilityLimiting();
                this.pawn.UpdateStageInfo(true);
            }
        }
        /// <summary>
        /// 设置力量流量
        /// </summary>
        /// <param name="num">数值</param>
        public void SetPowerFlow(int num)
        {
            bool flag = num <= 0;
            if (!flag)
            {
                int num2 = this.powerflow + num;
                this.powerflow = (num2 > this.MaxPowerFlow ? this.MaxPowerFlow : num2);
            }
        }
        /// <summary>
        /// 升级检查
        /// </summary>
        /// <returns></returns>
        public bool IsUpdateLevelTiming()
        {
            bool flag = this.OnMaxLevel;
            bool result;
            if (flag)
            {
                result = false;
            }
            else
            {
                bool flag2 = this.exp >= this.MaxExp;
                if (flag2)
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            return result;
        }
        /// <summary>
        /// 可用完全境界等级
        /// </summary>
        /// <returns></returns>
        public int AvailableCompleteRealm()
        {
            int acr = (int)Math.Floor(this.completerealm * 10);
            return acr;
        }
        /// <summary>
        /// 力量流量乘数
        /// </summary>
        /// <returns></returns>
        public int PowerFlowFactor()
        {
            float num = this.powerflow / 100000;
            int pff = (int)Math.Ceiling(num);
            return pff;
        }
        /// <summary>
        /// 设置等级
        /// </summary>
        public void SetLevel()
        {
            bool flag = this.IsUpdateLevelTiming();
            if (flag)
            {
                this.level += 1;
                this.OnPostSetLevel();
                this.pawn.UpdateStageInfo();
                this.exp = 0f;
            }
        }
        /// <summary>
        /// 设置等级后方法
        /// </summary>
        private void OnPostSetLevel()
        {
            this.pawn.CheckAbilityLimiting();
        }

        public void ExposeData()
        {
            Scribe_Values.Look<float>(ref this.energy, "energy", 0f, false);
            Scribe_Values.Look<float>(ref this.MaxEnergy, "maxenergy", 0f, false);
            Scribe_Values.Look<float>(ref this.exp, "exp", 0f, false);
            Scribe_Values.Look<float>(ref this.completerealm, "completerealm", 0f, false);
            Scribe_Values.Look<int>(ref this.powerflow, "powerflow", 0, false);
            Scribe_Values.Look<int>(ref this.level, "level", 0, false);
            Scribe_Values.Look<bool>(ref this.canSelfDestruct, "canSelfDestruct", false, false);
            bool flag = Scribe.mode == LoadSaveMode.PostLoadInit;
        }

        public Pawn pawn;
        /// <summary>
        /// 完全境界
        /// </summary>
        public float completerealm = 0f;
        /// <summary>
        /// 完全境界上限
        /// </summary>
        public int MaxCompleteRealm = 10000;
        /// <summary>
        /// 力量流量
        /// </summary>
        public int powerflow = 0;
        /// <summary>
        /// 力量流量上限
        /// </summary>
        public int MaxPowerFlow = 100000000;
        /// <summary>
        /// 最大能量
        /// </summary>
        public float MaxEnergy = 0f;
        /// <summary>
        /// 能量值
        /// </summary>
        public float energy = 0f;
        /// <summary>
        /// 经验值
        /// </summary>
        public float exp = 0f;
        /// <summary>
        /// 经验值
        /// </summary>
        public float damage = 0f;
        /// <summary>
        /// 练功渴望度
        /// </summary>
        public int trainDesireFactor = 1;
        /// <summary>
        /// 等级
        /// </summary>
        public int level = 0;
        /// <summary>
        /// 等级上限
        /// </summary>
        public int LevelMax = 99;
        /// <summary>
        /// 等级下限
        /// </summary>
        public int LevelMin = 0;

        public bool canSelfDestruct = false;
    }
}
