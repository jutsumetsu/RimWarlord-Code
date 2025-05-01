using System;
using System.Collections.Generic;
using System.Linq;
using Electromagnetic.Abilities;
using RimWorld;
using Verse;
using Verse.AI;
using HarmonyLib;
using UnityEngine;
using Electromagnetic.Setting;
using Electromagnetic.UI;

namespace Electromagnetic.Core
{
    public class Hediff_RWrd_PowerRoot : HediffWithComps
    {
        //获取Gizmo
        public override IEnumerable<Gizmo> GetGizmos()
        {
            bool flag = this.energy != null;
            if (flag)
            {
                bool flag1 = !this.reeStartInit;
                bool flag2 = this.energy.AvailableLevel == 0;
                bool flag3 = this.energy.AvailableLevel >= 10;
                if (flag1)
                {
                    this.pawn.CheckEMAbilityLimiting();
                    this.reeStartInit = true;
                }
                Gizmo_Electromagnetic gizmo = new Gizmo_Electromagnetic(this.pawn, this)
                {
                    EnergyLabel = "RWrd_EP_Energy".Translate(),
                    CompleteRealmLabel = "Rwrd_CompleteRealm".Translate(),
                    PowerFlowLabel = "RWrd_PowerFlow".Translate()
                };
                if (flag2)
                {
                    gizmo.PowerLabel = "RWrd_BE".Translate();
                    gizmo.ExpLabel = "RWrd_Volt".Translate();
                }
                else
                {
                    if (Tools.IsChineseLanguage)
                    {
                        gizmo.PowerLabel = "RWrd_BM".Translate(this.energy.AvailableLevel.ToString("D2"));
                    }
                    else
                    {
                        gizmo.PowerLabel = "RWrd_BM".Translate(this.energy.AvailableLevel.ToString());
                    }
                    gizmo.ExpLabel = "RWrd_HorsePower".Translate();
                }
                yield return gizmo;
            }
            if (abilitySets.Count > 0)
            {
                var nextIndex = abilitysetIndex + 1;
                if (nextIndex > this.abilitySets.Count) nextIndex = 0;
                yield return new Command_ActionWithFloat
                {
                    defaultLabel = "RWrd_AbilitySetsNext".Translate(),
                    defaultDesc = "RWrd_AbilitySetsDesc".Translate(AbilitySetLabel(abilitysetIndex), AbilitySetLabel(nextIndex)),
                    icon = Tools.AbilitySetNext,
                    action = () => abilitysetIndex = nextIndex,
                    Order = 0f,
                    floatMenuGetter = GetAbilitySetFloatMenuOptions
                };
            }
            bool godMode = DebugSettings.godMode;
            if (godMode)
            {
                if (_cachedGodCommand != null && _cachedGodCommand.action != null)
                {
                    yield return _cachedGodCommand;
                }
                else
                {
                    _cachedGodCommand = new Command_ActionWithFloat
                    {
                        defaultLabel = "RWrd_SelectAction".Translate(),
                        defaultDesc = "RWrd_SelectActionDesc".Translate(),
                        icon = Tools.LiftingArrow2D,
                        floatMenuGetter = GetGodCommandOptions,
                        action = delegate ()
                        {
                            Messages.Message("RWrd_SelectActionFirst".Translate(), this.pawn, MessageTypeDefOf.PositiveEvent, true);
                        }
                    };
                    yield return _cachedGodCommand;
                }
                if (_cachedReloadCommand != null && _cachedReloadCommand.action != null)
                {
                    yield return _cachedReloadCommand;
                }
                else
                {
                    _cachedReloadCommand = new Command_ActionWithFloat
                    {
                        defaultLabel = "RWrd_SelectAction".Translate(),
                        defaultDesc = "RWrd_SelectActionDesc".Translate(),
                        icon = Tools.ReloadIcon,
                        floatMenuGetter = GetReloadSkillOptions,
                        action = delegate ()
                        {
                            Messages.Message("RWrd_SelectActionFirst".Translate(), this.pawn, MessageTypeDefOf.PositiveEvent, true);
                        }
                    };
                    yield return _cachedReloadCommand;
                }
                if (Tools.IsChineseLanguage)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "釋天風模擬器",
                        action = delegate ()
                        {
                            for (; this.energy.level < this.energy.LevelMax; this.energy.SetLevel())
                            {
                                this.energy.ForceSetExp(this.energy.MaxExp);
                            }
                            this.energy.ForceSetExp(this.energy.MaxExp);
                            this.energy.ForceSetPowerFlow(95000000);
                            this.energy.ForceSetCompleteRealm(9700);
                            this.pawn.UpdatePowerRootStageInfo();
                            this.pawn.UpdatePowerRootStageInfo(true);
                        },
                        icon = Tools.UltimateLeuiOu
                    };
                }
                /*yield return new Command_Action
                {
                    defaultLabel = "Temporary",
                    action = delegate ()
                    {
                        int lf = this.energy.level + 1;
                        int ntn = 0;
                    }
                };*/
            }
            yield break;
        }
        /// <summary>
        /// 技能组名字
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns></returns>
        private string AbilitySetLabel(int index)
        {
            if (index == abilitySets.Count) return "RWrd_AllAbility".Translate();
            return this.abilitySets[index].Name;
        }
        /// <summary>
        /// 获取技能组浮动菜单选项
        /// </summary>
        /// <returns></returns>
        private IEnumerable<FloatMenuOption> GetAbilitySetFloatMenuOptions()
        {
            for (var i = 0; i <= abilitySets.Count; i++)
            {
                var index = i;
                yield return new FloatMenuOption(this.AbilitySetLabel(index), delegate ()
                {
                    this.abilitysetIndex = index;
                });
            }
            yield break;
        }
        /// <summary>
        /// 升级
        /// </summary>
        private void LevelUp()
        {
            this.energy.ForceSetExp(this.energy.MaxExp);
            this.energy.SetLevel();
            this.pawn.CheckEMAbilityLimiting();
        }
        /// <summary>
        /// 提升力量
        /// </summary>
        private void IncreaseEXP()
        {
            if (this.energy.level != 0) this.energy.ForceSetExp(1000f);
            else this.energy.ForceSetExp(10000f);
        }
        /// <summary>
        /// 增加能量
        /// </summary>
        private void IncreaseEnergy()
        {
            if (this.energy.MaxEnergy <= 5000) this.energy.SetEnergy(1000f);
            else this.energy.SetEnergy(this.energy.MaxEnergy * 0.2f);
        }
        /// <summary>
        /// 提升完全境界
        /// </summary>
        private void IncreaseCompleteRealm()
        {
            if (this.energy.completerealm >= 1000) this.energy.SetCompleteRealm(1000);
            else if (this.energy.completerealm >= 100) this.energy.SetCompleteRealm(100);
            else if (this.energy.completerealm >= 10) this.energy.SetCompleteRealm(10);
            else if (this.energy.completerealm >= 1) this.energy.SetCompleteRealm(1);
            else this.energy.SetCompleteRealm(0.1f);
        }
        /// <summary>
        /// 提升力量流量
        /// </summary>
        private void IncreasePowerFlow()
        {
            if (this.energy.powerflow >= 10000000) this.energy.SetPowerFlow(10000000);
            else if (this.energy.powerflow >= 1000000) this.energy.SetPowerFlow(1000000);
            else this.energy.SetPowerFlow(10000);
        }
        /// <summary>
        /// 刷新技能树
        /// </summary>
        private void RefreshSkillTree()
        {
            this.pawn.CheckRouteUnlock();
            this.pawn.CheckEMAbilityLimiting();
        }
        /// <summary>
        /// 重载指定技能树
        /// </summary>
        private void ReloadSkillTree(RWrd_RouteDef routeDef)
        {
            this.RemoveRWrdAbilities();
            this.UnlockRoute(routeDef);
            this.pawn.CheckEMAbilityLimiting();
        }
        private IEnumerable<FloatMenuOption> GetGodCommandOptions()
        {
            yield return new FloatMenuOption("RWrd_LevelUP".Translate(), () =>
            {
                _cachedGodCommand.action = LevelUp; // 设置主命令的 Action
                _cachedGodCommand.defaultLabel = "RWrd_LevelUP".Translate(); // 更新标签
            });
            yield return new FloatMenuOption("RWrd_IncreaseEXP".Translate(), () =>
            {
                _cachedGodCommand.action = IncreaseEXP;
                _cachedGodCommand.defaultLabel = "RWrd_IncreaseEXP".Translate();
            });
            yield return new FloatMenuOption("RWrd_IncreaseEnergy".Translate(), () =>
            {
                _cachedGodCommand.action = IncreaseEnergy;
                _cachedGodCommand.defaultLabel = "RWrd_IncreaseEnergy".Translate();
            });
            yield return new FloatMenuOption("RWrd_IncreaseCompleteRealm".Translate(), () =>
            {
                _cachedGodCommand.action = IncreaseCompleteRealm;
                _cachedGodCommand.defaultLabel = "RWrd_IncreaseCompleteRealm".Translate();
            });
            yield return new FloatMenuOption("RWrd_IncreasePowerFlow".Translate(), () =>
            {
                _cachedGodCommand.action = IncreasePowerFlow;
                _cachedGodCommand.defaultLabel = "RWrd_IncreasePowerFlow".Translate();
            });
            yield break;
        }
        private IEnumerable<FloatMenuOption> GetReloadSkillOptions()
        {
            yield return new FloatMenuOption("RWrd_RefreshSkillTree".Translate(), () =>
            {
                _cachedReloadCommand.action = RefreshSkillTree;
                _cachedReloadCommand.defaultLabel = "RWrd_RefreshSkillTree".Translate();
                _cachedReloadCommand.icon = Tools.RefreshSkillTree2D;
            });
            yield return new FloatMenuOption("RWrd_ReloadDefaultSkillTree".Translate(), () =>
            {
                _cachedReloadCommand.action = () => ReloadSkillTree(RWrd_DefOf.Base);
                _cachedReloadCommand.defaultLabel = "RWrd_ReloadDefaultSkillTree".Translate();
                _cachedReloadCommand.icon = Tools.ReloadDefault2D;
            });
            yield return new FloatMenuOption("RWrd_ReloadBaakFamilySkillTree".Translate(), () =>
            {
                _cachedReloadCommand.action = () => ReloadSkillTree(RWrd_DefOf.SixSecret);
                _cachedReloadCommand.defaultLabel = "RWrd_ReloadBaakFamilySkillTree".Translate();
                _cachedReloadCommand.icon = Tools.ReloadBaak2D;
            });
            yield break;
        }
        /// <summary>
        /// 移除技能组
        /// </summary>
        /// <param name="set"></param>
        public void RemoveAbilitySet(AbilitySet set)
        {
            this.abilitySets.Remove(set);
            this.abilitysetIndex = Mathf.Clamp(this.abilitysetIndex, 0, this.abilitySets.Count);
        }
        public override void PostRemoved()
        {
            base.PostRemoved();
            this.RemovePowerRootCache();
            this.RemoveRWrdAbilities();
        }
        /// <summary>
        /// 移除武神技能
        /// </summary>
        private void RemoveRWrdAbilities()
        {
            List<Ability> tmp = new List<Ability>();
            this.pawn.abilities.abilities.ForEach(delegate (Ability a)
            {
                bool flag = a.def.abilityClass == typeof(RWrd_AbilityBase);
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
        private void RemovePowerRootCache()
        {
            PowerRootUtillity.powerRootCacheMap.Remove(this.pawn);
        }
        /// <summary>
        /// 解锁技能树
        /// </summary>
        /// <param name="route">技能树Def</param>
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
        /// <summary>
        /// 身体机能加成列表
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PawnCapacityModifier> GetPCMList
        {
            get
            {
                int lf = this.energy.AvailableLevel + this.energy.FinalLevelOffset + 1;
                int lf2 = lf + 1;
                int oft = (int)Math.Floor((lf - 50) / 5f) * 4;
                int oft2 = (int)Math.Floor((lf2 - 51) / 5f) * 4;
                bool flag = oft > 0;
                bool flag2 = lf == 100;
                int limit;
                int limit2;
                bool ultimate = this.energy.IsUltimate;
                int ntn = 0;
                if (flag2)
                {
                    if (this.energy.exp >= 9000)
                    {
                        ntn += 1;
                    }
                    if (this.energy.exp >= 9900)
                    {
                        ntn += 2;
                    }
                    if (this.energy.exp >= 9990)
                    {
                        ntn += 3;
                    }
                    if (this.energy.exp >= 9999)
                    {
                        ntn += 4;
                    }
                }
                if (flag)
                {
                    if (flag2)
                    {
                        limit = 50 + oft + ntn;
                        limit2 = 50 + oft2 + ntn;
                    }
                    else
                    {
                        limit = 50 + oft;
                        limit2 = 50 + oft2;
                    }
                }
                else
                {
                    limit = 50;
                    limit2 = 50;
                }
                float cr = this.energy.completerealm;
                yield return new PawnCapacityModifier
                {
                    capacity = PawnCapacityDefOf.Consciousness,
                    offset = ultimate ? lf + this.energy.PowerEnergy : Math.Min(lf, limit),
                };
                yield return new PawnCapacityModifier
                {
                    capacity = PawnCapacityDefOf.Moving,
                    offset = ultimate ? lf + this.energy.PowerEnergy : Math.Min(lf, limit),
                };
                yield return new PawnCapacityModifier
                {
                    capacity = PawnCapacityDefOf.Sight,
                    offset = ultimate ? lf2 + this.energy.PowerEnergy : Math.Min(lf2, limit2),
                };
                yield return new PawnCapacityModifier
                {
                    capacity = PawnCapacityDefOf.Hearing,
                    offset = ultimate ? lf2 + this.energy.PowerEnergy : Math.Min(lf2, limit2),
                };
                yield return new PawnCapacityModifier
                {
                    capacity = PawnCapacityDefOf.BloodFiltration,
                    offset = ultimate ? lf2 + this.energy.PowerEnergy : Math.Min(lf2, limit2),
                };
                yield return new PawnCapacityModifier
                {
                    capacity = PawnCapacityDefOf.BloodPumping,
                    offset = ultimate ? lf2 + this.energy.PowerEnergy : Math.Min(lf2, limit2),
                };
                yield return new PawnCapacityModifier
                {
                    capacity = PawnCapacityDefOf.Breathing,
                    offset = ultimate ? lf2 + this.energy.PowerEnergy : Math.Min(lf2, limit2),
                };
                yield return new PawnCapacityModifier
                {
                    capacity = PawnCapacityDefOf.Manipulation,
                    offset = cr,
                };
            }
        }
        /// <summary>
        /// 人物属性加成列表
        /// </summary>
        public IEnumerable<StatModifier> GetStatOffsetList
        {
            get
            {
                float cr = this.energy.completerealm;
                int acr = this.energy.AvailableCompleteRealm();
                int level = this.energy.AvailableLevel + this.energy.FinalLevelOffset;
                int lf = level + 1;
                if (this.energy.IsUltimate)
                {
                    lf += (int)Math.Floor(this.energy.PowerEnergy);
                }
                yield return new StatModifier
                {
                    stat = StatDefOf.ShootingAccuracyPawn,
                    value = acr,
                };
                yield return new StatModifier
                {
                    stat = StatDefOf.ShootingAccuracyFactor_Touch,
                    value = cr,
                };
                yield return new StatModifier
                {
                    stat = StatDefOf.ShootingAccuracyFactor_Short,
                    value = cr,
                };
                yield return new StatModifier
                {
                    stat = StatDefOf.ShootingAccuracyFactor_Medium,
                    value = cr,
                };
                yield return new StatModifier
                {
                    stat = StatDefOf.ShootingAccuracyFactor_Long,
                    value = cr,
                };
                yield return new StatModifier
                {
                    stat = StatDefOf.MeleeHitChance,
                    value = acr,
                };
                yield return new StatModifier
                {
                    stat = StatDefOf.HuntingStealth,
                    value = cr,
                };
                yield return new StatModifier
                {
                    stat = StatDefOf.MedicalSurgerySuccessChance,
                    value = cr,
                };
                yield return new StatModifier
                {
                    stat = StatDefOf.MedicalTendQuality,
                    value = cr,
                };
                yield return new StatModifier
                {
                    stat = StatDefOf.MeleeDodgeChance,
                    value = acr,
                };
                yield return new StatModifier
                {
                    stat = StatDefOf.ComfyTemperatureMax,
                    value = 100 + 50 * lf,
                };
                yield return new StatModifier
                {
                    stat = StatDefOf.ComfyTemperatureMin,
                    value = -50 - 50 * lf,
                };
                /*yield return new StatModifier
                {
                    stat = StatDefOf.MeleeCooldownFactor,
                    value = -lf * 0.02f,
                };*/
                yield return new StatModifier
                {
                    stat = StatDefOf.InjuryHealingFactor,
                    value = (int)Math.Ceiling(lf / 2f) * 5,
                };
                yield return new StatModifier
                {
                    stat = StatDefOf.IncomingDamageFactor,
                    value = -cr,
                };
                if (level >= 10)
                {
                    yield return new StatModifier
                    {
                        stat = StatDefOf.NegotiationAbility,
                        value = 0.8f,
                    };
                }
                if (ModDetector.SOSIsLoaded)
                {
                    yield return new StatModifier
                    {
                        stat = RWrd_DefOf.HypoxiaResistance,
                        value = Math.Min(level * 0.05f, 1f),
                    };
                    yield return new StatModifier
                    {
                        stat = RWrd_DefOf.DecompressionResistance,
                        value = Math.Min(level * 0.05f, 1f),
                    };
                }
            }
        }
        /// <summary>
        /// 人物属性乘数列表
        /// </summary>
        public IEnumerable<StatModifier> GetStatFactorList
        {
            get
            {
                int level = this.energy.AvailableLevel + this.energy.FinalLevelOffset;
                int lf = level + 1;
                float lifeFactor;
                if (this.energy.IsUltimate)
                {
                    lf += (int)Math.Floor(this.energy.PowerEnergy);
                    lifeFactor = 999999999;
                }
                else
                {
                    lifeFactor = 3.75f;
                }
                yield return new StatModifier
                {
                    stat = StatDefOf.CarryingCapacity,
                    value = lf,
                };
                yield return new StatModifier
                {
                    stat = StatDefOf.GlobalLearningFactor,
                    value = lf,
                };
                yield return new StatModifier
                {
                    stat = StatDefOf.ImmunityGainSpeed,
                    value = lf,
                };
                yield return new StatModifier
                {
                    stat = StatDefOf.ToxicResistance,
                    value = lf,
                };
                yield return new StatModifier
                {
                    stat = StatDefOf.LifespanFactor,
                    value = lifeFactor,
                };
                if (RWrdSettings.NoRestRequired)
                {
                    yield return new StatModifier
                    {
                        stat = StatDefOf.RestFallRateFactor,
                        value = 0,
                    };
                }
                yield return new StatModifier
                {
                    stat = StatDefOf.Flammability,
                    value = 0,
                };
                yield return new StatModifier
                {
                    stat = StatDefOf.StaggerDurationFactor,
                    value = 0,
                };
                if (ModDetector.DBHIsLoaded && RWrdSettings.NoFoodDrinkRequired)
                {
                    yield return new StatModifier
                    {
                        stat = RWrd_DefOf.BladderRateMultiplier,
                        value = Math.Max(1 - level * 0.05f, 0f),
                    };
                    if (ModDetector.DBHThirstExist)
                    {
                        yield return new StatModifier
                        {
                            stat = RWrd_DefOf.ThirstRateMultiplier,
                            value = Math.Max(1 - level * 0.05f, 0f),
                        };
                    }
                }
            }
        }
        /// <summary>
        /// 疾病免疫
        /// </summary>
        private List<HediffDef> GetImmuneTo
        {
            get
            {
                var list = new List<HediffDef>
                {
                    RWrd_DefOf.Asthma,
                    RWrd_DefOf.BadBack,
                    HediffDefOf.Carcinoma,
                    HediffDefOf.Dementia,
                    RWrd_DefOf.Flu,
                    RWrd_DefOf.Frail
                };
                return list;
            }
        }
        // 重写stage
        public override HediffStage CurStage
        {
            get
            {
                float num1;
                float num2;
                int level = this.energy.AvailableLevel + this.energy.FinalLevelOffset;
                if (this.energy.AvailableLevel == 0)
                {
                    num1 = 0.1f;
                }
                else
                {
                    num1 = 0;
                }
                if (RWrdSettings.NoFoodDrinkRequired)
                {
                    num2 = Math.Max(1 - level * 0.05f, 0f);
                }
                else
                {
                    num2 = 1;
                }
                if (this.stage == null)
                {
                    this.stage = new HediffStage
                                {
                                    painFactor = num1,
                                    becomeVisible = false,
                                    hungerRateFactor = num2,
                                    totalBleedFactor = 0,
                                    makeImmuneTo = this.GetImmuneTo,
                                    statOffsets = this.GetStatOffsetList.ToList(),
                                    statFactors = this.GetStatFactorList.ToList(),
                                    capMods = this.GetPCMList.ToList()
                                };
                }
                return this.stage;
            }
        }
        // 绘制详情页
        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            foreach (StatDrawEntry statDrawEntry in this.CurStage.SpecialDisplayStats())
            {
                yield return statDrawEntry;
            }
            yield break;
        }
        public override void PostMake()
        {
            base.PostMake();
            //赋予EnergyTracker
            bool flag = this.energy == null;
            if (flag)
            {
                this.energy = new Pawn_EnergyTracker(this.pawn);
                this.routes = new List<RWrd_RouteDef>();
                this.IsInit = true;
                this.initActivity = true;
            }
            bool flag3 = this.pawn.Faction != Faction.OfPlayer;
            if (flag3)
            {
                //NPC生成
                this.energy.level = this.InitialLevel();
                this.energy.powerflow = UnityEngine.Random.Range(3, 51) * 10000;
                float prenum = this.pawn.ageTracker.AgeBiologicalTicks / 3600000f;
                //年龄限制完全境界
                int age = (int)Math.Floor(prenum);
                int randomNum;
                int cacheNum;
                // 根据年龄确定初始随机数范围
                int initialMin = 1;
                int initialMax;
                if (age < 10)
                {
                    initialMax = 3;
                }
                else
                {
                    initialMax = 5;
                }
                randomNum = UnityEngine.Random.Range(initialMin, initialMax);
                if (randomNum >= 3)
                {
                    int loopCount;
                    if (age < 10)
                    {
                        loopCount = 0;
                    }
                    else if (age < 20)
                    {
                        loopCount = 1;
                    }
                    else if (age < 30)
                    {
                        loopCount = 2;
                    }
                    else
                    {
                        loopCount = 3;
                    }

                    for (int i = 0; i < loopCount; i++)
                    {
                        int min = 3 + i * 2;
                        int max = 7 + i * 2;
                        cacheNum = UnityEngine.Random.Range(min, max);
                        if (cacheNum > randomNum)
                        {
                            randomNum = cacheNum;
                        }
                    }
                }
                this.energy.completerealm = randomNum * 0.1f;
                this.energy.trainDesireFactor = UnityEngine.Random.Range(1, 51);
            }
            else
            {
                //殖民者生成
                this.energy.level = this.InitialLevel();
                this.energy.powerflow = UnityEngine.Random.Range(2, 11) * 50000;
                float prenum = this.pawn.ageTracker.AgeBiologicalTicks / 3600000f;
                //年龄限制完全境界
                int age = (int)Math.Floor(prenum);
                int randomNum;
                int cacheNum;
                // 根据年龄确定初始随机数范围
                int initialMin = 1;
                int initialMax;
                if (age < 10)
                {
                    initialMax = 3;
                }
                else
                {
                    initialMax = 5;
                }
                randomNum = UnityEngine.Random.Range(initialMin, initialMax);
                if (randomNum >= 3)
                {
                    int loopCount;
                    if (age < 10)
                    {
                        loopCount = 0;
                    }
                    else if (age < 20)
                    {
                        loopCount = 1;
                    }
                    else if (age < 30)
                    {
                        loopCount = 2;
                    }
                    else
                    {
                        loopCount = 3;
                    }

                    for (int i = 0; i < loopCount; i++)
                    {
                        int min = 3 + i * 2;
                        int max = 7 + i * 2;
                        cacheNum = UnityEngine.Random.Range(min, max);
                        if (cacheNum > randomNum)
                        {
                            randomNum = cacheNum;
                        }
                    }
                }
                this.energy.completerealm = randomNum * 0.1f;
                this.energy.trainDesireFactor = UnityEngine.Random.Range(1, 51);
            }
            this.pawn.CheckRouteUnlock(this);
            this.pawn.CheckEMAbilityLimiting(this);
            if (PowerRootUtillity.powerRootCacheMap == default(Dictionary<Pawn, Hediff_RWrd_PowerRoot>))
            {
                PowerRootUtillity.powerRootCacheMap = new Dictionary<Pawn, Hediff_RWrd_PowerRoot>();
            }
            PowerRootUtillity.powerRootCacheMap.Add(this.pawn, this);
            Log.Message("Root post make is called");
        }
        //保存数据
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look<Pawn_EnergyTracker>(ref this.energy, "energy", Array.Empty<object>());
            Scribe_Values.Look<bool>(ref this.IsInit, "IsInit", false, false);
            Scribe_Values.Look<bool>(ref this.openingBasicAbility, "openingBasicAbility", false, false);
            Scribe_Values.Look<bool>(ref this.SelfDestruction, "selfdestruction", false, false);
            Scribe_Values.Look<bool>(ref this.SDWeak, "sdweak", false, false);
            Scribe_Values.Look<bool>(ref this.SDRecharge, "sdrecharge", false, false);
            Scribe_Values.Look<int>(ref this.SDTolerance, "sdtolerance", 0, false);
            Scribe_Values.Look<int>(ref this.abilitysetIndex, "abilitysetIndex", 0, false);
            Scribe_Collections.Look<RWrd_RouteDef>(ref this.routes, "routes", LookMode.Def, Array.Empty<object>());
            Scribe_Collections.Look<AbilitySet>(ref this.abilitySets, "abilitysets", LookMode.Deep, Array.Empty<object>());
            bool flag = Scribe.mode == LoadSaveMode.PostLoadInit;
            if (flag)
            {
                if (this.routes == null)
                {
                    this.routes = new List<RWrd_RouteDef>();
                }
                if (this.abilitySets == null)
                {
                    this.abilitySets = new List<AbilitySet>();
                }
                if (PowerRootUtillity.powerRootCacheMap == default(Dictionary<Pawn, Hediff_RWrd_PowerRoot>))
                {
                    PowerRootUtillity.powerRootCacheMap = new Dictionary<Pawn, Hediff_RWrd_PowerRoot>();
                }
                if (!PowerRootUtillity.powerRootCacheMap.ContainsKey(pawn))
                {
                    PowerRootUtillity.powerRootCacheMap.Add(this.pawn, this);
                }
                this.energy.pawn = this.pawn;
                this.RefreshSkillTree();
            }
        }
        public override void Tick()
        {
            base.Tick();
            //最大能量赋值
            if (Find.TickManager.TicksGame % 360000 == 0)
            {
                //随时间增加力量流量
                this.energy.SetPowerFlow(100);
            }
            if (Find.TickManager.TicksGame % 180 == 0)
            {
                if (!SDRecharge)
                {
                    this.energy.EnergyRecharge();
                }
                else
                {
                    if (this.SDRechargeTime > 0)
                    {
                        this.SDRechargeTime -= 1;
                    }
                    else
                    {
                        this.SDRecharge = false;
                    }
                }
                if (this.SDWeak)
                {
                    if (this.energy.level < this.energy.LevelMax)
                    {
                        this.energy.SetExp(400);
                    }
                    else
                    {
                        float deltaExp = this.energy.Oexp - this.energy.Exp;
                        if (deltaExp >= 400)
                        {
                            this.energy.SetExp(400);
                        }
                        else
                        {
                            this.energy.ForceSetExp(deltaExp);
                            this.energy.Oexp = 0;
                            this.SDWeak = false;
                        }
                    }
                }
            }
        }
        private Command_ActionWithFloat _cachedGodCommand;
        private Command_ActionWithFloat _cachedReloadCommand;

        public int meleeAttackCounter = 0;

        public Pawn_EnergyTracker energy;

        private bool IsInit = false;
        public bool initActivity = false;

        public List<RWrd_RouteDef> routes;

        public bool openingBasicAbility = false;

        public bool reeStartInit = false;

        public bool SelfDestruction = false;
        public bool SDWeak = false;
        public bool SDRecharge = false;
        public int SDRechargeTime = 0;
        public int SDTolerance = 0;

        public HediffStage stage;

        public List<AbilitySet> abilitySets = new List<AbilitySet>();
        public int abilitysetIndex;
    }
}