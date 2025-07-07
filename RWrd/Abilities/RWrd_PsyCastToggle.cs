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
    public class RWrd_PsyCastToggle : RWrd_AbilityBase
    {

        public RWrd_PsyCastToggle(Pawn pawn) : base(pawn)
        {
        }

        public RWrd_PsyCastToggle(Pawn pawn, AbilityDef def) : base(pawn, def)
        {
        }
        public override AcceptanceReport CanCast
        {
            get
            {
                bool flag = !base.CanCast;
                return !flag;
            }
        }
        public List<CompAbilityToggle_Electromagnetic> ToggleComps
        {
            get
            {
                if (this.toggleComps == null)
                {
                    IEnumerable<CompAbilityToggle_Electromagnetic> enumerable = this.CompsOfType<CompAbilityToggle_Electromagnetic>();
                    this.toggleComps = ((enumerable == null) ? new List<CompAbilityToggle_Electromagnetic>() : enumerable.ToList<CompAbilityToggle_Electromagnetic>());
                }
                return this.toggleComps;
            }
        }
        public virtual void ApplyToggles()
        {
            foreach (CompAbilityToggle_Electromagnetic compAbilityToggle in ToggleComps)
            {
                compAbilityToggle.Apply();
            }
        }
        /// <summary>
        /// 获取Gizmo
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<Command> GetGizmos()
        {
            bool flag = this.gizmo == null;
            if (flag)
            {
                this.gizmo = new Command_EMToggle(this, this.pawn)
                {
                    isActive = () => this.isActive
                };
            }
            else if (!(this.gizmo is Command_EMToggle))
            {
                this.gizmo = new Command_EMToggle(this, this.pawn)
                {
                    isActive = () => this.isActive
                };
            }
            yield return this.gizmo;
            yield break;
        }
        protected override void ApplyEffects(IEnumerable<CompAbilityEffect> effects, LocalTargetInfo target, LocalTargetInfo dest)
        {
            foreach (CompAbilityEffect compAbilityEffect in effects)
            {
                compAbilityEffect.Apply(target, dest);
            }
        }
        public new void AbilityTick()
        {
            this.AbilityTick();
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.isActive, "activetoggle", false, false);
        }
        public bool isActive = false;

        private List<CompAbilityToggle_Electromagnetic> toggleComps;
    }
}
