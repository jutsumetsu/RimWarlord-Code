using Electromagnetic.Core;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;

namespace Electromagnetic.Abilities
{
    public class Hediff_ElectricInternalEnergy : Hediff_TargetBase
    {
        public override void Tick()
        {
            base.Tick();
            //定时生效
            tickCounter++;
            if (tickCounter == 60)
            {
                this.Trigger();
            }
        }
        //触发器
        private void Trigger()
        {
            Map map = this.pawn.Map;
            Pawn pawn = this.pawn;
            Hediff_RWrd_PowerRoot root = this.root;
            //伤害计算
            int masteryOffset = (int)Math.Floor(mastery / 10f);
            float num = 20;
            num = Tools.FinalDamage(root, num, masteryOffset);
            num *= outputPower;
            //生成特效
            FleckCreationData dataStatic1 = FleckMaker.GetDataStatic(this.pawn.DrawPos, map, RWrd_DefOf.RWrd_ElectricClawFleck, 1f);
            map.flecks.CreateFleck(dataStatic1);
            //造成伤害
            pawn.TakeDamage(new DamageInfo(DamageDefOf.Flame, num, 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null, true, true, QualityCategory.Normal, true));
            tickCounter = 0;
            this.Severity = 0;
        }
        private int tickCounter = 0;
    }
}
