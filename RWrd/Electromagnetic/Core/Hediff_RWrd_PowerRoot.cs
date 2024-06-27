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

        public override IEnumerable<Gizmo> GetGizmos()
        {
            bool flag = this.energy != null;
            if (flag)
            {
                bool flag1 = !this.reeStartInit;
                bool flag2 = this.energy.currentRWrd.def.level == 0;
                if (flag1)
                {
                    this.pawn.CheckLevelAndLimitingAbility();
                    this.reeStartInit = true;
                }
                Gizmo_Psychic gizmo = new Gizmo_Psychic(this.pawn, this);
                gizmo.EnergyLabel = "能量值";
                gizmo.CompleteRealmLabel = "完全境界";
                gizmo.PowerFlowLabel = "力量流量";
                if (flag2)
                {
                    gizmo.ExpLabel = "伏特";
                }
                else
                {
                    gizmo.ExpLabel = "匹";
                }
                yield return gizmo;
                gizmo = null;
            }
            bool godMode = DebugSettings.godMode;
            if (godMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "测试：直接升级",
                    action = delegate ()
                    {
                        this.energy.SetExp(this.energy.CurrentDef.EXP);
                        this.energy.SetLevel();
                        this.pawn.CheckLevelAndLimitingAbility();
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "测试：提升力量",
                    action = delegate ()
                    {
                        this.energy.SetExp(1000f);
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "测试：加能量",
                    action = delegate ()
                    {
                        this.energy.SetEnergy(1000f);
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "测试：提升完全境界",
                    action = delegate ()
                    {
                        this.energy.SetCompleteRealm(0.1f);
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "测试：提升力量流量",
                    action = delegate ()
                    {
                        this.energy.SetPowerFlow(10000);
                    }
                };
            }
            yield break;
        }
        public override void PostRemoved()
        {
            this.pawn.CheckLevelAndLimitingAbility();
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
            RWrd_DefOf.Base.AllAbilities.ForEach(delegate (AbilityDef a)
            {
                this.pawn.abilities.GainAbility(a);
            });
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
                RimWarlordDef rimWarlordDef = null;
                RWrd_RouteDef rwrd_RouteDef = null;
                PowerRootUtillity.RandomPowerRootSpawn(this.pawn, this, out rimWarlordDef, out rwrd_RouteDef);
                this.energy.powerflow = UnityEngine.Random.Range(3, 51) * 10000;
                this.energy.currentRWrd.def.MaxEnergy = this.energy.powerflow / 100;
                this.energy.completerealm = UnityEngine.Random.Range(1, 11) * 0.1f;
                while (this.energy.currentRWrd.def.level < rimWarlordDef.level)
                {
                    this.energy.exp = this.energy.currentRWrd.def.EXP;
                    this.energy.energy = this.energy.currentRWrd.def.MaxEnergy;
                    this.energy.SetLevel();
                    this.pawn.CheckLevelAndLimitingAbility();
                }
            }
            else
            {
                this.energy.powerflow = UnityEngine.Random.Range(3, 51) * 10000;
                this.energy.currentRWrd.def.MaxEnergy = this.energy.powerflow / 100;
                this.energy.completerealm = 0.1f;
                this.energy.trainDesireFactor = UnityEngine.Random.Range(1, 51);
            }
            this.pawn.CheckLevelAndLimitingAbility();
            foreach (PawnCapacityModifier pawnCapacityModifier in this.GetPCMList())
            {
                this.CurStage.capMods.Add(pawnCapacityModifier);
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look<Pawn_EnergyTracker>(ref this.energy, "energy", Array.Empty<object>());
            Scribe_Values.Look<bool>(ref this.IsInit, "IsInit", false, false);
            Scribe_Values.Look<bool>(ref this.openingBasicAbility, "openingBasicAbility", false, false);
            Scribe_Values.Look<string>(ref this.Route, "Route", null, false);
            Scribe_Defs.Look<RWrd_RouteDef>(ref this.route, "route");
            bool flag = Scribe.mode == LoadSaveMode.PostLoadInit;
            if (flag)
            {
                bool flag2 = this.route == null && !this.Route.NullOrEmpty();
                if (flag2)
                {
                    this.route = DefDatabase<RWrd_RouteDef>.GetNamed(this.Route.Split(new char[]
                    {
                        '_'
                    }).Last<string>(), true);
                }
                this.energy.pawn = this.pawn;
            }
        }
        public override void Tick()
        {
            base.Tick();
            JobDriver jobDriver = this.pawn.jobs.curDriver;
            this.energy.currentRWrd.def.MaxEnergy = this.energy.PowerFlow / 100;
            if (Find.TickManager.TicksGame % 60 == 0)
            {
                this.pawn.CheckLevelAndLimitingAbility();
            }
            if (this.energy.IsUpdateLevelTiming())
            {
                this.energy.SetLevel();
            }
            foreach (PawnCapacityModifier pcm in this.CurStage.capMods)
            {
                foreach (PawnCapacityModifier pawnCapacityModifier in this.GetPCMList())
                {
                    if (pcm.capacity == pawnCapacityModifier.capacity)
                    {
                        pcm.offset = pawnCapacityModifier.offset;
                    }
                }
            }
            if (Find.TickManager.TicksGame % 360000 == 0)
            {
                this.energy.SetPowerFlow(100);
            }
            if (Find.TickManager.TicksGame % 180 == 0)
            {
                this.energy.SetEnergy(300);
            }
            if (jobDriver != null)
            {
                if (jobDriver.GetType() == typeof(JobDriver_AttackMelee))
                {
                    int numMeleeAttacksMade = Traverse.Create(jobDriver).Field("numMeleeAttacksMade").GetValue<int>();
                    if (numMeleeAttacksMade > this.meleeAttackCounter)
                    {
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
                        this.energy.SetExp(exp2);
                    }
                    else
                    {
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

        public string Route = "";

        public RWrd_RouteDef route;

        public bool openingBasicAbility = false;

        public bool reeStartInit = false;
    }
}