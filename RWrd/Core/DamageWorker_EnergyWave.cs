using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Electromagnetic.Core
{
    public class DamageWorker_EnergyWave : DamageWorker_AddInjury
    {
        public override void ExplosionStart(Explosion explosion, List<IntVec3> cellsToAffect)
        {
            if (this.def.explosionHeatEnergyPerCell > 1E-45f)
            {
                GenTemperature.PushHeat(explosion.Position, explosion.Map, this.def.explosionHeatEnergyPerCell * (float)cellsToAffect.Count);
            }
            if (explosion.doVisualEffects)
            {
                if (explosion.Map == Find.CurrentMap)
                {
                    float magnitude = (explosion.Position.ToVector3Shifted() - Find.Camera.transform.position).magnitude;
                    Find.CameraDriver.shaker.DoShake(4f * explosion.radius * explosion.screenShakeFactor / magnitude);
                }
                this.ExplosionVisualEffectCenterOverride(explosion);
            }
        }
        protected virtual void ExplosionVisualEffectCenterOverride(Explosion explosion)
        {
            if (this.def.explosionCenterFleck != null)
            {
                FleckMaker.Static(explosion.Position.ToVector3Shifted(), explosion.Map, this.def.explosionCenterFleck, 1f);
            }
            else if (this.def.explosionCenterMote != null)
            {
                MoteMaker.MakeStaticMote(explosion.Position.ToVector3Shifted(), explosion.Map, this.def.explosionCenterMote, 1f, false, 0f);
            }
            if (this.def.explosionCenterEffecter != null)
            {
                this.def.explosionCenterEffecter.Spawn(explosion.Position, explosion.Map, Vector3.zero, 1f);
            }
            if (this.def.explosionInteriorMote != null || this.def.explosionInteriorFleck != null || this.def.explosionInteriorEffecter != null)
            {
                int num = Mathf.RoundToInt(3.1415927f * explosion.radius * explosion.radius / 6f * this.def.explosionInteriorCellCountMultiplier);
                for (int j = 0; j < num; j++)
                {
                    Vector3 b = Gen.RandomHorizontalVector(explosion.radius * this.def.explosionInteriorCellDistanceMultiplier);
                    if (this.def.explosionInteriorEffecter != null)
                    {
                        Vector3 vect = explosion.Position.ToVector3Shifted() + b;
                        this.def.explosionInteriorEffecter.Spawn(explosion.Position, vect.ToIntVec3(), explosion.Map, 1f);
                    }
                    else if (this.def.explosionInteriorFleck != null)
                    {
                        FleckMaker.ThrowExplosionInterior(explosion.Position.ToVector3Shifted() + b, explosion.Map, this.def.explosionInteriorFleck);
                    }
                    else
                    {
                        MoteMaker.ThrowExplosionInteriorMote(explosion.Position.ToVector3Shifted() + b, explosion.Map, this.def.explosionInteriorMote);
                    }
                }
            }
        }
    }
}
