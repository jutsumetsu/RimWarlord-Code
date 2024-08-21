using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Electromagnetic.Abilities
{
    public static class AntiGravityUtils
    {
        public static bool IsAntiGravity(this Pawn p)
        {
            return AntiGravityPawns.Contains(p);
        }

        public static bool IsAntiGravitySlow(this Pawn p)
        {
            return p.health.hediffSet.GetAllComps().OfType<HediffComp_AntiGravity>().Any();
        }

        public static HashSet<Pawn> AntiGravityPawns = new HashSet<Pawn>();
    }
}
