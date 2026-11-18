using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI.Group;
using Verse;

namespace Electromagnetic.Quests
{
    public class QuestPart_BaakSpawnTarget : QuestPart
    {
        public Pawn target;
        public Site site;
        public string inSignal;

        public QuestPart_BaakSpawnTarget() { }

        public QuestPart_BaakSpawnTarget(Pawn target, Site site, string inSignal)
        {
            this.target = target;
            this.site = site;
            this.inSignal = inSignal;
        }

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            if (signal.tag != inSignal || target == null || site == null || site.Map == null) return;
            Map map = site.Map;
            IntVec3 cell = CellFinder.RandomClosewalkCellNear(map.Center, map, 10);
            GenSpawn.Spawn(target, cell, map);
            Lord lord = map.lordManager.lords.FirstOrDefault(l => l.faction == target.Faction);
            if (lord != null) lord.AddPawn(target);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref target, "target");
            Scribe_References.Look(ref site, "site");
            Scribe_Values.Look(ref inSignal, "inSignal");
        }
    }
}
