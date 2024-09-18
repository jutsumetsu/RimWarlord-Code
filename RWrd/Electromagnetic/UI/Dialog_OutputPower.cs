using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;
using Electromagnetic.Abilities;
using Electromagnetic.Core;
using Electromagnetic.Setting;

namespace Electromagnetic.UI
{
    public class Dialog_OutputPower : Window
    {
        private Vector2 scrollPosition;
        private readonly ITab_Pawn_RWrd parent;
        public Dialog_OutputPower(ITab_Pawn_RWrd parent)
        {
            doCloseButton = false;
            doCloseX = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = false;
            forcePause = true;
            this.parent = parent;
        }
        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Text.Anchor = TextAnchor.MiddleLeft;
            Pawn pawn = parent.pawn;
            if (parent.GetSelectedPawn() == null)
            {
                Close();
            }
            List<RWrd_PsyCastBase> abilities = new List<RWrd_PsyCastBase>();
            foreach (Ability ability in pawn.abilities.abilities)
            {
                if (ability is RWrd_PsyCastBase && ability.CompOfType<CompAbilityEffect_ReduceEnergy>() is CompAbilityEffect_ReduceEnergy compAbilityEffect_ReduceEnergy)
                {
                    if (compAbilityEffect_ReduceEnergy.Props.isAttack)
                    {
                        abilities.Add(ability as RWrd_PsyCastBase);
                    }
                }
            }
            var dialogTitleLabel = new Rect(inRect.x, inRect.y, 300, 24);
            Widgets.Label(dialogTitleLabel, "RWrd_OutputPower".Translate() + "(" + pawn.Name.ToStringShort + ")");
            Rect outRect = new Rect(inRect);
            outRect.y += 30;
            outRect.yMax -= 70f;
            outRect.width -= 16f;
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, abilities.Count() * 35f);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            float num = 0f;
            //图标矩形
            Rect iconRect = new Rect(0f, num, 32, 32);
            //近战攻击对应征召图标
            GUI.DrawTexture(iconRect, TexCommand.Draft);
            //技能名称矩形
            Rect rect = new Rect(iconRect.xMax + 5, num, viewRect.width * 0.7f, 32f);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.LabelFit(rect, "RWrd_CombatAttack".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            rect.x = rect.xMax + 10;
            rect.width = 100;
            //滑条绘制
            Rect sliderRect = new Rect(iconRect.xMax + 110f, num + 10f, 180f, 32f);
            Widgets.Label(new Rect(sliderRect.xMax + 45f, num + 10f, sliderRect.width, 24f), pawn.GetPowerRoot().energy.outputPower.ToString("P2"));
            pawn.GetPowerRoot().energy.outputPower = Widgets.HorizontalSlider(sliderRect, pawn.GetPowerRoot().energy.outputPower, 0f, 1f);
            num += 35f;
            if (RWrdSettings.PowerfulEnergyWave)
            {
                //绘制图标
                iconRect = new Rect(0f, num, 32, 32);
                GUI.DrawTexture(iconRect, ContentFinder<Texture2D>.Get("UI/Gizmos/EMPower", true));
                //名称显示
                rect = new Rect(iconRect.xMax + 5, num, 105f, 32f);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.CheckboxLabeled(rect, "RWrd_EnergyWave".Translate(), ref pawn.GetPowerRoot().energy.personalEnergyWave);
                Text.Anchor = TextAnchor.UpperLeft;
                rect.x = rect.xMax + 10;
                rect.width = 100;
                if (pawn.GetPowerRoot().energy.personalEnergyWave)
                {
                    //滑条绘制
                    sliderRect = new Rect(iconRect.xMax + 110f, num + 10f, 180f, 32f);
                    Widgets.Label(new Rect(sliderRect.xMax + 45f, num + 10f, sliderRect.width, 24f), pawn.GetPowerRoot().energy.wavePower.ToString("P2"));
                    pawn.GetPowerRoot().energy.wavePower = Widgets.HorizontalSlider(sliderRect, pawn.GetPowerRoot().energy.wavePower, 0f, 1f);
                    num += 35f;
                    //绘制图标
                    iconRect = new Rect(0f, num, 32, 32);
                    GUI.DrawTexture(iconRect, ContentFinder<Texture2D>.Get("UI/Gizmos/PowerWave", true));
                    //名称显示
                    rect = new Rect(iconRect.xMax + 5, num, viewRect.width * 0.7f, 32f);
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.LabelFit(rect, "RWrd_WaveRange".Translate());
                    Text.Anchor = TextAnchor.UpperLeft;
                    rect.x = rect.xMax + 10;
                    rect.width = 100;
                    //滑条绘制
                    sliderRect = new Rect(iconRect.xMax + 110f, num + 10f, 180f, 32f);
                    Widgets.Label(new Rect(sliderRect.xMax + 45f, num + 10f, sliderRect.width, 24f), pawn.GetPowerRoot().energy.waveRange.ToString("P2"));
                    pawn.GetPowerRoot().energy.waveRange = Widgets.HorizontalSlider(sliderRect, pawn.GetPowerRoot().energy.waveRange, 0f, 2f);
                    num += 35f;
                }
                else
                {
                    num += 35f;
                }
            }
            foreach (RWrd_PsyCastBase ability1 in abilities)
            {
                //绘制图标
                iconRect = new Rect(0f, num, 32, 32);
                GUI.DrawTexture(iconRect, ContentFinder<Texture2D>.Get(ability1.def.iconPath, true));
                //名称显示
                rect = new Rect(iconRect.xMax + 5, num, viewRect.width * 0.7f, 32f);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.LabelFit(rect, ability1.def.LabelCap);
                Text.Anchor = TextAnchor.UpperLeft;
                rect.x = rect.xMax + 10;
                rect.width = 100;
                //滑条绘制
                sliderRect = new Rect(iconRect.xMax + 110f, num + 10f, 180f, 32f);
                Widgets.Label(new Rect(sliderRect.xMax + 45f, num + 10f, sliderRect.width, 24f), ability1.outputPower.ToString("P2"));
                ability1.outputPower = Widgets.HorizontalSlider(sliderRect, ability1.outputPower, 0f, 2f);
                num += 35f;
            }
            Widgets.EndScrollView();
        }
    }
}
