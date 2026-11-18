using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electromagnetic.GameComponents
{
    public class GameComponent_RWrdVassal : Verse.GameComponent
    {
        public RimWorld.Faction overlord;
        public bool abandonedStaticOverlord;
        public GameComponent_RWrdVassal(Game game)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref overlord, "rwrdOverlord");
            Scribe_Values.Look(ref abandonedStaticOverlord, "rwrdAbandonedStaticOverlord", false);
        }
    }
}
