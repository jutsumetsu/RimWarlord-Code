using Electromagnetic.Abilities;
using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electromagnetic.UI
{
    public class Command_Electromagnetic : Command_Ability
    {
        public Command_Electromagnetic(Ability ability, float mastery, Pawn pawn) : base(ability, pawn)
        {
            this.Mastery = mastery;
        }
        public override string Tooltip
        {
            get
            {
                string text = this.ability.Tooltip;
                if (ReduceEnergy != 0)
                {
                    text += "\n" + "RWrd_EnergyReduce".Translate() + this.ReduceEnergy.ToString();
                }
                text += "\n" + "RWrd_Mastery".Translate() + this.Mastery;
                return text;
            }
        }
        public float ReduceEnergy
        {
            get
            {
                CompAbilityEffect_ReduceEnergy compAbilityEffect_ReduceEnergy = ability.CompOfType<CompAbilityEffect_ReduceEnergy>();
                return -compAbilityEffect_ReduceEnergy.EnergyReduce;
            }
        }
        public float Mastery;
    }
}
