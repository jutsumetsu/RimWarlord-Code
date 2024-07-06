using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Electromagnetic.Abilities
{
    public class HediffComp_Draw_RWrd : HediffComp
    {
        public virtual void DrawAt(Vector3 drawPos)
        {
            Graphic graphic = this.Graphic;
            bool flag = graphic != null;
            if (flag)
            {
                graphic.Draw(drawPos, base.Pawn.Rotation, base.Pawn, 0f);
            }
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            List<HediffComp_Draw_RWrd> list;
            bool flag = ShieldsSystem_RWrd.HediffDrawsByPawn.TryGetValue(base.Pawn, out list);
            if (flag)
            {
                list.Add(this);
            }
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            List<HediffComp_Draw_RWrd> list;
            bool flag = ShieldsSystem_RWrd.HediffDrawsByPawn.TryGetValue(base.Pawn, out list);
            if (flag)
            {
                list.Remove(this);
            }
        }


        public virtual Graphic Graphic
        {
            get
            {
                HediffCompProperties_Draw_RWrd hediffCompProperties_Draw = this.props as HediffCompProperties_Draw_RWrd;
                Graphic result;
                if (hediffCompProperties_Draw == null)
                {
                    result = null;
                }
                else
                {
                    GraphicData graphic = hediffCompProperties_Draw.graphic;
                    result = ((graphic != null) ? graphic.Graphic : null);
                }
                return result;
            }
        }
    }
}
