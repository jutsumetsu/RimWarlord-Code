using Electromagnetic.Core;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Electromagnetic.Quests
{
    public class QuestPart_BaakOfferSettlement : QuestPart
    {
        public Settlement settlement;
        public string offerSignal;
        public string mapRemovedSignal;
        public bool defeated;
        public bool converted;
        public PlanetTile tile;
        public string settlementName;

        public QuestPart_BaakOfferSettlement() { }

        public QuestPart_BaakOfferSettlement(Settlement settlement, string offerSignal, string mapRemovedSignal)
        {
            this.settlement = settlement;
            this.offerSignal = offerSignal;
            this.mapRemovedSignal = mapRemovedSignal;
            this.tile = settlement.Tile;
            this.settlementName = settlement.Label;
        }

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            if (signal.tag != mapRemovedSignal || !defeated || converted)
                return;
            ConvertToBaak();
        }

        private void ConvertToBaak()
        {
            RimWorld.Faction baak = Find.FactionManager.FirstFactionOfDef(RWrd_DefOf.RWrd_Faction_Empire_Baak);
            if (baak == null || baak.defeated)
                return;

            if (settlement != null && !settlement.Destroyed)
            {
                tile = settlement.Tile;
                settlementName = settlement.Label;
                Find.WorldObjects.Remove(settlement);
                settlement.Destroy();
            }

            foreach (DestroyedSettlement destroyed in Find.WorldObjects.ObjectsAt(tile)
                         .OfType<DestroyedSettlement>().ToList())
            {
                Find.WorldObjects.Remove(destroyed);
            }

            Settlement newSettlement = WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement) as Settlement;
            if (newSettlement == null)
                return;

            newSettlement.Tile = tile;
            newSettlement.Name = settlementName;
            newSettlement.SetFaction(baak);
            Find.WorldObjects.Add(newSettlement);

            converted = true;
            Find.SignalManager.SendSignal(new Signal(offerSignal));
        }

        public override IEnumerable<GlobalTargetInfo> QuestLookTargets
        {
            get { yield return settlement; }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref settlement, "settlement");
            Scribe_Values.Look(ref defeated, "defeated");
            Scribe_Values.Look(ref converted, "converted");
            Scribe_Values.Look(ref tile, "tile");
            Scribe_Values.Look(ref settlementName, "settlementName");
        }
    }
}
