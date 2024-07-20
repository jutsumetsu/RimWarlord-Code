using System;
using System.Collections.Generic;
using System.Linq;
using Electromagnetic.Abilities;
using RimWorld;
using Verse;
using Verse.AI;
using HarmonyLib;

namespace Electromagnetic.Core
{
    public class Hediff_RWrd_PowerRoot : HediffWithComps
    {
        //显示标签
        public override string LabelInBrackets
        {
            get
            {
                return string.Concat(new string[]
                {
                    this.energy.currentRWrd.def.label,
                    ",",
                    Math.Round((double)this.energy.Energy, 2).ToString(),
                    ",",
                    Math.Round((double)this.energy.Exp, 2).ToString(),
                    ",",
                    Math.Round((double)this.energy.CompleteRealm, 2).ToString(),
                    ",",
                    Math.Round((double)this.energy.PowerFlow, 2).ToString()
                });
            }
        }
        //获取UI
        public override IEnumerable<Gizmo> GetGizmos()
        {
            bool flag = this.energy != null;
            if (flag)
            {
                bool flag1 = !this.reeStartInit;
                bool flag2 = this.energy.currentRWrd.def.level == 0;
                if (flag1)
                {
                    this.pawn.CheckAbilityLimiting();
                    this.reeStartInit = true;
                }
                Gizmo_Psychic gizmo = new Gizmo_Psychic(this.pawn, this);
                gizmo.EnergyLabel = "RWrd_EP_Energy".Translate();
                gizmo.CompleteRealmLabel = "Rwrd_CompleteRealm".Translate();
                gizmo.PowerFlowLabel = "RWrd_PowerFlow".Translate();
                if (flag2)
                {
                    gizmo.ExpLabel = "RWrd_Volt".Translate();
                }
                else
                {
                    gizmo.ExpLabel = "RWrd_HorsePower".Translate();
                }
                yield return gizmo;
                gizmo = null;
            }
            bool godMode = DebugSettings.godMode;
            if (godMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "RWrd_LevelUP".Translate(),
                    action = delegate ()
                    {
                        this.energy.SetExp(this.energy.CurrentDef.EXP);
                        this.energy.SetLevel();
                        this.pawn.CheckAbilityLimiting();
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "RWrd_IncreaseEXP".Translate(),
                    action = delegate ()
                    {
                        this.energy.SetExp(1000f);
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "RWrd_IncreaseEnergy".Translate(),
                    action = delegate ()
                    {
                        this.energy.SetEnergy(1000f);
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "RWrd_IncreaseCompleteRealm".Translate(),
                    action = delegate ()
                    {
                        this.energy.SetCompleteRealm(0.1f);
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "RWrd_IncreasePowerFlow".Translate(),
                    action = delegate ()
                    {
                        this.energy.SetPowerFlow(10000);
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "RWrd_ReloadDefaultSkillTree".Translate(),
                    action = delegate ()
                    {
                        this.RemoveRWrdAbilities();
                        this.UnlockRoute(RWrd_DefOf.Base);
                        this.pawn.CheckAbilityLimiting();
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "RWrd_ReloadBaakFamilySkillTree".Translate(),
                    action = delegate ()
                    {
                        this.RemoveRWrdAbilities();
                        this.UnlockRoute(RWrd_DefOf.SixSecret);
                        this.pawn.CheckAbilityLimiting();
                    }
                };
            }
            yield break;
        }
        public override void PostRemoved()
        {
            this.pawn.CheckAbilityLimiting();
            this.RemoveRWrdAbilities();
            base.PostRemoved();
        }
        //移除武神技能
        private void RemoveRWrdAbilities()
        {
            List<Ability> tmp = new List<Ability>();
            this.pawn.abilities.abilities.ForEach(delegate (Ability a)
            {
                bool flag = a.def.abilityClass == typeof(RWrd_PsyCastBase);
                if (flag)
                {
                    tmp.Add(a);
                }
            });
            tmp.ForEach(delegate (Ability a)
            {
                this.pawn.abilities.RemoveAbility(a.def);
            });
        }
        public void UnlockRoute(RWrd_RouteDef route)
        {
            if (!this.routes.Contains(route))
            {
                this.routes.Add(route);
            }
            else
            {
                Messages.Message("RWrd_UnlockError".Translate(this.pawn.Name.ToStringShort, route.label), this.pawn, MessageTypeDefOf.PositiveEvent, true);
            }
        }
        //身体机能加成列表
        private IEnumerable<PawnCapacityModifier> GetPCMList()
        {
            int lf = this.energy.CurrentDef.level + 1;
            int lf2 = lf + 1;
            float cr = this.energy.completerealm;
            yield return new PawnCapacityModifier
            {
                capacity = PawnCapacityDefOf.Consciousness,
                offset = Math.Min(lf, 50),
            };
            yield return new PawnCapacityModifier
            {
                capacity = PawnCapacityDefOf.Moving,
                offset = Math.Min(lf, 50),
            };
            yield return new PawnCapacityModifier
            {
                capacity = PawnCapacityDefOf.Sight,
                offset = Math.Min(lf2, 50),
            };
            yield return new PawnCapacityModifier
            {
                capacity = PawnCapacityDefOf.Hearing,
                offset = Math.Min(lf2, 50),
            };
            yield return new PawnCapacityModifier
            {
                capacity = PawnCapacityDefOf.BloodFiltration,
                offset = Math.Min(lf2, 50),
            };
            yield return new PawnCapacityModifier
            {
                capacity = PawnCapacityDefOf.BloodPumping,
                offset = Math.Min(lf2, 50),
            };
            yield return new PawnCapacityModifier
            {
                capacity = PawnCapacityDefOf.Breathing,
                offset = Math.Min(lf2, 50),
            };
            yield return new PawnCapacityModifier
            {
                capacity = PawnCapacityDefOf.Manipulation,
                offset = cr,
            };
        }
        public override void PostMake()
        {
            base.PostMake();
            //解锁默认技能树
            this.UnlockRoute(RWrd_DefOf.Base);
            //赋予EnergyTracker
            bool flag = this.energy == null;
            if (flag)
            {
                this.energy = new Pawn_EnergyTracker(this.pawn);
                this.IsInit = true;
                this.initActivity = true;
            }
            bool flag3 = this.pawn.Faction != Faction.OfPlayer;
            if (flag3)
            {
                //NPC生成
                RimWarlordDef rimWarlordDef = null;
                RWrd_RouteDef rwrd_RouteDef = null;
                PowerRootUtillity.RandomPowerRootSpawn(this.pawn, this, out rimWarlordDef, out rwrd_RouteDef);
                this.energy.powerflow = UnityEngine.Random.Range(3, 51) * 10000;
                this.energy.MaxEnergy = this.energy.powerflow / 100;
                float prenum = this.pawn.ageTracker.AgeBiologicalTicks / 3600000f;
                //年龄限制完全境界
                int age = (int)Math.Floor(prenum);
                if (age < 30)
                {
                    this.energy.completerealm = UnityEngine.Random.Range(1, 4) * 0.1f;
                }
                else if (age < 40)
                {
                    this.energy.completerealm = UnityEngine.Random.Range(1, 6) * 0.1f;
                }
                else if (age < 50)
                {
                    this.energy.completerealm = UnityEngine.Random.Range(1, 11) * 0.1f;
                }
                else
                {
                    this.energy.completerealm = UnityEngine.Random.Range(1, 11) * 0.1f;
                }
                while (this.energy.currentRWrd.def.level < rimWarlordDef.level)
                {
                    this.energy.exp = this.energy.currentRWrd.def.EXP;
                    this.energy.energy = this.energy.MaxEnergy;
                    this.energy.SetLevel();
                    this.pawn.CheckAbilityLimiting();
                }
            }
            else
            {
                //殖民者生成
                this.energy.powerflow = UnityEngine.Random.Range(2, 11) * 50000;
                this.energy.MaxEnergy = this.energy.powerflow / 100;
                this.energy.completerealm = UnityEngine.Random.Range(1, 4) * 0.1f;
                this.energy.trainDesireFactor = UnityEngine.Random.Range(1, 51);
            }
            this.pawn.CheckAbilityLimiting();
            /*foreach (PawnCapacityModifier pawnCapacityModifier in this.GetPCMList())
            {
                this.CurStage.capMods.Add(pawnCapacityModifier);
            }*/
        }
        //保存数据
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look<Pawn_EnergyTracker>(ref this.energy, "energy", Array.Empty<object>());
            Scribe_Values.Look<bool>(ref this.IsInit, "IsInit", false, false);
            Scribe_Values.Look<bool>(ref this.openingBasicAbility, "openingBasicAbility", false, false);
            Scribe_Collections.Look<RWrd_RouteDef>(ref this.routes, "routes", LookMode.Def, Array.Empty<object>());
            bool flag = Scribe.mode == LoadSaveMode.PostLoadInit;
            if (flag)
            {
                if (this.routes == null)
                {
                    this.routes = new List<RWrd_RouteDef>();
                }
                this.energy.pawn = this.pawn;
            }
        }
        public override void Tick()
        {
            base.Tick();
            JobDriver jobDriver = this.pawn.jobs.curDriver;
            //最大能量赋值
            this.energy.MaxEnergy = this.energy.PowerFlow / 100;
            if (Find.TickManager.TicksGame % 60 == 0)
            {
                foreach (PawnCapacityModifier pcm in this.CurStage.capMods)
                {
                    foreach (PawnCapacityModifier pawnCapacityModifier in this.GetPCMList())
                    {
                        if (pcm.capacity == pawnCapacityModifier.capacity)
                        {
                            //更新机能加成
                            pcm.offset = pawnCapacityModifier.offset;
                        }
                    }
                }
            }
            if (this.energy.IsUpdateLevelTiming())
            {
                this.energy.SetLevel();
            }
            if (this.CurStage.capMods.Count == 0)
            {
                foreach (PawnCapacityModifier pawnCapacityModifier in this.GetPCMList())
                {
                    //重新添加机能加成
                    this.CurStage.capMods.Add(pawnCapacityModifier);
                }
            }
            if (Find.TickManager.TicksGame % 360000 == 0)
            {
                //随时间增加力量流量
                this.energy.SetPowerFlow(100);
            }
            if (Find.TickManager.TicksGame % 180 == 0)
            {
                if (this.pawn.Faction != Faction.OfPlayer)
                {
                    //NPC能量回复
                    this.energy.SetEnergy(100);
                }
                else
                {
                    if (this.pawn.Drafted)
                    {
                        //战斗状态下能量回复
                        this.energy.SetEnergy(100);
                    }
                    else
                    {
                        //脱战状态下能量回复
                        this.energy.SetEnergy(300);
                    }
                }
            }
            if (jobDriver != null)
            {
                if (jobDriver.GetType() == typeof(JobDriver_AttackMelee))
                {
                    int numMeleeAttacksMade = Traverse.Create(jobDriver).Field("numMeleeAttacksMade").GetValue<int>();
                    if (numMeleeAttacksMade > this.meleeAttackCounter)
                    {
                        //攻击次数计数器
                        meleeAttackCounter++;
                    }
                }
                else
                {
                    float num = this.energy.damage;
                    float num2 = num * meleeAttackCounter / 10;
                    int exp1 = (int)Math.Floor(num2);
                    int exp2 = exp1 * 10;
                    int currentLevel = this.energy.currentRWrd.def.level;
                    if (currentLevel == 0)
                    {
                        //电推经验获取
                        this.energy.SetExp(exp2);
                    }
                    else
                    {
                        //磁场转动经验获取
                        this.energy.SetExp(exp1);
                    }
                    this.energy.damage = 0;
                    meleeAttackCounter = 0;
                }
            }
        }
        public int meleeAttackCounter = 0;

        public Pawn_EnergyTracker energy;

        private bool IsInit = false;
        public bool initActivity = false;

        public List<RWrd_RouteDef> routes = new List<RWrd_RouteDef>();

        public bool openingBasicAbility = false;

        public bool reeStartInit = false;
    }
}