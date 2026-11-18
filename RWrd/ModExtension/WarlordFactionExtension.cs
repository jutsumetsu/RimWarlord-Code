using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Electromagnetic.ModExtension
{
    public class WarlordFactionExtension : DefModExtension
    {
        public bool canReceiveTribute;
        public bool canIssueBounty;
        public float bountyWeight = 1f;
    }
}
