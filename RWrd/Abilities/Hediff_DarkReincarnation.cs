using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Electromagnetic.Abilities
{
    public class Hediff_DarkReincarnation : Hediff_TargetBase
    {
        public override void Tick()
        {
            base.Tick();
            List<IntVec3> range = GenRadial.RadialCellsAround(this.pawn.Position, 10f, true).ToList<IntVec3>();
            foreach (Pawn p in this.pawn.MapHeld.mapPawns.AllHumanlikeSpawned)
            {
                if (p == this.pawn || !range.Contains(p.Position))
                {
                    continue; // 当pawn为施术者或不在范围内时跳过
                }
                Log.Message("Get pawn in range");
                // 检查施术者是否倒地或死亡
                if (pawn.Downed || pawn.Dead)
                {
                    continue;
                }
                if (p.jobs.curDriver is JobDriver_CastAbility jobDriver)
                {
                    Ability ability = jobDriver.job.ability;
                    CompAbilityEffect_ReduceEnergy compAbilityEffect_ReduceEnergy = ability.CompOfType<CompAbilityEffect_ReduceEnergy>();
                    if (compAbilityEffect_ReduceEnergy != null)
                    {
                        RWrd_PsyCastBase ability2 = (RWrd_PsyCastBase)this.pawn.abilities.GetAbility(ability.def);
                        if (ability2 == null)
                        {
                            this.pawn.abilities.GainAbility(ability.def);
                            ability2 = (RWrd_PsyCastBase)this.pawn.abilities.GetAbility(ability.def);
                            ability2.mastery = -100;
                            CompAbilityEffect_ReduceEnergy compAbilityEffect_ReduceEnergy2 = ability2.CompOfType<CompAbilityEffect_ReduceEnergy>();
                            compAbilityEffect_ReduceEnergy2.Props.isAshura = true;
                        }
                        float num = 0.1f * (float)Math.Ceiling(this.mastery / 10f) / 10f;
                        ability2.mastery += num;
                    }
                }
            }
        }
    }
}
