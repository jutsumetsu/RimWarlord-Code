using Electromagnetic.Abilities;
using Electromagnetic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Electromagnetic.UI
{
    public class Dialog_HeavenLock : Window
    {
        public Dialog_HeavenLock(Pawn caster, Pawn victim,bool surpriseAttack = false)
        {
            this.caster = caster;
            this.victim = victim;
            this.surpriseAttack = surpriseAttack;
            forcePause = true;
        }
        public override void DoWindowContents(Rect inRect)
        {
            // 标题
            var dialogTitleLabel = new Rect(inRect.x, inRect.y, 300, 24);
            Widgets.Label(dialogTitleLabel, "RWrd_Locking".Translate(caster.Name.ToStringShort,victim.Name.ToStringShort));
            Rect outRect = new Rect(inRect);
            outRect.y += 30;
            outRect.yMax -= 70f;
            outRect.width -= 16f;
            int num = 35;
            // 获取两者力量之源
            Hediff_RWrd_PowerRoot rootCaster = this.caster.GetPowerRoot();
            Hediff_RWrd_PowerRoot rootVictim = this.victim.GetPowerRoot();
            // 设置匹数上限（文本）
            Text.Font = GameFont.Small;
            Rect txtRect = new Rect(0f, num, outRect.width * 0.25f, 32f);
            Widgets.LabelFit(txtRect, "RWrd_SetLockLevel".Translate());
            // 获取初始上限数值
            int lockedLevel;
            float lockedRealm;
            // 计算可限制范围
            int limitRange = (rootCaster.energy.level - rootVictim.energy.level) * 2;
            int limitMin = Math.Max(0, rootVictim.energy.level - limitRange);
            if (surpriseAttack && rootCaster.energy.level <= rootVictim.energy.level)
            {
                limitMin = rootVictim.energy.level;
            }
            /*Log.Message($"Limit Range: {limitRange}, Min: {limitMin}, Max: {rootVictim.energy.level}");*/
            // 设置滑条和文本框矩形
            Rect sliderRect = new Rect(142f, num + 8f, 180f, 32f);
            Rect textFieldNumeric = new Rect(sliderRect.xMax + 30f, num, 60f, 32f);
            int textFieldNumber = this.limitNum;
            string text = textFieldNumber.ToString();
            // 设置文本框
            Widgets.TextFieldNumeric<int>(textFieldNumeric, ref textFieldNumber, ref text, limitMin, rootVictim.energy.level);
            /*Log.Message($"Text Field Number: {textFieldNumber}");*/
            this.limitNum = textFieldNumber;
            // 设置滑条
            this.limitNum = (int)Widgets.HorizontalSlider(sliderRect, this.limitNum, limitMin, rootVictim.energy.level, false, null, null, null, 1f);
            /*Log.Message($"Limit Num after Slider: {this.limitNum}");*/
            // 下移一行
            num += 35;
            // 设置勾选框
            Rect checkboxRect = new Rect(0, num, outRect.width * 0.25f, 32f);
            int casterLevel = rootCaster.energy.level + rootCaster.energy.FinalLevel;
            if (existHeavenLock)
            {
                lockedLevel = heavenLock.casterLevel;
                lockedRealm = heavenLock.casterCompleteRealm;
                if (casterLevel >= lockedLevel || rootCaster.energy.completerealm >= lockedRealm)
                {
                    Widgets.CheckboxLabeled(checkboxRect, "Unlocks".Translate(), ref this.removeHeavenLock);
                }
            }
            // 设置确认按钮
            Rect rectConfirm = new Rect(this.windowRect.AtZero().width / 2f - Window.CloseButSize.x / 2f, this.windowRect.AtZero().height - 85f, Window.CloseButSize.x, Window.CloseButSize.y);
            if (Widgets.ButtonText(rectConfirm, "AcceptButton".Translate()))
            {
                if (!removeHeavenLock)
                {
                    if (existHeavenLock)
                    {
                        bool flag1 = false;
                        bool flag2 = false;
                        lockedLevel = heavenLock.casterLevel;
                        lockedRealm = heavenLock.casterCompleteRealm;
                        if (casterLevel >= lockedLevel)
                        {
                            flag1 = true;
                        }
                        if (rootCaster.energy.completerealm  >= lockedRealm)
                        {
                            flag2 = true;
                        }
                        if (flag1 && flag2)
                        {
                            heavenLock.root = rootCaster;
                            heavenLock.casterLevel = casterLevel;
                            heavenLock.casterCompleteRealm = rootCaster.energy.completerealm;
                        }
                    }
                    else
                    {
                        victim.health.AddHediff(heavenLock);
                    }
                    rootVictim.energy.powerLimit = this.limitNum;
                }
                else
                {
                    victim.health.RemoveHediff(heavenLock);
                }
                victim.UpdatePowerRootStageInfo();
                this.abnormalClose = false;
                this.Close(true);
            }
        }
        public override void PreOpen()
        {
            base.PreOpen();
            // 获取两者力量之源
            Hediff_RWrd_PowerRoot rootCaster = this.caster.GetPowerRoot();
            Hediff_RWrd_PowerRoot rootVictim = this.victim.GetPowerRoot();
            if (rootVictim.energy.availableLevel != rootVictim.energy.level)
            {
                this.limitNum = rootVictim.energy.availableLevel;
            }
            else
            {
                this.limitNum = rootVictim.energy.level;
            }
            // 获取磁场天锁状态
            if (victim.IsLockedByEMPower())
            {
                heavenLock = victim.GetHeavenLock();
                existHeavenLock = true;
            }
            else
            {
                heavenLock = (Hediff_HeavenLock)Tools.MakeEMHediff(RWrd_DefOf.RWrd_HeavenLock, victim, rootCaster, null);
                heavenLock.surpriseAttack = this.surpriseAttack;
            }
        }
        public override void PostClose()
        {
            base.PostClose();
            if (abnormalClose)
            {
                if (!this.victim.IsLockedByEMPower())
                {
                    victim.GetPowerRoot().energy.powerLimit = 999;
                }
            }
        }

        int limitNum = 0;
        Pawn caster;
        Pawn victim;
        bool surpriseAttack;
        bool removeHeavenLock = false;
        bool abnormalClose = true;
        bool existHeavenLock = false;
        Hediff_HeavenLock heavenLock;
    }
}
