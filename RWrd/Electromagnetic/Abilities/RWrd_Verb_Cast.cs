using System;
using System.Collections.Generic;
using Electromagnetic.Core;
using RimWorld;
using UnityEngine;
using Verse;

namespace Electromagnetic.Abilities
{
    public class RWrd_Verb_Cast : Verb_CastAbility
    {
        public RWrd_PsycastOrigin Psycast
        {
            get
            {
                return this.ability as RWrd_PsycastOrigin;
            }
        }
        public override bool IsApplicableTo(LocalTargetInfo target, bool showMessages = false)
        {
            bool flag = !base.IsApplicableTo(target, showMessages);
            bool result;
            if (flag)
            {
                result = false;
            }
            else
            {
                bool flag2 = !this.Psycast.def.HasAreaOfEffect && !this.Psycast.CanApplyPsycastTo(target);
                if (flag2)
                {
                    if (showMessages)
                    {
                        Messages.Message(this.ability.def.LabelCap + ": " + "AbilityTargetPsychicallyDeaf".Translate(), target.ToTargetInfo(this.ability.pawn.Map), MessageTypeDefOf.RejectInput, false);
                    }
                    result = false;
                }
                else
                {
                    foreach (AbilityComp abilityComp in this.Psycast.comps)
                    {
                        bool flag3 = abilityComp.props.GetType() == typeof(CompProperties_SpawnMoteCasting);
                        if (flag3)
                        {
                            CompProperties_SpawnMoteCasting compProperties_SpawnMoteCasting = (CompProperties_SpawnMoteCasting)abilityComp.props;
                            ThingDef moteCastDef = compProperties_SpawnMoteCasting.moteCastDef;
                            bool spawned = this.caster.Spawned;
                            if (spawned)
                            {
                                bool flag4 = compProperties_SpawnMoteCasting.moteCastDef != null;
                                if (flag4)
                                {
                                    this.timePast = Time.time - this.timeSet;
                                    bool flag5 = this.timePast >= 0.3f + RWrd_Verb_Cast.MoteCastFadeTime;
                                    if (flag5)
                                    {
                                        MoteMaker.MakeAttachedOverlay(this.caster, moteCastDef, RWrd_Verb_Cast.MoteCastOffset, RWrd_Verb_Cast.MoteCastScale, 0.3f - RWrd_Verb_Cast.MoteCastFadeTime);
                                        this.timeSet = Time.time;
                                    }
                                }
                            }
                        }
                    }
                    result = true;
                }
            }
            return result;
        }
        public override void OrderForceTarget(LocalTargetInfo target)
        {
            bool flag = this.IsApplicableTo(target, false);
            if (flag)
            {
                base.OrderForceTarget(target);
            }
        }
        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            bool flag = !base.ValidateTarget(target, showMessages);
            return !flag;
        }
        public override void OnGUI(LocalTargetInfo target)
        {
            bool flag = this.ability.EffectComps.Any((CompAbilityEffect e) => e.Props.psychic);
            Texture2D texture2D = this.UIIcon;
            bool flag2 = !this.Psycast.CanApplyPsycastTo(target);
            if (flag2)
            {
                texture2D = TexCommand.CannotShoot;
                this.DrawIneffectiveWarning(target);
            }
            bool flag3 = target.IsValid && this.CanHitTarget(target) && this.IsApplicableTo(target, false);
            if (flag3)
            {
                bool flag4 = flag;
                if (flag4)
                {
                    foreach (LocalTargetInfo target2 in this.ability.GetAffectedTargets(target))
                    {
                        bool flag5 = this.Psycast.CanApplyPsycastTo(target2);
                        if (flag5)
                        {
                            this.DrawSensitivityStat(target2);
                        }
                        else
                        {
                            this.DrawIneffectiveWarning(target2);
                        }
                    }
                }
                bool flag6 = this.ability.EffectComps.Any((CompAbilityEffect e) => !e.Valid(target, false));
                if (flag6)
                {
                    texture2D = TexCommand.CannotShoot;
                }
            }
            else
            {
                texture2D = TexCommand.CannotShoot;
            }
            bool flag7 = this.Psycast.def.DetectionChance > 0f;
            if (flag7)
            {
                TaggedString taggedString = "Illegal".ToUpper().Translate() + "\n" + this.Psycast.def.DetectionChance.ToStringPercent() + " " + "DetectionChance".Translate();
                Text.Font = GameFont.Small;
                GenUI.DrawMouseAttachment(texture2D, taggedString, 0f, default(Vector2), null, true, new Color(0.25f, 0f, 0f), null, null);
            }
            else
            {
                GenUI.DrawMouseAttachment(texture2D);
            }
            base.DrawAttachmentExtraLabel(target);
        }
        private void DrawIneffectiveWarning(LocalTargetInfo target)
        {
            bool flag = target.Pawn != null;
            if (flag)
            {
                Vector3 drawPos = target.Pawn.DrawPos;
                drawPos.z += 1f;
                GenMapUI.DrawText(new Vector2(drawPos.x, drawPos.z), "Ineffective".Translate(), Color.red);
            }
        }
        private void DrawSensitivityStat(LocalTargetInfo target)
        {
            bool flag = target.Pawn != null;
            if (flag)
            {
                Pawn pawn = target.Pawn;
                float statValue = pawn.GetStatValue(StatDefOf.PsychicSensitivity, true, -1);
                Vector3 drawPos = pawn.DrawPos;
                drawPos.z += 1f;
                GenMapUI.DrawText(new Vector2(drawPos.x, drawPos.z), StatDefOf.PsychicSensitivity.LabelCap + ": " + statValue.ToString(), (statValue > float.Epsilon) ? Color.white : Color.red);
            }
        }
        private const float StatLabelOffsetY = 1f;
        private static float MoteCastFadeTime = 0.4f;
        private float timeSet = Time.time;
        private float timePast;
        private static float MoteCastScale = 1f;
        private static Vector3 MoteCastOffset = new Vector3(0f, 0f, 0.87f);
    }
}
