using Electromagnetic.Core;
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
    public class CompAbilityEffect_WindBlade : CompAbilityEffect
    {
        //绑定Properties
        public new CompProperties_AbilityWindBlade Props
        {
            get
            {
                return (CompProperties_AbilityWindBlade)this.props;
            }
        }
        private Pawn Pawn
        {
            get
            {
                return this.parent.pawn;
            }
        }
        //技能接口
        private RWrd_PsyCastBase Ability
        {
            get
            {
                return (RWrd_PsyCastBase)this.parent;
            }
        }
        //检测可用格子
        private bool canusecell(IntVec3 c)
        {
            ShootLine shootLine;
            return c.InBounds(this.Pawn.Map) && !(c == this.Pawn.Position) && c.InHorDistOf(this.Pawn.Position, this.Props.range) && this.parent.verb.TryFindShootLineFromTo(this.parent.pawn.Position, c, out shootLine, false);
        }
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            IntVec3 position = this.Pawn.Position;
            bool flag = this.Props.SpawnFleck != null;
            bool flag2 = flag;
            if (flag2)
            {
                //生成风刃特效
                FleckCreationData dataStatic = FleckMaker.GetDataStatic(this.Pawn.DrawPos, this.Pawn.Map, this.Props.SpawnFleck, 6f);
                float num = (target.CenterVector3.ToIntVec3() - this.Pawn.Position).AngleFlat;
                dataStatic.rotation = 0f;
                dataStatic.rotation = num;
                dataStatic.velocityAngle = num;
                dataStatic.velocitySpeed = 32f;
                this.Pawn.Map.flecks.CreateFleck(dataStatic);
            }
            DamageDef named = DefDatabase<DamageDef>.GetNamed("Cut", true);
            int masteryOffset = (int)Math.Floor(this.Ability.mastery / 10f);
            int num2 = 30 + masteryOffset;
            if (Pawn.IsHaveRoot())
            {
                //计算伤害
                Hediff_RWrd_PowerRoot root = Pawn.GetRoot();
                num2 += root.energy.CurrentDef.level;
                int acr = root.energy.AvailableCompleteRealm();
                int pff = root.energy.PowerFlowFactor();
                int multiplier = acr + pff;
                multiplier = (int)Math.Floor(multiplier / 2f);
                num2 *= multiplier;
            }
            //友伤豁免
            List<Thing> list = new List<Thing>();
            foreach (Pawn pawn2 in Pawn.MapHeld.mapPawns.AllPawns)
            {
                bool flag3 = pawn2.Faction == Pawn.Faction;
                if (flag3)
                {
                    list.Add(pawn2);
                }
            }
            GenExplosion.DoExplosion(position, this.parent.pawn.MapHeld, this.Props.range, named, this.Pawn, num2, -1f, null, null, null, null, null, 1f, 1, null, false, null, 0f, 1, 0f, false, null, list, null, false, 1.6f, 0f, false, null, 1f, null, this.AffectedCells(target));
            base.Apply(target, dest);
        }
        //影响的格子
        private List<IntVec3> AffectedCells(LocalTargetInfo target)
        {
            this.tmpCells.Clear();
            Vector3 b = this.Pawn.Position.ToVector3Shifted().Yto0();
            IntVec3 intVec = target.Cell.ClampInsideMap(this.Pawn.Map);
            bool flag = this.Pawn.Position == intVec;
            List<IntVec3> result;
            if (flag)
            {
                result = this.tmpCells;
            }
            else
            {
                float lengthHorizontal = (intVec - this.Pawn.Position).LengthHorizontal;
                float num = (float)(intVec.x - this.Pawn.Position.x) / lengthHorizontal;
                float num2 = (float)(intVec.z - this.Pawn.Position.z) / lengthHorizontal;
                intVec.x = Mathf.RoundToInt((float)this.Pawn.Position.x + num * this.Props.range);
                intVec.z = Mathf.RoundToInt((float)this.Pawn.Position.z + num2 * this.Props.range);
                float target2 = Vector3.SignedAngle(intVec.ToVector3Shifted().Yto0() - b, Vector3.right, Vector3.up);
                float num3 = this.Props.lineWidthEnd / 2f;
                float num4 = Mathf.Sqrt(Mathf.Pow((intVec - this.Pawn.Position).LengthHorizontal, 2f) + Mathf.Pow(num3, 2f));
                float num5 = 57.29578f * Mathf.Asin(num3 / num4);
                int num6 = GenRadial.NumCellsInRadius(this.Props.range);
                for (int i = 0; i < num6; i++)
                {
                    IntVec3 intVec2 = this.Pawn.Position + GenRadial.RadialPattern[i];
                    bool flag2 = this.canusecell(intVec2) && Mathf.Abs(Mathf.DeltaAngle(Vector3.SignedAngle(intVec2.ToVector3Shifted().Yto0() - b, Vector3.right, Vector3.up), target2)) <= num5;
                    if (flag2)
                    {
                        this.tmpCells.Add(intVec2);
                    }
                }
                List<IntVec3> list = GenSight.BresenhamCellsBetween(this.Pawn.Position, intVec);
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
            bool flag = this.Pawn.Faction != null;
            bool result;
            if (flag)
            {
                foreach (IntVec3 c in this.AffectedCells(target))
                {
                    List<Thing> thingList = c.GetThingList(this.Pawn.Map);
                    for (int i = 0; i < thingList.Count; i++)
                    {
                        bool flag2 = thingList[i].Faction == this.Pawn.Faction;
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
