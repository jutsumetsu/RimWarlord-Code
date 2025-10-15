using Electromagnetic.Things;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;

namespace Electromagnetic.Core
{
    public class JobDriver_RWrd_StudyBook : JobDriver_StudyItem
    {
        protected Thing Book
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
                return this.Book.TryGetComp<CompThingStudiable>();
            }
        }
        // 预留工作所需资源（物品、位置等）
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.Book, this.job, 1, 1, null, errorOnFailed, false);
        }
        // 创建工作流程（任务序列）
        protected override IEnumerable<Toil> MakeNewToils()
        {
            // 设置最终处理逻辑（工作结束时）
            base.SetFinalizerJob(delegate (JobCondition condition)
            {
                // 如果角色没有携带书籍则返回
                if (!this.pawn.IsCarryingThing(this.Book))
                {
                    return null;
                }
                // 如果工作未成功完成，放下书籍
                if (condition != JobCondition.Succeeded)
                {
                    Thing thing;
                    this.pawn.carryTracker.TryDropCarriedThing(this.pawn.Position, ThingPlaceMode.Direct, out thing, null);
                    return null;
                }
                // 成功时将书籍存入仓库
                return HaulAIUtility.HaulToStorageJob(this.pawn, this.Book, false);
            });

            // 准备阅读书籍的步骤
            foreach (Toil toil in this.PrepareToReadBook())
            {
                yield return toil;
            }

            // 步骤6：执行核心研究行为
            yield return this.ReadBook();
            yield break;
        }
        public override void Notify_Starting()
        {
            base.Notify_Starting();
            this.job.count = 1;
        }

        // 准备阅读书籍的步骤
        private IEnumerable<Toil> PrepareToReadBook()
        {
            // 如果已携带书籍则跳过
            if (this.carrying)
            {
                yield break;
            }

            // 如果书籍在物品栏中，取出书籍
            if (this.hasInInventory)
            {
                yield return Toils_Misc.TakeItemFromInventoryToCarrier(this.pawn, TargetIndex.A);
            }
            else
            {
                // 移动到书籍位置
                yield return Toils_Goto.GotoCell(this.Book.PositionHeld, PathEndMode.ClosestTouch)
                    .FailOnDestroyedOrNull(TargetIndex.A)
                    .FailOnSomeonePhysicallyInteracting(TargetIndex.A);

                // 拾取书籍
                yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, false, false, true, true);
            }

            // 携带书籍到阅读位置
            yield return this.CarryToReadingSpot().FailOnDestroyedOrNull(TargetIndex.A);

            // 寻找相邻的阅读表面
            yield return this.FindAdjacentReadingSurface();

            yield break;
        }

        // 创建研究行为的工作单元
        private Toil ReadBook()
        {
            Toil study = ToilMaker.MakeToil("StudyToil");
            // 失败条件：书籍被销毁或禁用
            study.FailOnDestroyedNullOrForbidden(TargetIndex.A);
            study.handlingFacing = true;
            // 初始化动作
            study.initAction = delegate ()
            {
                this.pawn.pather.StopDead();
                this.job.showCarryingInspectLine = false;
            };
            // 每帧执行的研究逻辑
            study.tickAction = delegate ()
            {
                // 处理角色朝向
                if (this.job.GetTarget(TargetIndex.B).IsValid)
                {
                    this.pawn.rotationTracker.FaceCell(this.job.GetTarget(TargetIndex.B).Cell);
                }
                else if (this.Book.Spawned)
                {
                    this.pawn.rotationTracker.FaceCell(this.Book.Position);
                }
                else if (this.pawn.Rotation == Rot4.North)
                {
                    this.pawn.Rotation = new Rot4(Rand.Range(1, 4));
                }

                // 获取阅读加成
                float readingBonus = BookUtility.GetReadingBonus(this.pawn);
                this.isReading = true;

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
                    if (this.StudyComp.Props.Qigong)
                    {
                        if (!pawn.IsHavePowerRoot())
                        {
                            Hediff hediff = Tools.MakePowerRoot(RWrd_DefOf.Hediff_RWrd_PowerRoot, pawn, true);
                            pawn.health.AddHediff(hediff);
                        }
                    }
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

        // 携带书籍到阅读位置
        private Toil CarryToReadingSpot()
        {
            Toil toil = ToilMaker.MakeToil("CarryToReadingSpot");
            toil.initAction = delegate ()
            {
                IntVec3 intVec;
                // 尝试寻找最近的椅子位置
                if (!this.TryGetClosestChairFreeSittingSpot(true, out intVec) &&
                    !this.TryGetClosestChairFreeSittingSpot(false, out intVec))
                {
                    // 找不到椅子时寻找站立位置
                    intVec = RCellFinder.SpotToChewStandingNear(this.pawn, this.Book,
                        (IntVec3 c) => !c.Fogged(this.pawn.Map) &&
                        this.pawn.CanReserveSittableOrSpot(c, false));
                }

                if (!intVec.IsValid)
                {
                    this.pawn.pather.StartPath(this.pawn.Position, PathEndMode.OnCell);
                    return;
                }

                // 预留位置并开始移动
                this.pawn.ReserveSittableOrSpot(intVec, this.pawn.CurJob, true);
                this.pawn.Map.pawnDestinationReservationManager.Reserve(this.pawn, this.pawn.CurJob, intVec);
                this.pawn.pather.StartPath(intVec, PathEndMode.OnCell);
            };
            toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            return toil;
        }

        // 尝试获取最近的椅子位置
        private bool TryGetClosestChairFreeSittingSpot(bool skipInteractionCells, out IntVec3 cell)
        {
            Thing thing = GenClosest.ClosestThingReachable(this.pawn.Position, this.pawn.Map,
                ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial),
                PathEndMode.OnCell, TraverseParms.For(this.pawn, Danger.Deadly, TraverseMode.ByPawn, false, false, false, true),
                32f, (Thing t) => JobDriver_RWrd_StudyBook.ValidateChair(t, this.pawn, skipInteractionCells) &&
                t.Position.GetDangerFor(this.pawn, t.Map) == Danger.None, null, 0, -1, false, RegionType.Set_Passable, false, false);

            if (thing != null)
            {
                return JobDriver_RWrd_StudyBook.TryFindFreeSittingSpotOnThing(thing, this.pawn, skipInteractionCells, out cell);
            }
            cell = IntVec3.Invalid;
            return false;
        }

        // 寻找相邻的阅读表面
        private Toil FindAdjacentReadingSurface()
        {
            Toil toil = ToilMaker.MakeToil("FindAdjacentReadingSurface");
            toil.initAction = delegate ()
            {
                Map map = this.pawn.Map;
                IntVec3 position = this.pawn.Position;
                Building firstThing = this.pawn.Position.GetFirstThing<Building>(this.pawn.Map);

                // 如果没有可坐的表面，尝试面对最近的表面
                if (firstThing == null || firstThing.def.building == null || !firstThing.def.building.isSittable)
                {
                    this.TryFaceClosestSurface(position, map);
                    return;
                }

                // 尝试面对最近的表面
                if (this.TryFaceClosestSurface(position, map))
                {
                    return;
                }

                // 设置面对目标
                this.job.SetTarget(TargetIndex.B, position + firstThing.Rotation.FacingCell);
                this.pawn.jobs.curDriver.rotateToFace = TargetIndex.B;
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }

        // 尝试面对最近的表面
        private bool TryFaceClosestSurface(IntVec3 pos, Map map)
        {
            // 检查四个方向寻找可用的表面
            for (int i = 0; i < 4; i++)
            {
                Rot4 rot = new Rot4(i);
                IntVec3 c = pos + rot.FacingCell;
                if (c.GetSurfaceType(map) == SurfaceType.Eat)
                {
                    this.job.SetTarget(TargetIndex.B, c);
                    this.pawn.jobs.curDriver.rotateToFace = TargetIndex.B;
                    return true;
                }
            }

            for (int j = 0; j < 4; j++)
            {
                Rot4 rot2 = new Rot4(j);
                IntVec3 c2 = pos + rot2.FacingCell;
                if (c2.GetSurfaceType(map) == SurfaceType.Item)
                {
                    this.job.SetTarget(TargetIndex.B, c2);
                    this.pawn.jobs.curDriver.rotateToFace = TargetIndex.B;
                    return true;
                }
            }
            return false;
        }

        // 验证椅子是否可用
        private static bool ValidateChair(Thing t, Pawn pawn, bool skipInteractionCells)
        {
            IntVec3 intVec;
            return t.def.building != null &&
                   t.def.building.isSittable &&
                   JobDriver_RWrd_StudyBook.TryFindFreeSittingSpotOnThing(t, pawn, skipInteractionCells, out intVec) &&
                   !t.Fogged() &&
                   !t.IsForbidden(pawn) &&
                   pawn.CanReserve(t, 1, -1, null, false) &&
                   t.IsSociallyProper(pawn) &&
                   !t.IsBurning() &&
                   !t.HostileTo(pawn);
        }

        // 尝试在物体上找到空闲座位
        private static bool TryFindFreeSittingSpotOnThing(Thing t, Pawn pawn, bool skipInteractionCells, out IntVec3 cell)
        {
            foreach (IntVec3 intVec in t.OccupiedRect())
            {
                if ((!skipInteractionCells || !intVec.IsBuildingInteractionCell(pawn.Map)) &&
                    !intVec.Fogged(pawn.Map) &&
                    pawn.CanReserveSittableOrSpot(intVec, false))
                {
                    cell = intVec;
                    return true;
                }
            }
            cell = default(IntVec3);
            return false;
        }

        // 保存数据
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.carrying, "carrying", false, false);
            Scribe_Values.Look<bool>(ref this.hasInInventory, "hasInInventory", false, false);
        }

        // 成员变量
        private bool hasInInventory;     // 书籍是否在物品栏中
        private bool carrying;           // 是否正携带书籍
        private bool isReading;          // 是否正在阅读
        private const TargetIndex SurfaceIndex = TargetIndex.B;
        protected const TargetIndex StudiableInd = TargetIndex.A;

        // 研究参数常量
        private const float DefaultResearchSpeed = 0.08f;  // 默认研究速度
        private const int JobEndInterval = 4000;           // 工作持续时间
    }
}
