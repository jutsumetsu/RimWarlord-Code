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
        public RimWarlordDef CurrentDef
        {
            get
            {
                return this.currentRWrd.def;
            }
        }
        //判断是否满级
        public bool OnMaxLevel
        {
            get
            {
                return this.CurrentDef.level == 99;
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
            this.currentRWrd = new RimWarlord(RimWarlordDef.Dict.GetValueOrDefault(0), this.pawn);
            this.OnPostSetLevel();
        }
        //能量
        public float Energy
        {
            get
            {
                return this.energy;
            }
        }
        //经验
        public float Exp
        {
            get
            {
                return this.exp;
            }
        }
        //完全境界
        public float CompleteRealm
        {
            get
            {
                return this.completerealm;
            }
        }
        //力量流量
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
        //设置能量
        public void SetEnergy(float num)
        {
            float num2 = this.energy + num;
            bool flag = num2 >= 0f;
            if (flag)
            {
                this.energy = ((num2 < this.CurrentDef.MaxEnergy) ? num2 : this.CurrentDef.MaxEnergy);
            }
            else
            {
                this.energy = 0f;
            }
        }
        //设置经验
        public void SetExp(float num)
        {
            bool flag = num <= 0f;
            if (!flag)
            {
                float num2 = this.exp + num;
                this.exp = ((num2 > this.CurrentDef.EXP) ? this.CurrentDef.EXP : num2);
            }
        }
        //设置完全境界
        public void SetCompleteRealm(float num)
        {
            bool flag = num <= 0f;
            if (!flag)
            {
                float num2 = this.completerealm + num;
                this.completerealm = (num2 > this.CurrentDef.MaxCompleteRealm ? this.CurrentDef.MaxCompleteRealm : num2);
            }
        }
        //设置力量流量
        public void SetPowerFlow(int num)
        {
            bool flag = num <= 0;
            if (!flag)
            {
                int num2 = this.powerflow + num;
                this.powerflow = (num2 > this.CurrentDef.MaxPowerFlow ? this.CurrentDef.MaxPowerFlow : num2);
            }
        }
        //升级检查
        public bool IsUpdateLevelTiming()
        {
            RimWarlord nextRWrd = this.GetNextRWrd();
            bool flag = this.currentRWrd == nextRWrd;
            bool result;
            if (flag)
            {
                result = false;
            }
            else
            {
                bool flag2 = this.exp >= this.CurrentDef.EXP;
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
        //可用完全境界等级
        public int AvailableCompleteRealm()
        {
            int acr = (int)Math.Floor(this.completerealm * 10);
            return acr;
        }
        //力量流量乘数
        public int PowerFlowFactor()
        {
            float num = this.powerflow / 100000;
            int pff = (int)Math.Ceiling(num);
            return pff;
        }
        //设置等级
        public void SetLevel()
        {
            RimWarlord nextRWrd = this.GetNextRWrd() ;
            bool flag = nextRWrd == this.currentRWrd ;
            if (!flag)
            {
                bool flag2 = this.IsUpdateLevelTiming() ;
                if (flag2)
                {
                    this.currentRWrd = nextRWrd ;
                    this.OnPostSetLevel() ;
                    this.exp = 0f;
                }
            }
        }
        //检查等级技能限制
        private void OnPostSetLevel()
        {
            this.pawn.CheckLevelAndLimitingAbility();
        }
        //获取下一级参数
        public RimWarlord GetNextRWrd()
        {
            RimWarlord rimWarlord = this.currentRWrd;
            bool flag = this.CurrentDef.level == 99;
            RimWarlord result;
            if (flag)
            {
                result = rimWarlord;
            }
            else
            {
                RimWarlordDef def;
                bool flag2 = RimWarlordDef.Dict.TryGetValue(this.CurrentDef.level + 1, out def);
                if (flag2)
                {
                    result = new RimWarlord(def, this.pawn);
                }
                else
                {
                    result = rimWarlord;
                }
            }
            return result;
        }

        public void ExposeData()
        {
            Scribe_Values.Look<float>(ref this.energy, "energy", 0f, false);
            Scribe_Values.Look<float>(ref this.exp, "exp", 0f, false);
            Scribe_Values.Look<float>(ref this.completerealm, "completerealm", 0f, false);
            Scribe_Values.Look<int>(ref this.powerflow, "powerflow", 0, false);
            Scribe_Deep.Look<RimWarlord>(ref this.currentRWrd, "currentRWrd", Array.Empty<object>());
            Scribe_Values.Look<bool>(ref this.canSelfDestruct, "canSelfDestruct", false, false);
        }

        public Pawn pawn;

        public RimWarlord currentRWrd = new RimWarlord();

        public float completerealm = 0f;
        public int powerflow = 0;
        public float energy = 0f;
        public float exp = 0f;
        public float damage = 0f;
        public int trainDesireFactor = 1;
        //等级上下限
        public const int LevelMax = 99;
        public const int LevelMin = 0;

        public bool canSelfDestruct = false;
    }
}
