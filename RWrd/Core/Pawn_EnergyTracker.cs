using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Electromagnetic.Setting;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Electromagnetic.Core
{
    [StaticConstructorOnStartup]
    public class Pawn_EnergyTracker : IExposable
    {
        /// <summary>
        /// 判断是否满级
        /// </summary>
        public bool OnMaxLevel
        {
            get
            {
                return this.level == this.LevelMax;
            }
        }
        /// <summary>
        /// 是否是终极强者
        /// </summary>
        public bool IsUltimate
        {
            get
            {
                return this.PowerEnergy >= 1;
            }
        }
        public int FinalLevel
        {
            get
            {
                if (this.AvailableLevel != this.LevelMax)
                {
                    return 0;
                }

                if (this.exp >= 9999)
                {
                    return 4;
                }
                if (this.exp >= 9990)
                {
                    return 3;
                }
                if (this.exp >= 9900)
                {
                    return 2;
                }
                if (this.exp >= 9000)
                {
                    return 1;
                }
                return 0;
            }
        }
        public int FinalLevelOffset
        {
            get
            {
                int num = 0;
                if (this.FinalLevel > 0)
                {
                    int level = this.FinalLevel;
                    num = (1 + level) * level * level / 2;
                }
                return num;
            }
        }
        /// <summary>
        /// 兆级修为
        /// </summary>
        public float PowerEnergy
        {
            get
            {
                float EMPower;
                if (this.level == LevelMax && FinalLevel > 0)
                {
                    EMPower = 1000000;
                }
                else if (this.level == 0)
                {
                    EMPower = 0;
                }
                else
                {
                    EMPower = this.level * 10000 + this.exp;
                }
                int cr = (int)Math.Floor(this.completerealm);
                float preEnergy = EMPower * cr * this.powerflow / 1000000000000;
                return (float)Math.Floor(preEnergy);
            }
        }
        public float Multiplier
        {
            get
            {
                int acr = this.AvailableCompleteRealm();
                int pff = this.PowerFlowFactor(true);
                if (!this.IsUltimate)
                {
                    return (int)Math.Floor((acr + pff) / 2f);
                }
                else
                {
                    return acr + pff;
                }
            }
        }
        /// <summary>
        /// 初始化
        /// </summary>
        private void Init()
        {
            this.energy = 0f;
            this.exp = 0f;
            this.completerealm = 0f;
            this.powerflow = 0;
            this.OnPostSetLevel();
        }
        /// <summary>
        /// 每秒飞行耗能
        /// </summary>
        public int FlightConsumptionPerSecond
        {
            get
            {
                int result = 90;
                if (this.AvailableLevel < 50) result -= Mathf.FloorToInt(this.AvailableLevel / 10) * 7;
                else result = 0;
                return result;
            }
        }
        /// <summary>
        /// 可用等级
        /// </summary>
        public int AvailableLevel
        {
            get
            {
                if (this.pawn.IsLockedByEMPower() && !this.IsUltimate)
                {
                    return Math.Min(this.powerLimit, this.level);
                }
                else
                {
                    return this.level;
                }
            }
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
        /// <summary>
        /// 最大经验值
        /// </summary>
        public int MaxExp
        {
            get
            {
                if (OnMaxLevel)
                {
                    return 9999;
                }
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
        /// 经验补正因子
        /// </summary>
        public float ExpCorrectionFactor
        {
            get
            {
                int x = this.level;
                float factor;
                if (x <= 10)
                {
                    factor = 18.75f - 0.0231f * x;
                }
                else if (x <= 30)
                {
                    factor = 24.017f - 0.5499f * x;
                }
                else if (x <= 47)
                {
                    factor = 19.6392f - 0.4039f * x;
                }
                else if (x == 48)
                {
                    factor = 0.4539f;
                }
                else if (x > 48 && x <= 50)
                {
                    factor = 2.7268f - 0.04735f * x;
                }
                else if (x <= 75)
                {
                    factor = 0.7432f - 0.007678f * x;
                }
                else if (x <= 90)
                {
                    factor = 0.5484f - 0.005081f * x;
                }
                else if (x <= 97)
                {
                    factor = 0.3621f - 0.003011f * x;
                }
                else if (x == 98)
                {
                    factor = 0.06202f;
                }
                else
                {
                    factor = 0.05406f;
                }
                return factor;
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
                if (IsUltimate)
                {
                    return (int)this.powerflow;
                }
                return (int)(this.powerflow * this.pawn.GetHeartHealthPercent());
            }
        }
        /// <summary>
        /// 伤害免疫阈值
        /// </summary>
        public float DamageImmunityThreshold
        {
            get
            {
                float num = 12;
                int lf = this.AvailableLevel + this.FinalLevelOffset + 1;
                float uf = 0;
                if (this.IsUltimate)
                {
                    lf *= 2;
                    uf = this.PowerEnergy * 3;
                }
                num += lf + uf;
                return num;
            }
        }
        /// <summary>
        /// 脱战能量回复数值
        /// </summary>
        public int PCTEnergy
        {
            get
            {
                return (int)Math.Floor(this.MaxEnergy * 0.06f);
            }
        }
        /// <summary>
        /// 战斗中能量回复数值
        /// </summary>
        public int PCTEnergyFight
        {
            get
            {
                return (int)Math.Floor(this.MaxEnergy * 0.02f);
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
        /// 增减能量
        /// </summary>
        /// <param name="num">数值</param>
        public void SetEnergy(float num)
        {
            float num2 = this.energy + num;
            bool flag = num2 >= 0f;
            if (flag)
            {
                this.energy = ((num2 < this.MaxAvailableEnergy) ? num2 : this.MaxAvailableEnergy);
            }
            else
            {
                this.energy = 0f;
            }
        }
        /// <summary>
        /// 能量回复
        /// </summary>
        public void EnergyRecharge()
        {
            bool flag = this.MaxEnergy > 5000;
            bool drafted = this.pawn.Drafted;
            bool NPC = this.pawn.Faction != Faction.OfPlayer;
            if (!NPC)
            {
                if (!flag)
                {
                    if (drafted)
                    {
                        this.SetEnergy(100);
                    }
                    else
                    {
                        this.SetEnergy(300);
                    }
                }
                else
                {
                    if (drafted)
                    {
                        this.SetEnergy(this.PCTEnergyFight);
                    }
                    else
                    {
                        this.SetEnergy(this.PCTEnergy);
                    }
                }
            }
            else
            {
                if (!flag)
                {
                    this.SetEnergy(100);
                }
                else
                {
                    this.SetEnergy(this.PCTEnergyFight);
                }
            }
        }
        /// <summary>
        /// 设置经验
        /// </summary>
        /// <param name="num">数值</param>
        public void SetExp(float num)
        {
            bool flag = num <= 0f;
            if (!flag && !this.pawn.IsLockedByEMPower())
            {
                if (this.FinalLevel > 0)
                {
                    num *= (float)Math.Pow(0.1f, this.FinalLevel);
                    num /= this.FinalLevel + 1;
                }
                float num2 = this.exp + num * RWrdSettings.XpFactor;
                this.exp = (num2 > this.MaxExp) ? this.MaxExp : num2;
                if (this.OnMaxLevel)
                {
                    this.pawn.UpdatePowerRootStageInfo();
                }
                if (this.IsUpdateLevelTiming() && this.level != RWrdSettings.GlobalLevelLimit)
                {
                    this.SetLevel();
                }
            }
        }
        /// <summary>
        /// 强制设置经验
        /// </summary>
        /// <param name="num">数值</param>
        public void ForceSetExp(float num)
        {
            bool flag = num <= 0f;
            if (!flag)
            {
                float num2 = this.exp + num;
                this.exp = (num2 > this.MaxExp) ? this.MaxExp : num2;
                if (this.level == LevelMax)
                {
                    this.pawn.UpdatePowerRootStageInfo();
                }
                if (this.IsUpdateLevelTiming() && this.level != RWrdSettings.GlobalLevelLimit)
                {
                    this.SetLevel();
                }
            }
        }
        /// <summary>
        /// 增加完全境界
        /// </summary>
        /// <param name="num">数值</param>
        public void SetCompleteRealm(float num)
        {
            bool flag = num <= 0f;
            if (!flag)
            {
                float num2 = this.completerealm + num;
                this.completerealm = (num2 > this.MaxCompleteRealm ? this.MaxCompleteRealm : num2);
                this.pawn.CheckEMAbilityLimiting();
                this.pawn.UpdatePowerRootStageInfo(true);
            }
        }
        /// <summary>
        /// 设置完全境界
        /// </summary>
        /// <param name="num">数值</param>
        public void ForceSetCompleteRealm(float num)
        {
            this.completerealm = num;
            this.pawn.CheckEMAbilityLimiting();
            this.pawn.UpdatePowerRootStageInfo(true);
        }
        /// <summary>
        /// 增加力量流量
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
        /// 设置力量流量
        /// </summary>
        /// <param name="num">数值</param>
        public void ForceSetPowerFlow(int num)
        {
            this.powerflow = num;
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
        public int PowerFlowFactor(bool restricted = false)
        {
            float num = this.powerflow / 100000;
            if (restricted)
            {
                num = this.PowerFlow / 100000;
            }
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
                this.exp = 0f;
                if (this.level == 50)
                {
                    Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(RWrd_DefOf.RWrd_Flight);
                    if (hediff != null)
                    {
                        pawn.health.RemoveHediff(hediff);
                        Hediff newHediff = HediffMaker.MakeHediff(RWrd_DefOf.RWrd_Antigravity, this.pawn);
                        pawn.health.AddHediff(newHediff);
                    }
                }
                this.pawn.UpdatePowerRootStageInfo();
                this.OnPostSetLevel();
            }
        }
        /// <summary>
        /// 强制设置等级
        /// </summary>
        /// <param name="level"></param>
        public void ForceSetLevel(int level)
        {
            this.level = level;
            if (this.level >= 50)
            {
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(RWrd_DefOf.RWrd_Flight);
                if (hediff != null)
                {
                    pawn.health.RemoveHediff(hediff);
                    Hediff newHediff = HediffMaker.MakeHediff(RWrd_DefOf.RWrd_Antigravity, this.pawn);
                    pawn.health.AddHediff(newHediff);
                }
            }
            this.pawn.UpdatePowerRootStageInfo();
            this.OnPostSetLevel();
        }
        /// <summary>
        /// 设置等级后方法
        /// </summary>
        private void OnPostSetLevel()
        {
            this.pawn.CheckEMAbilityLimiting();
        }

        public void ExposeData()
        {
            Scribe_Values.Look<float>(ref this.energy, "energy", 0f, false);
            Scribe_Values.Look<float>(ref this.exp, "exp", 0f, false);
            Scribe_Values.Look<float>(ref this.Oexp, "oexp", 0f, false);
            Scribe_Values.Look<float>(ref this.completerealm, "completerealm", 0f, false);
            Scribe_Values.Look<float>(ref this.outputPower, "outputpower", 1f, false);
            Scribe_Values.Look<float>(ref this.wavePower, "wavePower", 1f, false);
            Scribe_Values.Look<float>(ref this.waveRange, "waveRange", 1f, false);
            Scribe_Values.Look<int>(ref this.powerflow, "powerflow", 0, false);
            Scribe_Values.Look<int>(ref this.level, "level", 0, false);
            Scribe_Values.Look<int>(ref this.powerLimit, "powerLimit", 999, false);
            Scribe_Values.Look<bool>(ref this.personalEnergyWave, "personalEnergyWave", true, false);
            bool flag = Scribe.mode == LoadSaveMode.PostLoadInit;
            if (flag)
            {
                if (this.exp > this.MaxExp)
                {
                    exp = this.MaxExp;
                }
            }
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
        public float MaxEnergy
        {
            get
            {
                return this.powerflow / 100;
            }
        }
        /// <summary>
        /// 最大可用能量
        /// </summary>
        public float MaxAvailableEnergy
        {
            get
            {
                return this.PowerFlow / 100;
            }
        }
        /// <summary>
        /// 能量值
        /// </summary>
        public float energy = 0f;
        /// <summary>
        /// 经验值
        /// </summary>
        public float exp = 0f;
        /// <summary>
        /// 原经验值
        /// </summary>
        public float Oexp = 0f;
        /// <summary>
        /// 伤害
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
        /// <summary>
        /// 出力上限
        /// </summary>
        public float outputPower = 1;
        /// <summary>
        /// 余波出力
        /// </summary>
        public float wavePower = 1;
        /// <summary>
        /// 余波范围
        /// </summary>
        public float waveRange = 1;
        /// <summary>
        /// 启用个人余波
        /// </summary>
        public bool personalEnergyWave = true;
        /// <summary>
        /// 力量上限
        /// </summary>
        public int powerLimit = 999;
    }
}
