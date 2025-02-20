using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Electromagnetic.Abilities
{
    public class CompAbilityEffect_Electromagnetic : CompAbilityEffect
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
        public RWrd_PsyCastBase Ability
        {
            get
            {
                return (RWrd_PsyCastBase)this.parent;
            }
        }
    }
}
