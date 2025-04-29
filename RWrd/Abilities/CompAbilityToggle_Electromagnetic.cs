using Electromagnetic.Abilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Electromagnetic.Abilities
{
    public class CompAbilityToggle_Electromagnetic : AbilityComp
    {
        /// <summary>
        /// pawn接口
        /// </summary>
        public Pawn Caster
        {
            get
            {
                return this.parent.pawn;
            }
        }
        /// <summary>
        /// 技能接口
        /// </summary>
        public RWrd_PsyCastToggle Ability
        {
            get
            {
                return (RWrd_PsyCastToggle)this.parent;
            }
        }
        public virtual void Apply() { }
    }
}
