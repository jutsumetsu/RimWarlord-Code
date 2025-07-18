﻿using Electromagnetic.Core;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Electromagnetic.Abilities
{
    public class Hediff_ExplosiveEnergy : Hediff_TargetBase
    {
        public override void Tick()
        {
            base.Tick();
            //定时起爆
            tickCounter++;
            if (tickCounter == 20)
            {
                this.Trigger();
            }
        }
        //起爆器
        private void Trigger()
        {
            Pawn pawn = this.pawn;
            //伤害计算
            Hediff_RWrd_PowerRoot root = this.root;
            int num = (int)Tools.FinalDamage(root, damage);
            num = (int)Math.Floor(num * outputPower);
            //友伤豁免
            List<Thing> list = new List<Thing>();
            foreach (Pawn pawn2 in pawn.MapHeld.mapPawns.AllPawns)
            {
                bool flag = pawn2.Faction == root.pawn.Faction;
                if (flag)
                {
                    list.Add(pawn2);
                }
            }
            //产生爆炸
            GenExplosion.DoExplosion(pawn.PositionHeld, pawn.MapHeld, 2f, DamageDefOf.Bomb, this.root.pawn, num, 0, null, null, null, null, null, 0, 1, null, null, 0, false, null, 0, 1, 0, false, null, list);
            tickCounter = 0;
            this.Severity -= 0.1f;
        }
        private int tickCounter = 0;
        public int damage = 10;
    }
}
