using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Electromagnetic.Quests
{
    public class QuestPart_WarlordBounty : QuestPartActivable
    {
        public Pawn target;
        public Faction issuer;
        public Faction hostFaction;
        public string successSignal;
        public bool succeeded;

        public override IEnumerable<GlobalTargetInfo> QuestLookTargets
        {
            get
            {
                if (target != null)
                    yield return target;
            }
        }

        protected override void Enable(SignalArgs receivedArgs)
        {
            base.Enable(receivedArgs);
            if (target == null || issuer == null)
                return;

            Find.LetterStack.ReceiveLetter(
                "WarlordBountyLabel".Translate(issuer.Named("FACTION")),
                "WarlordBountyText".Translate(target.Named("PAWN"), issuer.Named("FACTION")),
                LetterDefOf.ThreatSmall);
        }

        public override void Notify_PawnKilled(Pawn pawn, DamageInfo? dinfo)
        {
            if (State != QuestPartState.Enabled || succeeded || pawn != target || issuer == null)
                return;

            succeeded = true;
            Find.SignalManager.SendSignal(new Signal(successSignal));
            Complete();
            // 结束由 Root 挂的 quest.End(..., bountySuccessSignal) 处理；
            // 这里不能调用 QuestGen 的 End 扩展，运行时没有 QuestGen 的生成期 slate。
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref target, "target");
            Scribe_References.Look(ref issuer, "issuer");
            Scribe_References.Look(ref hostFaction, "hostFaction");
            Scribe_Values.Look(ref successSignal, "successSignal");
            Scribe_Values.Look(ref succeeded, "succeeded", false);
        }
    }
}
