using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Electromagnetic.UI
{
    public class Command_ActionWithFloat : Command_Action
    {
        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
        {
            get
            {
                Func<IEnumerable<FloatMenuOption>> func = this.floatMenuGetter;
                return (func != null) ? func() : null;
            }
        }
        public Func<IEnumerable<FloatMenuOption>> floatMenuGetter;
    }
}
