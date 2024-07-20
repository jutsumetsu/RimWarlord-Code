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
        public virtual bool CanPawnUnlock(Pawn pawn)
        {
            return this.PawnHasEnoughLevel(pawn) && this.PawnHasEnoughCompleteRealm(pawn) && this.PawnHasUnlockPreAbility(pawn);
        }
        //检测是否抵达需求等级
        private bool PawnHasEnoughLevel(Pawn pawn)
        {
            bool result;
            if (pawn.IsHaveRoot())
            {
                Hediff_RWrd_PowerRoot root = pawn.GetRoot();
                int level = root.energy.CurrentDef.level;
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
        //检测是否抵达需求完全境界
        private bool PawnHasEnoughCompleteRealm(Pawn pawn)
        {
            bool result;
            if (pawn.IsHaveRoot())
            {
                Hediff_RWrd_PowerRoot root = pawn.GetRoot();
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
        //检测是否解锁前置技能
        private bool PawnHasUnlockPreAbility(Pawn pawn)
        {
            bool result;
            if (pawn.IsHaveRoot())
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
        public IEnumerable<string> ConfigErrors()
        {
            bool flag = this.abilities == null;
            if (flag)
            {
                yield return "No Abilities Found For Node " + this.number.ToString();
            }
            yield break;
        }
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
                    //游戏语言检测
                    bool lflag = LanguageDatabase.activeLanguage.ToString() == "Simplified Chinese";
                    bool lflag2 = LanguageDatabase.activeLanguage.ToString() == "Traditional Chinese";
                    string required = "";
                    if (this.requiredCompleteRealm < 1)
                    {
                        if (lflag || lflag2)
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
        public int requiredLevel = 0;
        public float requiredCompleteRealm = 0;
        public int level;
        public int preNode = 0;
        public List <AbilityDef> requiredAbilities = new List <AbilityDef>();
        public List<AbilityDef> abilities = new List<AbilityDef>();
    }
}
