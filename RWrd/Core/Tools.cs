using Electromagnetic.Abilities;
using Electromagnetic.Setting;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Electromagnetic.Core
{
    /// <summary>
    /// 工具箱
    /// </summary>
    [StaticConstructorOnStartup]
    public static class Tools
    {
        /// <summary>
        /// 检测中文语言
        /// </summary>
        public static bool IsChineseLanguage
        {
            get
            {
                return LanguageDatabase.activeLanguage.ToString() == "Simplified Chinese" || LanguageDatabase.activeLanguage.ToString() == "Traditional Chinese";
            }
        }
        /// <summary>
        /// 角度工具
        /// </summary>
        /// <param name="p1">目标点</param>
        /// <param name="p2">原点</param>
        /// <returns></returns>
        public static double PointsAngleTool(IntVec3 p1, IntVec3 p2)
        {
            return Math.Atan2((double)(p2.x - p1.x), (double)(p2.z - p1.z)) * 180.0 / 3.141592653589793;
        }
        /// <summary>
        /// 最终伤害计算
        /// </summary>
        /// <param name="root">力量之源</param>
        /// <param name="baseDamage">基础伤害</param>
        /// <param name="masteryOffset">精通加成</param>
        /// <returns></returns>
        public static float FinalDamage(Hediff_RWrd_PowerRoot root, float baseDamage, float masteryOffset = 0)
        {
            float damageAmount = 0;
            damageAmount += baseDamage + root.energy.AvailableLevel + root.energy.FinalLevelOffset + masteryOffset;
            if (root.energy.IsUltimate)
            {
                damageAmount += (int)Math.Floor(root.energy.PowerEnergy);
            }
            damageAmount *= root.energy.Multiplier;
            return damageAmount;
        }
        public static Hediff_TargetBase MakeEMHediff(HediffDef def, Pawn pawn, Hediff_RWrd_PowerRoot root, BodyPartRecord partRecord = null)
        {
            if (pawn == null)
            {
                Log.Error("Cannot make hediff " + def + " for null pawn.");
                return null;
            }
            Hediff_TargetBase hediff = (Hediff_TargetBase)Activator.CreateInstance(def.hediffClass);
            hediff.def = def;
            hediff.pawn = pawn;
            hediff.Part = partRecord;
            hediff.root = root;
            hediff.loadID = Find.UniqueIDsManager.GetNextHediffID();
            hediff.PostMake();
            return hediff;
        }
        public static IEnumerable<TResult> SelectNotNull<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, IEnumerable<TResult>> selector)
        {
            return source
                .Select(selector)
                .Where(enumerable => enumerable != null)
                .SelectMany(enumerable => enumerable);
        }
        /// <summary>
        /// 移除已存在的健康状态
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="defToRemove"></param>
        public static void RemoveHediffIfExists(Pawn pawn, HediffDef defToRemove)
        {
            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(defToRemove);
            if (hediff != null)
            {
                pawn.health.RemoveHediff(hediff);
            }
        }
        /// <summary>
        /// 一百万匹力量
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="victim"></param>
        public static void MillionHp(Pawn caster, Pawn victim, DamageInfo? dinfo = null)
        {
            Hediff_RWrd_PowerRoot Croot = caster.GetPowerRoot();
            bool VPowerful = victim.IsHavePowerRoot();
            bool CUltimate = Croot.energy.IsUltimate;
            if (VPowerful)
            {
                Hediff_RWrd_PowerRoot Vroot = victim.GetPowerRoot();
                bool VUltimate = Vroot.energy.IsUltimate;
                if (VUltimate)
                {
                }
                else
                {
                    if (dinfo != null)
                    {
                        victim.TakeDamage((DamageInfo)dinfo);
                    }
                    if (!victim.Dead)
                    {
                        victim.Kill(dinfo);
                    }
                }
            }
            else
            {
                if (dinfo != null)
                {
                    victim.TakeDamage((DamageInfo)dinfo);
                }
                if (!victim.Dead)
                {
                    victim.Kill(dinfo);
                }
            }
            if (CUltimate)
            {
                int reduceEnergy = Croot.energy.MaxEnergy >= 5000 ? Mathf.CeilToInt(Croot.energy.MaxEnergy * 0.25f) : 1000;
                Croot.energy.SetEnergy(-reduceEnergy);
                Croot.SDRecharge = true;
                Croot.SDRechargeTime = 60;
            }
            else
            {
                int rrMin = 1 + Mathf.FloorToInt(Croot.SDTolerance / 60);
                int rrMax = 6 - Mathf.CeilToInt(Croot.SDTolerance / 25);
                int fateDice = UnityEngine.Random.Range(1, 101);
                int tolerence = RWrdSettings.SDDefaultSuccessRate + Croot.SDTolerance;
                if (tolerence < 100)
                {
                    if (fateDice <= tolerence)
                    {
                        Tools.DamageUntilSI(caster);
                        Croot.SDTolerance += 1;
                    }
                    else
                    {
                        Tools.DamageUntilSI(caster);
                        caster.Kill(dinfo);
                    }
                }
                else
                {
                    Croot.energy.SetPowerFlow(100000);
                }
                Croot.energy.level -= UnityEngine.Random.Range(rrMin, rrMax);
                Croot.energy.Oexp = Croot.energy.Exp;
                Croot.SDWeak = true;
            }
            Croot.SelfDestruction = false;
        }
        public static void DamageUntilSI(Pawn p, bool allowBleedingWounds = true, ThingDef sourceDef = null, BodyPartGroupDef bodyGroupDef = null)
        {
            if (p.Downed)
            {
                return;
            }
            HediffSet hediffSet = p.health.hediffSet;
            p.health.forceDowned = true;
            IEnumerable<BodyPartRecord> source = from x in Tools.HittablePartsViolence(hediffSet)
                                                 where !p.health.hediffSet.hediffs.Any((Hediff y) => y.Part == x && y.CurStage != null && y.CurStage.partEfficiencyOffset < 0f)
                                                 select x;
            int num = 0;
            bool seriousInjury = p.health.summaryHealth.SummaryHealthPercent <= 0.3;
            while (num < 100 && !seriousInjury && source.Any<BodyPartRecord>())
            {
                num++;
                BodyPartRecord bodyPartRecord = source.RandomElementByWeight((BodyPartRecord x) => x.coverageAbs);
                int num2 = Mathf.RoundToInt(UnityEngine.Random.Range(p.GetPowerRoot().energy.DamageImmunityThreshold, hediffSet.GetPartHealth(bodyPartRecord) / 10));
                float statValue = p.GetStatValue(StatDefOf.IncomingDamageFactor, true, -1);
                if (statValue > 0f)
                {
                    num2 = (int)((float)num2 / statValue);
                }
                num2 -= 3;
                if (num2 > 0 && (num2 >= 8 || num >= 250))
                {
                    if (num > 275)
                    {
                        num2 = Rand.Range(1, 8);
                    }
                    DamageDef damageDef = DamageDefOf.Bomb;
                    int num3 = Rand.RangeInclusive(Mathf.RoundToInt((float)num2 * 0.65f), num2);
                    HediffDef hediffDefFromDamage = HealthUtility.GetHediffDefFromDamage(damageDef, p, bodyPartRecord);
                    if (!p.health.WouldDieAfterAddingHediff(hediffDefFromDamage, bodyPartRecord, (float)num3 * p.GetStatValue(StatDefOf.IncomingDamageFactor, true, -1)))
                    {
                        DamageInfo dinfo = new DamageInfo(damageDef, (float)num3, 999f, -1f, null, bodyPartRecord, null, DamageInfo.SourceCategory.ThingOrUnknown, null, true, true, QualityCategory.Normal, false);
                        dinfo.SetAllowDamagePropagation(false);
                        DamageWorker.DamageResult damageResult = p.TakeDamage(dinfo);
                        if (damageResult.hediffs != null)
                        {
                            foreach (Hediff hediff in damageResult.hediffs)
                            {
                                if (sourceDef != null)
                                {
                                    hediff.sourceDef = sourceDef;
                                }
                                if (bodyGroupDef != null)
                                {
                                    hediff.sourceBodyPartGroup = bodyGroupDef;
                                }
                            }
                        }
                    }
                }
            }
            if (p.Dead && !p.kindDef.forceDeathOnDowned)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(p + " died during GiveInjuriesToForceDowned");
                for (int i = 0; i < p.health.hediffSet.hediffs.Count; i++)
                {
                    stringBuilder.AppendLine("   -" + p.health.hediffSet.hediffs[i]);
                }
                Log.Error(stringBuilder.ToString());
            }
            p.health.forceDowned = false;
        }
        private static IEnumerable<BodyPartRecord> HittablePartsViolence(HediffSet bodyModel)
        {
            return from x in bodyModel.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null)
                   where x.depth == BodyPartDepth.Outside || (x.depth == BodyPartDepth.Inside && x.def.IsSolid(x, bodyModel.hediffs))
                   select x;
        }
        /// <summary>
        /// 能量条材质
        /// </summary>
        public static readonly Texture2D EnergyBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 0.84f, 0f));
        /// <summary>
        /// 能量条减少材质
        /// </summary>
        public static readonly Texture2D EnergyBarTexReduce = SolidColorMaterials.NewSolidColorTexture(new Color(0.73f, 0.65f, 0.24f));
        /// <summary>
        /// 能量条空材质
        /// </summary>
        public static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);
        /// <summary>
        /// 技能组切换图标
        /// </summary>
        public static readonly Texture2D AbilitySetNext = ContentFinder<Texture2D>.Get("UI/Gizmos/AbilitySet_Next", true);
        /// <summary>
        /// 提升箭头图标
        /// </summary>
        public static readonly Texture2D LiftingArrow2D = ContentFinder<Texture2D>.Get("UI/Gizmos/LiftingArrow", true);
        /// <summary>
        /// 终极雷奥/释天风
        /// </summary>
        public static readonly Texture2D UltimateLeuiOu = ContentFinder<Texture2D>.Get("UI/Gizmos/UltimateLeuiOu", true);
        /// <summary>
        /// 重载图标
        /// </summary>
        public static readonly Texture2D ReloadIcon = ContentFinder<Texture2D>.Get("UI/Gizmos/ReloadIcon", true);
        public static readonly Texture2D RefreshSkillTree2D = ContentFinder<Texture2D>.Get("UI/Gizmos/RefreshSkillTree", true);
        public static readonly Texture2D ReloadDefault2D = ContentFinder<Texture2D>.Get("UI/Gizmos/ReloadDefault", true);
        public static readonly Texture2D ReloadBaak2D = ContentFinder<Texture2D>.Get("UI/Gizmos/ReloadBaak", true);
        /// <summary>
        /// 飞行技能
        /// </summary>
        public static readonly Texture2D Flight2D = ContentFinder<Texture2D>.Get("Ability/Base/Flight", true);
        /// <summary>
        /// 原子分裂
        /// </summary>
        public static readonly Texture2D AtomSplit2D = ContentFinder<Texture2D>.Get("Ability/Base/AtomSplit", true);
    }
}
