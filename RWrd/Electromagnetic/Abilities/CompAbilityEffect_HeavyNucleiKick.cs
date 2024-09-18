using Electromagnetic.Core;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Electromagnetic.Abilities
{
    public class CompAbilityEffect_HeavyNucleiKick : CompAbilityEffect
    {
        //绑定Properties
        public new CompProperties_AbilityHeavyNucleiKick Props
        {
            get
            {
                return (CompProperties_AbilityHeavyNucleiKick)this.props;
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
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Map map = this.parent.pawn.Map;
            //大跳
            PawnFlyer pawnFlyer = PawnFlyer.MakeFlyer(ThingDefOf.PawnFlyer, this.parent.pawn, target.Cell, null, null);
            GenSpawn.Spawn(pawnFlyer, this.parent.pawn.Position, map, WipeMode.Vanish);
            base.Apply(target, dest);
            //击晕目标
            Pawn pawn = (Pawn)((Thing)target);
            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, false);
            pawn.stances.stunner.StunFor(60, this.parent.pawn, false, false);
            //传入参数
            caster = this.parent.pawn;
            enemy = pawn;
            spawn = false;
        }
        public override void CompTick()
        {
            base.CompTick();
            //在pawn完成跳跃后对目标施加效果
            if (spawn == false)
            {
                if (caster.Spawned)
                {
                    spawn = true;
                    Pawn pawn = enemy;
                    //伤害计算
                    Hediff_RWrd_PowerRoot root = caster.GetPowerRoot();
                    int masteryOffset = (int)Math.Floor(this.Ability.mastery / 10f);
                    int num = 40;
                    num = (int)Tools.FinalDamage(root, num, masteryOffset);
                    num = (int)Math.Floor(num * Ability.outputPower);
                    //关闭友伤
                    List<Thing> list = new List<Thing>();
                    foreach (Pawn pawn2 in pawn.MapHeld.mapPawns.AllPawns)
                    {
                        bool flag = pawn2.Faction == caster.Faction;
                        if (flag)
                        {
                            list.Add(pawn2);
                        }
                    }
                    //造成爆炸效果
                    GenExplosion.DoExplosion(pawn.PositionHeld, pawn.MapHeld, 1f, DamageDefOf.Bomb, caster, num, 0, null, null, null, null, null, 0, 1, null, false, null, 0, 1, 0, false, null, list);
                    //重新选中小人
                    CameraJumper.TrySelect(caster);
                    caster = null;
                    enemy = null;
                }
            }
        }
        private Pawn caster;
        private Pawn enemy;
        private bool spawn = true;
    }
}
