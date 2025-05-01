using Electromagnetic.Abilities;
using Electromagnetic.Core;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Electromagnetic.UI
{
    public class Command_SDToggle : Command_EMToggle
    {
        public Command_SDToggle(RWrd_SDPower ability, Pawn pawn) : base(ability, pawn)
        {
            this.ability = ability;
            float num = 0f;
            AbilityCategoryDef category = ability.def.category;
            int? num2 = (category != null) ? new int?(category.displayOrder) : null;
            this.Order = num + ((num2 != null) ? ((float)num2.GetValueOrDefault()) : 0f) / 100f + (float)ability.def.displayOrder / 1000f + (float)ability.def.level / 10000f;
            this.defaultLabel = ability.def.LabelCap;
            this.hotKey = ability.def.hotKey;
            this.icon = ability.def.uiIcon;
            this.shrinkable = true;
        }
        public override SoundDef CurActivateSound
        {
            get
            {
                if (this.isActive)
                {
                    return this.turnOffSound;
                }
                return this.turnOnSound;
            }
        }
        public override string Tooltip
        {
            get
            {
                string text = this.ability.Tooltip;
                return text;
            }
        }
        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            this.defaultLabel = (parms.multipleSelected ? this.pawnLabel : this.originalLabel);
            if (this.devGizmo)
            {
                this.defaultLabel = "DEV: " + this.defaultLabel;
            }
            Rect rect = new Rect(topLeft.x, topLeft.y, this.GetWidth(maxWidth), 75f);
            GizmoResult result = GizmoOnGUIInt(rect, parms);
            try
            {
                if (!this.disabled || !this.hideIconIfDisabled)
                {
                    Rect rect2 = new Rect(topLeft.x, topLeft.y, this.GetWidth(maxWidth), 75f);
                    Rect position = new Rect(rect2.x + rect2.width - 24f, rect2.y, 24f, 24f);
                    Texture2D image = this.isActive ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex;
                    GUI.DrawTexture(position, image);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"模组初始化异常: {ex.Message}\n堆栈跟踪: Checkbox");
            }

            try
            {
                if (result.State == GizmoState.Interacted && this.ability.CanCast)
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"模组初始化异常: {ex.Message}\n堆栈跟踪: result.State");
            }
            return new GizmoResult(result.State);
        }
        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            this.isActive = !this.isActive;
        }
        protected override void DisabledCheck()
        {
            if (this.Pawn.IsHavePowerRoot())
            {
                Hediff_RWrd_PowerRoot root = this.Pawn.GetPowerRoot();
                if (root.energy.FinalLevel == 0)
                {
                    this.DisableWithReason("RWrd_SDDisabledWeak".Translate());
                }
                else if (root.SDRecharge)
                {
                    this.DisableWithReason("RWrd_SDDisabledRecharge".Translate());
                }
                else
                {
                    this.disabled = false;
                }
            }
            else
            {
                this.DisableWithReason("RWrd_SDDisabledNon".Translate());
            }
        }
        public new bool isActive
        {
            get
            {
                if (this.Pawn.IsHavePowerRoot())
                {
                    return this.Pawn.GetPowerRoot().SelfDestruction;
                }
                return false;
            }
            set
            {
                if (this.Pawn.IsHavePowerRoot())
                {
                    this.Pawn.GetPowerRoot().SelfDestruction = value;
                }
            }
        }
    }
}
