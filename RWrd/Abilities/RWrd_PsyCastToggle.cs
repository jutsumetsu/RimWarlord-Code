using Electromagnetic.Core;
using Electromagnetic.Electromagnetic.UI;
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
    public class RWrd_PsyCastToggle : Ability
    {
        public override bool CanCast
        {
            get
            {
                bool flag = !base.CanCast;
                return !flag;
            }
        }
        public RWrd_PsyCastToggle(Pawn pawn) : base(pawn)
        {
        }
        public RWrd_PsyCastToggle(Pawn pawn, AbilityDef def) : base(pawn, def)
        {
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
            Scribe_Values.Look<float>(ref this.mastery, "mastery", 0f, false);
            Scribe_Values.Look<float>(ref this.outputPower, "outputpowerability", 0f, false);
            Scribe_Values.Look<bool>(ref this.isActive, "activetoggle", false, false);
        }
        /// <summary>
        /// 设置精通值
        /// </summary>
        /// <param name="num">数值</param>
        public void SetMastery(float num)
        {
            if (num > 0)
            {
                float num2 = this.mastery + num;
                this.mastery = (num2 > this.MaxMastery ? this.MaxMastery : num2);
            }
        }
        public float mastery = 0;
        public float outputPower = 1;
        public float MaxMastery = 100;
        public bool isActive = false;

        private List<CompAbilityToggle_Electromagnetic> toggleComps;
    }
}
