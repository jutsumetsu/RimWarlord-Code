using Electromagnetic.Core;
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
    public class CompAbilityEffect_HeavyNucleiKick : CompAbilityEffect
    {
        public new CompProperties_AbilityHeavyNucleiKick Props
        {
            get
            {
                return (CompProperties_AbilityHeavyNucleiKick)this.props;
            }
        }
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Map map = this.parent.pawn.Map;
            PawnFlyer pawnFlyer = PawnFlyer.MakeFlyer(ThingDefOf.PawnFlyer, this.parent.pawn, target.Cell, null, null);
            GenSpawn.Spawn(pawnFlyer, this.parent.pawn.Position, map, WipeMode.Vanish);
            base.Apply(target, dest);
            Pawn pawn = (Pawn)((Thing)target);
            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, false);
            pawn.stances.stunner.StunFor(60, this.parent.pawn, false, false);
            caster = this.parent.pawn;
            enemy = pawn;
            spawn = false;
        }
        public override void CompTick()
        {
            base.CompTick();
            if (spawn == false)
            {
                if (caster.Spawned)
                {
                    spawn = true;
                    Pawn pawn = enemy;
                    Hediff_RWrd_PowerRoot root = caster.GetRoot();
                    int num = 40 + root.energy.CurrentDef.level;
                    int acr = root.energy.AvailableCompleteRealm();
                    int pff = root.energy.PowerFlowFactor();
                    int multiplier = acr + pff;
                    multiplier = (int)Math.Floor(multiplier / 2f);
                    num *= multiplier;
                    List<Thing> list = new List<Thing>();
                    foreach (Pawn pawn2 in pawn.MapHeld.mapPawns.AllPawns)
                    {
                        bool flag = pawn2.Faction == Faction.OfPlayer;
                        if (flag)
                        {
                            list.Add(pawn2);
                        }
                    }
                    GenExplosion.DoExplosion(pawn.PositionHeld, pawn.MapHeld, 1f, DamageDefOf.Bomb, caster, num, 0, null, null, null, null, null, 0, 1, null, false, null, 0, 1, 0, false, null, list);
                    CameraJumper.TrySelect(caster);
                    caster = null;
                    enemy = null;
                }
            }
        }
        private Pawn caster;
        private Pawn enemy;
        private bool spawn = true;
    }
}
