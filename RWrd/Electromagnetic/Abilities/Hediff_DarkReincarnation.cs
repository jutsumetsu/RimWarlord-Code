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
                bool flag = p == this.pawn;
                if (!flag)
                {
                   if (range.Contains(p.Position))
                    {
                        Log.Message("Get pawn in range");
                        bool downed = pawn.Downed;
                        if (!downed)
                        {
                            bool dead = pawn.Dead;
                            if (!dead)
                            {
                                if (p.jobs.curDriver.GetType() == typeof(JobDriver_CastAbility))
                                {
                                    JobDriver_CastAbility jobDriver = (JobDriver_CastAbility)p.jobs.curDriver;
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
            }
        }
    }
}
