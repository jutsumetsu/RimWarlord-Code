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
        public int number;
        public int requiredLevel = 0;
        public float requiredCompleteRealm = 0;
        public List <AbilityDef> requiredAbilities = new List <AbilityDef>();
        public List<AbilityDef> abilities = new List<AbilityDef>();
    }
}
