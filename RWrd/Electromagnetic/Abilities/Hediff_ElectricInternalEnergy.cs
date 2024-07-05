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
            tickCounter++;
            if (tickCounter == 60)
            {
                this.Trigger();
            }
            if (Find.TickManager.TicksGame % 10 == 0)
            {
                Log.Message(tickCounter.ToString());
            }
        }
        public virtual FleckDef[] EffectSet
        {
            get
            {
                return new FleckDef[]
                {
                    RWrd_DefOf.RWrd_ElectricClawFleck
                };
            }
        }
        private void Trigger()
        {
            Map map = this.pawn.Map;
            Pawn pawn = this.pawn;
            Hediff_RWrd_PowerRoot root = this.root;
            FleckDef[] effectSet = this.EffectSet;
            float num = 30 + root.energy.CurrentDef.level;
            int acr = root.energy.AvailableCompleteRealm();
            int pff = root.energy.PowerFlowFactor();
            float multiplier = acr + pff;
            multiplier = (int)Math.Floor(multiplier / 2);
            num *= multiplier;
            FleckCreationData dataStatic1 = FleckMaker.GetDataStatic(this.pawn.DrawPos, map, effectSet[0], 1f);
            map.flecks.CreateFleck(dataStatic1);
            pawn.TakeDamage(new DamageInfo(DamageDefOf.Flame, num, 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null, true, true, QualityCategory.Normal, true));
            tickCounter = 0;
            this.Severity = 0;
        }
        private int tickCounter = 0;
    }
}
