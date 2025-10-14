using Electromagnetic.Things;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Electromagnetic.Core
{
    public class JobDriver_RWrd_ResearchDisc : JobDriver_StudyItem
    {
        protected Thing StudyThing
        {
            get
            {
                return base.TargetThingA;
            }
        }
        protected CompThingStudiable StudyComp
        {
            get
            {
                return this.StudyThing.TryGetComp<CompThingStudiable>();
            }
        }
        // 预留工作所需资源（物品、位置等）
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            // 同时预留研究物品（A）、研究台（B）和放置位置（C）
            bool flag = this.pawn.Reserve(base.TargetThingA, this.job, 1, -1, null, errorOnFailed, false) && this.pawn.Reserve(base.TargetB, this.job, 1, -1, null, errorOnFailed, false);
            return flag && this.pawn.Reserve(base.TargetC, this.job, 1, -1, null, errorOnFailed, false);
        }
        // 创建工作流程（任务序列）
        protected override IEnumerable<Toil> MakeNewToils()
        {
            // 失败条件：研究台被销毁/禁用或起火
            this.FailOnDespawnedNullOrForbidden(TargetIndex.B);
            this.FailOnBurningImmobile(TargetIndex.B);
            // 步骤1：设置工作物品数量为1
            yield return Toils_General.DoAtomic(delegate
            {
                this.job.count = 1;
            });
            // 步骤2：移动到研究物品旁
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch, false).FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            // 步骤3：拾取研究物品
            yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, true, false, true, false).FailOnDestroyedNullOrForbidden(TargetIndex.A);
            // 步骤4：移动到研究台
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.InteractionCell, false);
            // 步骤5：将物品放置到研究台指定位置
            yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, null, false, false);
            // 步骤6：执行核心研究行为
            yield return this.ResearchStudyToil();
            yield break;
        }
        // 创建研究行为的工作单元
        private Toil ResearchStudyToil()
        {
            Toil study = ToilMaker.MakeToil("StudyToil");
            // 每帧执行的研究逻辑
            study.tickAction = delegate ()
            {
                Pawn actor = study.actor;
                float num = 0.08f;  // 基础研究速度

                // 如果角色能进行研究工作，使用角色实际研究速度
                bool flag = !actor.WorkTypeIsDisabled(WorkTypeDefOf.Research);
                if (flag)
                {
                    num = actor.GetStatValue(StatDefOf.ResearchSpeed, true, -1);
                }

                // 应用物品的研究速度加成
                num *= this.TargetThingA.GetStatValue(StatDefOf.ResearchSpeedFactor, true, -1);

                // 执行研究进度更新
                this.StudyComp.Study(num, actor);

                // 研究完成时的处理
                bool completed = this.StudyComp.Completed;
                if (completed)
                {
                    this.StudyComp.LearnAbilities(this.pawn);
                    if (this.StudyComp.Props.route != null)
                    {
                        Hediff_RWrd_PowerRoot root = this.pawn.GetPowerRoot();
                        root.UnlockRoute(this.StudyComp.Props.route);
                    }
                    this.pawn.CheckEMAbilityLimiting();

                    // 准备进入下一工作阶段
                    this.pawn.jobs.curDriver.ReadyForNextToil();
                }
            };

            // 失败条件：无法接触研究物品
            study.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            study.WithProgressBar(TargetIndex.A, () => this.StudyComp.ProgressPercent, false, -0.5f, false);

            // 设置工作参数
            study.defaultCompleteMode = ToilCompleteMode.Delay;  // 基于时间完成
            study.defaultDuration = 4000;  // 持续4000ticks（约66.67秒）
            study.activeSkill = (() => SkillDefOf.Intellectual);
            return study;
        }
        private new const TargetIndex ResearchBenchInd = TargetIndex.B;  // 研究台
        private new const TargetIndex HaulCell = TargetIndex.C;          // 物品放置位置
        protected const TargetIndex StudiableInd = TargetIndex.A;        // 研究物品

        // 研究参数常量
        private const float DefaultResearchSpeed = 0.08f;  // 默认研究速度
        private const int JobEndInterval = 4000;           // 工作持续时间
    }
}
