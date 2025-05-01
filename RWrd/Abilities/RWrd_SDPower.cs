using Electromagnetic.Core;
using Electromagnetic.UI;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Electromagnetic.Abilities
{
    public class RWrd_SDPower : RWrd_PsyCastToggle
    {
        public RWrd_SDPower(Pawn pawn) : base(pawn)
        {
        }

        public RWrd_SDPower(Pawn pawn, AbilityDef def) : base(pawn, def)
        {
        }
        /// <summary>
        /// 获取Gizmo
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<Command> GetGizmos()
        {
            bool flag = this.gizmo == null;
            if (flag)
            {
                this.gizmo = new Command_SDToggle(this, this.pawn);
            }
            yield return this.gizmo;
            yield break;
        }
    }
}
