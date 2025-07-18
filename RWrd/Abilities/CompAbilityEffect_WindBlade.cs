﻿using Electromagnetic.Core;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Electromagnetic.Abilities
{
    public class CompAbilityEffect_WindBlade : CompAbilityEffect_Electromagnetic
    {
        //绑定Properties
        public new CompProperties_AbilityWindBlade Props
        {
            get
            {
                return (CompProperties_AbilityWindBlade)this.props;
            }
        }
        //检测可用格子
        private bool canusecell(IntVec3 c)
        {
            ShootLine shootLine;
            return c.InBounds(this.Caster.Map) && !(c == this.Caster.Position) && c.InHorDistOf(this.Caster.Position, this.Props.range) && this.parent.verb.TryFindShootLineFromTo(this.parent.pawn.Position, c, out shootLine, false);
        }
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            IntVec3 position = this.Caster.Position;
            bool flag = this.Props.SpawnFleck != null;
            bool flag2 = flag;
            if (flag2)
            {
                //生成风刃特效
                FleckCreationData dataStatic = FleckMaker.GetDataStatic(this.Caster.DrawPos, this.Caster.Map, this.Props.SpawnFleck, 6f);
                float num = (target.CenterVector3.ToIntVec3() - this.Caster.Position).AngleFlat;
                dataStatic.rotation = 0f;
                dataStatic.rotation = num;
                dataStatic.velocityAngle = num;
                dataStatic.velocitySpeed = 32f;
                this.Caster.Map.flecks.CreateFleck(dataStatic);
            }
            DamageDef named = DefDatabase<DamageDef>.GetNamed("Cut", true);
            int masteryOffset = (int)Math.Floor(this.Ability.mastery / 10f);
            int num2 = 30;
            if (Caster.IsHavePowerRoot())
            {
                //计算伤害
                Hediff_RWrd_PowerRoot root = Caster.GetPowerRoot();
                num2 = (int)Tools.FinalDamage(root, num2, masteryOffset);
                num2 = (int)Math.Floor(num2 * Ability.outputPower);
            }
            //友伤豁免
            List<Thing> list = new List<Thing>();
            foreach (Pawn pawn2 in Caster.MapHeld.mapPawns.AllPawns)
            {
                bool flag3 = pawn2.Faction == Caster.Faction;
                if (flag3)
                {
                    list.Add(pawn2);
                }
            }
            GenExplosion.DoExplosion(position, this.parent.pawn.MapHeld, this.Props.range, named, this.Caster, num2, -1f, null, null, null, null, null, 1f, 1, null, null, 0, false, null, 0, 1, 0, false, null, list, null, false, 1.6f, 0f, false, null, 1f, null, this.AffectedCells(target));
            base.Apply(target, dest);
        }
        //影响的格子
        private List<IntVec3> AffectedCells(LocalTargetInfo target)
        {
            this.tmpCells.Clear();
            Vector3 b = this.Caster.Position.ToVector3Shifted().Yto0();
            IntVec3 intVec = target.Cell.ClampInsideMap(this.Caster.Map);
            bool flag = this.Caster.Position == intVec;
            List<IntVec3> result;
            if (flag)
            {
                result = this.tmpCells;
            }
            else
            {
                float lengthHorizontal = (intVec - this.Caster.Position).LengthHorizontal;
                float num = (float)(intVec.x - this.Caster.Position.x) / lengthHorizontal;
                float num2 = (float)(intVec.z - this.Caster.Position.z) / lengthHorizontal;
                intVec.x = Mathf.RoundToInt((float)this.Caster.Position.x + num * this.Props.range);
                intVec.z = Mathf.RoundToInt((float)this.Caster.Position.z + num2 * this.Props.range);
                float target2 = Vector3.SignedAngle(intVec.ToVector3Shifted().Yto0() - b, Vector3.right, Vector3.up);
                float num3 = this.Props.lineWidthEnd / 2f;
                float num4 = Mathf.Sqrt(Mathf.Pow((intVec - this.Caster.Position).LengthHorizontal, 2f) + Mathf.Pow(num3, 2f));
                float num5 = 57.29578f * Mathf.Asin(num3 / num4);
                int num6 = GenRadial.NumCellsInRadius(this.Props.range);
                for (int i = 0; i < num6; i++)
                {
                    IntVec3 intVec2 = this.Caster.Position + GenRadial.RadialPattern[i];
                    bool flag2 = this.canusecell(intVec2) && Mathf.Abs(Mathf.DeltaAngle(Vector3.SignedAngle(intVec2.ToVector3Shifted().Yto0() - b, Vector3.right, Vector3.up), target2)) <= num5;
                    if (flag2)
                    {
                        this.tmpCells.Add(intVec2);
                    }
                }
                List<IntVec3> list = GenSight.BresenhamCellsBetween(this.Caster.Position, intVec);
                for (int j = 0; j < list.Count; j++)
                {
                    IntVec3 intVec3 = list[j];
                    bool flag3 = !this.tmpCells.Contains(intVec3) && this.canusecell(intVec3);
                    if (flag3)
                    {
                        this.tmpCells.Add(intVec3);
                    }
                }
                result = this.tmpCells;
            }
            return result;
        }
        //绘制影响区域预览
        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            GenDraw.DrawFieldEdges(this.AffectedCells(target));
        }
        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            bool flag = this.Caster.Faction != null;
            bool result;
            if (flag)
            {
                foreach (IntVec3 c in this.AffectedCells(target))
                {
                    List<Thing> thingList = c.GetThingList(this.Caster.Map);
                    for (int i = 0; i < thingList.Count; i++)
                    {
                        bool flag2 = thingList[i].Faction == this.Caster.Faction;
                        if (flag2)
                        {
                            return false;
                        }
                    }
                }
                result = true;
            }
            else
            {
                result = true;
            }
            return result;
        }
        private List<IntVec3> tmpCells = new List<IntVec3>();
        public static EffecterDef pulse;
    }
}
