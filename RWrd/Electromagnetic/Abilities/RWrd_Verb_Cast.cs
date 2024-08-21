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
            // 首先调用基类的 IsApplicableTo 方法进行基本的适用性检查
            bool flag = !base.IsApplicableTo(target, showMessages);

            bool result;

            if (flag)
            {
                // 如果基类检查返回不适用，直接返回 false
                result = false;
            }
            else
            {
                // 检查 Psycast 是否有区域效果，并且 Psycast 是否能应用到目标
                bool flag2 = !this.Psycast.def.HasAreaOfEffect && !this.Psycast.CanApplyPsycastTo(target);

                if (flag2)
                {
                    // 如果 Psycast 没有区域效果且不能应用到目标，则进行以下处理
                    if (showMessages)
                    {
                        // 显示消息，说明能力无法对目标应用
                        Messages.Message(
                            this.ability.def.LabelCap + ": " + "AbilityTargetPsychicallyDeaf".Translate(),
                            target.ToTargetInfo(this.ability.pawn.Map),
                            MessageTypeDefOf.RejectInput,
                            false
                        );
                    }
                    result = false;
                }
                else
                {
                    // 遍历 Psycast 的所有能力组件
                    foreach (AbilityComp abilityComp in this.Psycast.comps)
                    {
                        // 检查能力组件的属性是否是 CompProperties_SpawnMoteCasting 类型
                        bool flag3 = abilityComp.props.GetType() == typeof(CompProperties_SpawnMoteCasting);

                        if (flag3)
                        {
                            // 强制转换为 CompProperties_SpawnMoteCasting 类型
                            CompProperties_SpawnMoteCasting compProperties_SpawnMoteCasting = (CompProperties_SpawnMoteCasting)abilityComp.props;

                            // 获取该组件的 Mote 定义
                            ThingDef moteCastDef = compProperties_SpawnMoteCasting.moteCastDef;

                            // 检查施法者是否已经生成（即是否存在于场景中）
                            bool spawned = this.caster.Spawned;

                            if (spawned)
                            {
                                // 确保 Mote 定义不为 null
                                bool flag4 = compProperties_SpawnMoteCasting.moteCastDef != null;

                                if (flag4)
                                {
                                    // 计算时间差，判断是否满足生成新 Mote 的条件
                                    this.timePast = Time.time - this.timeSet;
                                    bool flag5 = this.timePast >= 0.3f + RWrd_Verb_Cast.MoteCastFadeTime;

                                    if (flag5)
                                    {
                                        // 生成并附加 Mote 覆盖物
                                        MoteMaker.MakeAttachedOverlay(
                                            this.caster,
                                            moteCastDef,
                                            RWrd_Verb_Cast.MoteCastOffset,
                                            RWrd_Verb_Cast.MoteCastScale,
                                            0.3f - RWrd_Verb_Cast.MoteCastFadeTime
                                        );

                                        // 更新时间戳
                                        this.timeSet = Time.time;
                                    }
                                }
                            }
                        }
                    }

                    // 如果所有检查通过，返回 true
                    result = true;
                }
            }

            // 返回最终的适用性检查结果
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
