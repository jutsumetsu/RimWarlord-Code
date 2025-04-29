using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Electromagnetic.Abilities
{
    public class AbilitySet : IExposable, IRenameable
    {
        public void ExposeData()
        {
            Scribe_Values.Look<string>(ref this.Name, "name", null, false);
            Scribe_Collections.Look<AbilityDef>(ref this.Abilities, "abilities", LookMode.Undefined);
        }
        public string RenamableLabel
        {
            get
            {
                return this.Name;
            }
            set
            {
                this.Name = value;
            }
        }
        public string BaseLabel
        {
            get
            {
                return this.Name;
            }
        }
        public string InspectLabel
        {
            get
            {
                return this.Name;
            }
        }
        public string Name;
        public HashSet<AbilityDef> Abilities = new HashSet<AbilityDef>();
    }
}
