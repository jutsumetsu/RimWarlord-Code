using Electromagnetic.Core;
using Electromagnetic.UI;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Electromagnetic.Abilities
{
    public class CompAbilityEffect_AtomSplit : CompAbilityEffect_Electromagnetic
    {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            var selectArtifact = new Dialog_SelectThings(this.parent.pawn.GetPowerRoot(),this.Ability);
            Find.WindowStack.Add(selectArtifact);
        }
    }
}
