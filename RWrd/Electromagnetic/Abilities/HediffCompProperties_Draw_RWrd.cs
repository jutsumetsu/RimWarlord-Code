using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Electromagnetic.Abilities
{
    public class HediffCompProperties_Draw_RWrd : HediffCompProperties
    {
        public override void PostLoad()
        {
            base.PostLoad();
            ShieldsSystem_RWrd.ApplyDrawPatches();
        }

        public GraphicData graphic;
    }
}
