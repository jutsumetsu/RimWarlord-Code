using Electromagnetic.Abilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Electromagnetic.Electromagnetic.UI
{
    [StaticConstructorOnStartup]
    public class Command_EMToggle : Command
    {
        public RWrd_PsyCastToggle Ability
        {
            get
            {
                return this.ability;
            }
        }
        public override Texture2D BGTexture
        {
            get
            {
                return Command_Ability.BGTex;
            }
        }
        public override Texture2D BGTextureShrunk
        {
            get
            {
                return Command_Ability.BGTexShrunk;
            }
        }
        public Pawn Pawn { get; private set; }
        public virtual string Tooltip
        {
            get
            {
                string text = this.ability.Tooltip;
                text += "\n" + "RWrd_Mastery".Translate() + Ability.mastery;
                return text;
            }
        }
        public override string TopRightLabel
        {
            get
            {
                return this.ability.GizmoExtraLabel;
            }
        }
        public override bool Disabled
        {
            get
            {
                this.DisabledCheck();
                return this.disabled;
            }
            set
            {
                this.disabled = value;
            }
        }
        protected virtual void DisabledCheck()
        {
            string str;
            this.disabled = this.ability.GizmoDisabled(out str);
            if (this.disabled)
            {
                this.DisableWithReason(str.CapitalizeFirst());
            }
        }
        protected void DisableWithReason(string reason)
        {
            this.disabledReason = reason;
            this.disabled = true;
        }
        public Command_EMToggle(RWrd_PsyCastToggle ability, Pawn pawn)
        {
            this.ability = ability;
            float num = 5f;
            AbilityCategoryDef category = ability.def.category;
            int? num2 = (category != null) ? new int?(category.displayOrder) : null;
            this.Order = num + ((num2 != null) ? ((float)num2.GetValueOrDefault()) : 0f) / 100f + (float)ability.def.displayOrder / 1000f + (float)ability.def.level / 10000f;
            this.defaultLabel = ability.def.LabelCap;
            this.hotKey = ability.def.hotKey;
            this.icon = ability.def.uiIcon;
            this.shrinkable = true;
            this.Pawn = pawn;
            this.originalLabel = this.defaultLabel;
            string text = this.LabelCap.Colorize(ColoredText.TipSectionTitleColor);
            if (pawn.Name != null)
            {
                string text2 = " (" + pawn.Name.ToStringShort + ")";
                this.pawnLabel = this.defaultLabel + text2;
                this.pawnTooltip = this.Tooltip.Insert(text.Length, text2);
                return;
            }
            this.pawnLabel = this.defaultLabel;
            this.pawnTooltip = this.Tooltip;
        }
        public override SoundDef CurActivateSound
        {
            get
            {
                if (this.isActive())
                {
                    return this.turnOffSound;
                }
                return this.turnOnSound;
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
            GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth, parms);
            if (!this.disabled || !this.hideIconIfDisabled)
            {
                Rect rect2 = new Rect(topLeft.x, topLeft.y, this.GetWidth(maxWidth), 75f);
                Rect position = new Rect(rect2.x + rect2.width - 24f, rect2.y, 24f, 24f);
                Texture2D image = this.isActive() ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex;
                GUI.DrawTexture(position, image);
            }
            if (result.State == GizmoState.Interacted && this.ability.CanCast)
            {
                return result;
            }
            return new GizmoResult(result.State);
        }
        protected override GizmoResult GizmoOnGUIInt(Rect butRect, GizmoRenderParms parms)
        {
            if (Mouse.IsOver(butRect))
            {
                if (parms.multipleSelected)
                {
                    if (this.Pawn.Map != null)
                    {
                        GenUI.DrawArrowPointingAtWorldspace(this.Pawn.DrawPos, Find.Camera);
                    }
                    this.defaultDesc = this.pawnTooltip;
                }
                else
                {
                    this.defaultDesc = this.Tooltip;
                }
            }
            this.DisabledCheck();
            return base.GizmoOnGUIInt(butRect, parms);
        }
        public override bool GroupsWith(Gizmo other)
        {
            Command_EMToggle command_Ability;
            return this.ability.def.groupAbility && (command_Ability = (other as Command_EMToggle)) != null && command_Ability.ability.def == this.ability.def;
        }
        public override bool ShowPawnDetailsWith(Gizmo other)
        {
            Command_EMToggle command_Ability;
            return (command_Ability = (other as Command_EMToggle)) != null && command_Ability.ability.def == this.ability.def;
        }
        public virtual void GroupAbilityCommands(List<Gizmo> group)
        {
            if (this.groupedCasts == null)
            {
                this.groupedCasts = new List<Command_EMToggle>();
            }
            this.groupedCasts.Clear();
            for (int i = 0; i < group.Count; i++)
            {
                if (group[i].GroupsWith(this))
                {
                    this.groupedCasts.Add((Command_EMToggle)group[i]);
                }
            }
        }
        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            this.Ability.ApplyToggles();
        }
        public override bool InheritInteractionsFrom(Gizmo other)
        {
            Command_EMToggle command_Toggle = other as Command_EMToggle;
            return command_Toggle != null && command_Toggle.isActive() == this.isActive();
        }
        protected RWrd_PsyCastToggle ability;
        private List<Command_EMToggle> groupedCasts;
        private string pawnLabel;
        private string originalLabel;
        private string pawnTooltip;
        public bool devGizmo;
        public new static readonly Texture2D BGTex = ContentFinder<Texture2D>.Get("UI/Widgets/AbilityButBG", true);
        public new static readonly Texture2D BGTexShrunk = ContentFinder<Texture2D>.Get("UI/Widgets/AbilityButBGShrunk", true);
        public Func<bool> isActive;
        public SoundDef turnOnSound = SoundDefOf.Checkbox_TurnedOn;
        public SoundDef turnOffSound = SoundDefOf.Checkbox_TurnedOff;
        public bool activateIfAmbiguous = true;
        public bool hideIconIfDisabled;
    }
}
