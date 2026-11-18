using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse.AI;
using Electromagnetic.Abilities;

namespace Electromagnetic.Core
{
    public class JobDriver_EMApparelRestock : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_General.Wait(30);
            yield return Toils_General.Do(delegate
            {
                Hediff_RWrd_PowerRoot root = pawn.GetPowerRoot();
                if (root == null) return;
                HediffComp_ApparelRestock comp = root.GetComp<HediffComp_ApparelRestock>();
                if (comp == null) return;
                Ability atomSplit = pawn.abilities.GetAbility(RWrd_DefOf.RWrd_Ability_AtomSplit);
                comp.RestockAll(pawn, atomSplit as RWrd_AbilityBase);
            });
        }
    }
}
