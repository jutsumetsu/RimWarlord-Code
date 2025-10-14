using Electromagnetic.Core;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Electromagnetic.Things
{
    public class CompThingStudiable : ThingComp
    {
        public CompProperties_ThingStudiable Props
        {
            get
            {
                return (CompProperties_ThingStudiable)this.props;
            }
        }
        public Pawn student = null;
        //学习进度百分比
        public float ProgressPercent
        {
            get
            {
                return this.studyManager.GetStudyProgress(this.parent.def, student);
            }
        }
        //是否完成
        public bool Completed
        {
            get
            {
                return this.ProgressPercent >= 1f;
            }
        }
        //整数进度
        public int ProgressInt
        {
            get
            {
                return Mathf.RoundToInt(this.ProgressPercent * (float)this.Props.cost);
            }
        }
        //学习功能实现
        public void Study(float amount, Pawn studier = null)
        {
            student = studier;
            amount *= 0.00825f;
            amount *= Find.Storyteller.difficulty.researchSpeedFactor;
            this.studyManager.SetStudied(this.parent.def, studier, amount);
            if (studier != null)
            {
                studier.skills.Learn(SkillDefOf.Intellectual, 0.1f, false, false);
            }
        }
        //学习能力
        public void LearnAbilities(Pawn studier = null)
        {
            bool flag = studier == null;
            if (!flag)
            {
                if (this.Props.ability != null)
                {
                    studier.abilities.GainAbility(this.Props.ability);
                }
            }
        }
        //信息超链接
        private IEnumerable<Dialog_InfoCard.Hyperlink> GetRelatedQuestHyperlinks()
        {
            List<Quest> quests = Find.QuestManager.QuestsListForReading;
            int num;
            for (int i = 0; i < quests.Count; i = num + 1)
            {
                bool flag = quests[i].hidden || (quests[i].State != QuestState.Ongoing && quests[i].State > QuestState.NotYetAccepted);
                if (!flag)
                {
                    List<QuestPart> partsListForReading = quests[i].PartsListForReading;
                    for (int j = 0; j < partsListForReading.Count; j = num + 1)
                    {
                        QuestPart_RequirementsToAcceptThingStudied questPart_RequirementsToAcceptThingStudied;
                        bool flag2 = (questPart_RequirementsToAcceptThingStudied = (partsListForReading[j] as QuestPart_RequirementsToAcceptThingStudied)) != null && questPart_RequirementsToAcceptThingStudied.thing == this.parent;
                        if (flag2)
                        {
                            yield return new Dialog_InfoCard.Hyperlink(quests[i], -1);
                            break;
                        }
                        questPart_RequirementsToAcceptThingStudied = null;
                        num = j;
                    }
                    partsListForReading = null;
                }
                num = i;
            }
            yield break;
        }
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn pawn)
        {
            bool completed = this.studyManager.StudyComplete(this.parent.def, pawn);
            //已完成学习
            if (completed)
            {
                yield return new FloatMenuOption("RWrd_CannotStudy".Translate() + " (" + "RWrd_AlreadyStudied".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                yield break;
            }
            //无可到达路径
            bool flag = !pawn.CanReach(this.parent, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn);
            if (flag)
            {
                yield return new FloatMenuOption("RWrd_CannotStudy".Translate() + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                yield break;
            }
            //无力量之源
            bool flag2 = !pawn.IsHavePowerRoot() && !this.Props.Qigong;
            if (flag2)
            {
                yield return new FloatMenuOption("RWrd_CannotStudy".Translate() + " (" + "RWrd_NoRoot".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                yield break;
            }
            //已有对应技能树
            bool flag3 = pawn.IsHavePowerRoot() && this.Props.route != null;
            if (flag3)
            {
                Hediff_RWrd_PowerRoot root = pawn.GetPowerRoot();
                if (root.routes.Contains(this.Props.route))
                {
                    yield return new FloatMenuOption("RWrd_CannotStudy".Translate() + " (" + "RWrd_AlreadyHasRoute".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                    yield break;
                }
            }
            //已有对应技能
            bool flag4 = pawn.IsHavePowerRoot() && this.Props.ability != null;
            if (flag4)
            {
                if (pawn.abilities.GetAbility(this.Props.ability) != null)
                {
                    yield return new FloatMenuOption("RWrd_CannotStudy".Translate() + " (" + "RWrd_AlreadyHasAbility".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                    yield break;
                }
            }
            bool flag5 = !pawn.CanReserve(this.parent, 1, -1, null, false);
            if (flag5)
            {
                yield return new FloatMenuOption("RWrd_CannotStudy".Translate() + " (" + "Reserved".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                yield break;
            }
            bool flag6 = !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
            if (flag6)
            {
                yield return new FloatMenuOption("RWrd_CannotStudy".Translate() + " (" + "Incapable".Translate().CapitalizeFirst() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                yield break;
            }
            Thing researchBench = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(RWrd_DefOf.HiTechResearchBench), PathEndMode.InteractionCell, TraverseParms.For(pawn, Danger.Some, TraverseMode.ByPawn, false, false, false), 9999f, (Thing t) => pawn.CanReserve(t, 1, -1, null, false), null, 0, -1, false, RegionType.Set_Passable, false);
            if (this.Props.studyType == "research")
            {
                bool flag7 = researchBench == null;
                if (flag7)
                {
                    researchBench = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(ThingDefOf.SimpleResearchBench), PathEndMode.InteractionCell, TraverseParms.For(pawn, Danger.Some, TraverseMode.ByPawn, false, false, false), 9999f, (Thing t) => pawn.CanReserve(t, 1, -1, null, false), null, 0, -1, false, RegionType.Set_Passable, false);
                }
                bool flag8 = researchBench == null;
                if (flag8)
                {
                    yield return new FloatMenuOption("RWrd_CannotStudy".Translate() + " (" + "RWrd_NoResearchBench".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                    yield break;
                }
            }
            TaggedString taggedString = "RWrd_Study".Translate(this.parent.Label);
            yield return new FloatMenuOption(taggedString, delegate ()
            {
                bool flag9 = pawn.CanReserveAndReach(this.parent, PathEndMode.Touch, Danger.Deadly, 1, -1, null, false);
                if (flag9)
                {
                    student = pawn;
                    if (this.Props.studyType == "research")
                    {
                        Job job = JobMaker.MakeJob(RWrd_DefOf.RWrd_ResearchDisc, this.parent, researchBench, researchBench.Position);
                        pawn.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), false);
                    }
                    if (this.Props.studyType == "study")
                    {
                        Job job = JobMaker.MakeJob(RWrd_DefOf.RWrd_StudyBook, this.parent);
                        pawn.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), false);
                    }
                }
            }, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
            yield break;
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            bool flag = Scribe.mode != LoadSaveMode.LoadingVars;
            if (!flag)
            {
                float num = 0f;
                Scribe_Values.Look<float>(ref num, "progress", 0f, false);
                bool flag2 = num > 0f;
                if (flag2)
                {
                    float num2 = num / (float)this.Props.cost;
                    float studyProgress = this.studyManager.GetStudyProgress(this.parent.def, student);
                    bool flag3 = num2 > studyProgress;
                    if (flag3)
                    {
                        this.studyManager.ForceSetStudiedProgress(this.parent.def, student, num2);
                    }
                }
            }
        }
        public override string CompInspectStringExtra()
        {
            return null;
        }
        private NeoStudyManager studyManager = new NeoStudyManager();
    }
}
