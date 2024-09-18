using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Electromagnetic.Core
{
    public class RWrd_RouteNode
    {
        /// <summary>
        /// 节点解锁总检测
        /// </summary>
        /// <param name="pawn">小人实例</param>
        /// <returns></returns>
        public virtual bool CanPawnUnlock(Pawn pawn)
        {
            return this.PawnHasEnoughLevel(pawn) && this.PawnHasEnoughCompleteRealm(pawn) && this.PawnHasUnlockPreAbility(pawn);
        }
        /// <summary>
        /// 检测是否抵达需求等级
        /// </summary>
        /// <param name="pawn">小人实例</param>
        /// <returns></returns>
        private bool PawnHasEnoughLevel(Pawn pawn)
        {
            bool result;
            if (pawn.IsHavePowerRoot())
            {
                Hediff_RWrd_PowerRoot root = pawn.GetPowerRoot();
                int level = root.energy.level;
                if (level >= requiredLevel)
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            else
            {
                result = false;
            }
            return result;
        }
        /// <summary>
        /// 检测是否抵达需求完全境界
        /// </summary>
        /// <param name="pawn">小人实例</param>
        /// <returns></returns>
        private bool PawnHasEnoughCompleteRealm(Pawn pawn)
        {
            bool result;
            if (pawn.IsHavePowerRoot())
            {
                Hediff_RWrd_PowerRoot root = pawn.GetPowerRoot();
                float cr = root.energy.completerealm;
                if (cr >= requiredCompleteRealm)
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            else
            {
                result = false;
            }
            return result;
        }
        /// <summary>
        /// 检测是否解锁前置技能
        /// </summary>
        /// <param name="pawn">小人实例</param>
        /// <returns></returns>
        private bool PawnHasUnlockPreAbility(Pawn pawn)
        {
            bool result;
            if (pawn.IsHavePowerRoot())
            {
                result = false;
                if (this.requiredAbilities.Count > 0)
                {
                    foreach (AbilityDef ability in this.requiredAbilities)
                    {
                        Ability flag = pawn.abilities.GetAbility(ability);
                        if (flag != null)
                        {
                            result = true;
                        }
                        else
                        {
                            result = false;
                        }
                    }
                }
                else
                {
                    result = true;
                }
            }
            else
            {
                result = false;
            }
            return result;
        }
        /// <summary>
        /// 节点能力为空报错
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> ConfigErrors()
        {
            bool flag = this.abilities == null;
            if (flag)
            {
                yield return "No Abilities Found For Node " + this.number.ToString();
            }
            yield break;
        }
        /// <summary>
        /// 解锁需求文本
        /// </summary>
        public string unlockRequired
        {
            get
            {
                string result = "";
                if (this.requiredLevel > 0)
                {
                    result += "RWrd_LevelRequired".Translate(this.requiredLevel.ToString());
                }
                else if (this.requiredCompleteRealm > 0)
                {
                    string required;
                    if (this.requiredCompleteRealm < 1)
                    {
                        if (Tools.IsChineseLanguage)
                        {
                            required = "RWrd_CompleteRealmRequired1".Translate((this.requiredCompleteRealm * 10).ToString());
                        }
                        else
                        {
                            required = "RWrd_CompleteRealmRequired1".Translate(this.requiredCompleteRealm.ToString("P0"));
                        }
                    }
                    else if (this.requiredCompleteRealm == 1)
                    {
                        required = "RWrd_FinalRealm".Translate();
                    }
                    else
                    {
                        required = "RWrd_CompleteRealmRequired2".Translate(this.requiredCompleteRealm.ToString());
                    }
                    if (result != "")
                    {
                        result += ", " + required;
                    }
                    else
                    {
                        result += required;
                    }
                }
                return result;
            }
        }
        public int number;
        /// <summary>
        /// 等级需求
        /// </summary>
        public int requiredLevel = 0;
        /// <summary>
        /// 完全境界需求
        /// </summary>
        public float requiredCompleteRealm = 0;
        /// <summary>
        /// 节点层级
        /// </summary>
        public int level;
        /// <summary>
        /// 绘画前置节点
        /// </summary>
        public int preNode = 0;
        /// <summary>
        /// 前置技能
        /// </summary>
        public List <AbilityDef> requiredAbilities = new List <AbilityDef>();
        /// <summary>
        /// 节点技能列表
        /// </summary>
        public List<AbilityDef> abilities = new List<AbilityDef>();
    }
}
