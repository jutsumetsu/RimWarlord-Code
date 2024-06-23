using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Electromagnetic.Core
{
    public class RimWarlord : IExposable
    {
        public void ExposeData()
        {
            Scribe_Defs.Look<RimWarlordDef>(ref this.def, "def");
        }

        public RimWarlord()
        {
            this.def = this.def;
            this.pawn = this.pawn;
        }

        public RimWarlord(RimWarlordDef def, Pawn pawn)
        {
            this.def = def;
            this.pawn = pawn;
        }

        public RimWarlord(RimWarlord other)
        {
            this.def = other.def;
            this.pawn = other.pawn;
        }

        public RimWarlordDef def;
        public Pawn pawn;
    }
}
