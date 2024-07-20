using Electromagnetic.Core;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Electromagnetic.Things
{
    public class CompProperties_ThingStudiable : CompProperties
    {
        public CompProperties_ThingStudiable()
        {
            this.compClass = typeof(CompThingStudiable);
        }
        public List<AbilityDef> abilities;
        public AbilityDef ability;
        public RWrd_RouteDef route;
        public int cost = 10;
        public string studyType;
    }
}
