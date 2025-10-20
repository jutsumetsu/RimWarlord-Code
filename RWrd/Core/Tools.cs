using Electromagnetic.Abilities;
using Electromagnetic.Setting;
using Electromagnetic.UI;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public static Hediff_RWrd_PowerRoot MakePowerRoot(HediffDef def, Pawn pawn, bool Qigong = false, bool newBorn = false, bool starter = false)
        {
            if (pawn == null)
            {
                Log.Error("Cannot make hediff " + ((def != null) ? def.ToString() : null) + " for null pawn.");
                return null;
            }
            Hediff_RWrd_PowerRoot hediff = (Hediff_RWrd_PowerRoot)Activator.CreateInstance(def.hediffClass);
            hediff.def = def;
            hediff.pawn = pawn;
            hediff.Part = null;
            hediff.Qigong = Qigong;
            hediff.newBorn = newBorn;
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
        /// <summary>
        /// 生成区间[min, max]内符合正态分布的随机数
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float GaussianRandom(float min, float max)
        {
            // 计算区间中心（均值）和标准差（区间宽度/6覆盖99.7%数据）
            float mean = (min + max) / 2f;
            float stdDev = (max - min) / 6f;

            // 使用Box-Muller变换生成标准正态分布
            float u1 = 1.0f - UnityEngine.Random.Range(0f, 1f); // 避免0值
            float u2 = 1.0f - UnityEngine.Random.Range(0f, 1f);
            float randStdNormal = Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Sin(2f * Mathf.PI * u2);

            // 缩放并平移至目标分布
            float randNormal = mean + stdDev * randStdNormal;

            // 确保结果在指定区间内（3σ原则下概率<0.3%需要重试）
            return Mathf.Clamp(randNormal, min, max);
        }
        /// <summary>
        /// 带强度参数的重载
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="strength"></param>
        /// <returns></returns>
        public static float GaussianRandom(float min, float max, float strength)
        {
            strength = Mathf.Clamp(strength, 0.1f, 10f);
            float mean = (min + max) / 2f;
            float stdDev = (max - min) / (2f * strength);

            float u1 = 1.0f - UnityEngine.Random.Range(0f, 1f);
            float u2 = 1.0f - UnityEngine.Random.Range(0f, 1f);
            float randNormal = mean + stdDev * Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Sin(2f * Mathf.PI * u2);

            return Mathf.Clamp(randNormal, min, max);
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
        public static void SetPawnGender(this Pawn pawn, Gender gender)
        {
            if (pawn != null)
            {
                pawn.gender = gender;
                Messages.Message(gender.ToString(), MessageTypeDefOf.SilentInput);
                pawn.SetDirty();
            }
        }
        private static IEnumerable<BodyPartRecord> HittablePartsViolence(HediffSet bodyModel)
        {
            return from x in bodyModel.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null)
                   where x.depth == BodyPartDepth.Outside || (x.depth == BodyPartDepth.Inside && x.def.IsSolid(x, bodyModel.hediffs))
                   select x;
        }
        public static int IntRestrict(float preNum)
        {
            return (int)Math.Min(preNum, int.MaxValue);
        }
        internal static void SetMemberValue(this object obj, string name, object value)
        {
            BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            FieldInfo fieldInfo = (obj != null) ? obj.GetType().GetField(name, bindingAttr) : null;
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(obj, value);
            }
        }
        public static void SetFMOIcon(this FloatMenuOption fmo, Texture2D t)
        {
            bool flag = t != null;
            if (flag)
            {
                fmo.SetMemberValue("iconTex", t);
            }
        }
        internal static Texture2D GetTIcon<T>(this T def, Selected s = null)
        {
            Texture2D texture2D = null;
            bool flag = typeof(T) == typeof(ThingDef);
            if (flag)
            {
                texture2D = (def as ThingDef).uiIcon;
            }
            else
            {
                bool flag2 = typeof(T) == typeof(Ability);
                if (flag2)
                {
                    texture2D = (def as Ability).def.uiIcon;
                }
                else
                {
                    bool flag3 = typeof(T) == typeof(AbilityDef);
                    if (flag3)
                    {
                        texture2D = (def as AbilityDef).uiIcon;
                    }
                    else
                    {
                        bool flag4 = typeof(T) == typeof(HairDef);
                        if (flag4)
                        {
                            texture2D = (def as HairDef).Icon;
                        }
                        else
                        {
                            bool flag5 = typeof(T) == typeof(BeardDef);
                            if (flag5)
                            {
                                texture2D = (def as BeardDef).Icon;
                            }
                            else
                            {
                                bool flag6 = typeof(T) == typeof(GeneDef);
                                if (flag6)
                                {
                                    texture2D = (def as GeneDef).Icon;
                                }
                                else
                                {
                                    bool flag7 = typeof(T) == typeof(XenotypeDef);
                                    if (flag7)
                                    {
                                        texture2D = (def as XenotypeDef).Icon;
                                    }
                                    else
                                    {
                                        bool flag8 = typeof(T) == typeof(TattooDef);
                                        if (flag8)
                                        {
                                            texture2D = (def as TattooDef).Icon;
                                    }
                                        else
                                        {
                                            bool flag10 = typeof(T) == typeof(ThingStyleDef);
                                            if (flag10)
                                            {
                                                texture2D = Dialog_​BodyRemold.IconForStyleCustom(s, def as ThingStyleDef);
                                            }
                                            else
                                            {
                                                bool flag11 = typeof(T) == typeof(ThingCategoryDef);
                                                if (flag11)
                                                {
                                                    texture2D = (def as ThingCategoryDef).icon;
                                                }
                                                else
                                                {
                                                    bool flag12 = typeof(T) == typeof(CustomXenotype);
                                                    if (flag12)
                                                    {
                                                        XenotypeIconDef iconDef = (def as CustomXenotype).IconDef;
                                                        texture2D = ((iconDef != null) ? iconDef.Icon : null);
                                                    }
                                                    else
                                                    {
                                                        bool flag13 = typeof(T) == typeof(Gene);
                                                        if (flag13)
                                                        {
                                                            texture2D = (def as Gene).def.Icon;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return (texture2D == BaseContent.BadTex) ? null : texture2D;
        }
        internal static string SubstringBackwardFrom(this string text, string startFrom, bool withoutIt = true)
        {
            bool flag = text != null;
            if (flag)
            {
                int num = text.LastIndexOf(startFrom);
                bool flag2 = num >= 0;
                if (flag2)
                {
                    if (withoutIt)
                    {
                        return text.Substring(num + startFrom.Length);
                    }
                    return text.Substring(num);
                }
            }
            return text;
        }
        internal static string SubstringBackwardTo(this string text, string endOn, bool withoutIt = true)
        {
            bool flag = text != null;
            if (flag)
            {
                int num = text.LastIndexOf(endOn);
                bool flag2 = num >= 0;
                if (flag2)
                {
                    if (withoutIt)
                    {
                        return text.Substring(0, num);
                    }
                    return text.Substring(num);
                }
            }
            return text;
        }
        internal static int NextOrPrevIndex<T>(this HashSet<T> l, int index, bool next, bool random)
        {
            bool flag = l.NullOrEmpty<T>();
            int result;
            if (flag)
            {
                result = 0;
            }
            else if (random)
            {
                result = l.IndexOf(l.RandomElement<T>());
            }
            else
            {
                if (next)
                {
                    bool flag2 = index + 1 < l.Count;
                    if (flag2)
                    {
                        index++;
                    }
                    else
                    {
                        index = 0;
                    }
                }
                else
                {
                    bool flag3 = index - 1 >= 0;
                    if (flag3)
                    {
                        index--;
                    }
                    else
                    {
                        index = l.Count - 1;
                    }
                }
                result = index;
            }
            return result;
        }
        internal static object CallMethod(this object obj, string name, object[] param)
        {
            BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            object result;
            if (obj == null)
            {
                result = null;
            }
            else
            {
                MethodInfo method = obj.GetType().GetMethod(name, bindingAttr);
                result = ((method != null) ? method.Invoke(obj, param) : null);
            }
            return result;
        }
        internal static T At<T>(this HashSet<T> l, int index)
        {
            return l.NullOrEmpty<T>() ? default(T) : l.ElementAt(index);
        }
        internal static T At<T>(this List<T> l, int index)
        {
            return l.NullOrEmpty<T>() ? default(T) : l.ElementAt(index);
        }
        internal static int IndexOf<T>(this HashSet<T> l, T val)
        {
            return (l.NullOrEmpty<T>() || val == null) ? 0 : l.FirstIndexOf((T y) => val.Equals(y));
        }
        internal static int NextOrPrevIndex<T>(this List<T> l, int index, bool next, bool random)
        {
            bool flag = l.NullOrEmpty<T>();
            int result;
            if (flag)
            {
                result = 0;
            }
            else if (random)
            {
                result = l.IndexOf(l.RandomElement<T>());
            }
            else
            {
                if (next)
                {
                    bool flag2 = index + 1 < l.Count;
                    if (flag2)
                    {
                        index++;
                    }
                    else
                    {
                        index = 0;
                    }
                }
                else
                {
                    bool flag3 = index - 1 >= 0;
                    if (flag3)
                    {
                        index--;
                    }
                    else
                    {
                        index = l.Count - 1;
                    }
                }
                result = index;
            }
            return result;
        }
        internal static string SubstringTo(this string text, string endOn, bool withoutIt = true)
        {
            bool flag = text != null;
            if (flag)
            {
                int num = text.IndexOf(endOn);
                bool flag2 = num >= 0;
                if (flag2)
                {
                    if (withoutIt)
                    {
                        return text.Substring(0, num);
                    }
                    return text.Substring(0, num + endOn.Length);
                }
            }
            return text;
        }
        internal static string SubstringTo(this string text, string to, int occuranceCount)
        {
            string text2 = text;
            string text3 = "";
            for (int i = 0; i < occuranceCount; i++)
            {
                text3 += text2.SubstringTo(to, false);
                text2 = text2.SubstringFrom(to, true);
            }
            bool flag = text3.Length > 0;
            string result;
            if (flag)
            {
                result = text3.Substring(0, text3.Length - 1);
            }
            else
            {
                result = text3;
            }
            return result;
        }
        internal static string SubstringFrom(this string text, string from, int occuranceCount)
        {
            string text2 = text;
            for (int i = 0; i < occuranceCount; i++)
            {
                text2 = text2.SubstringFrom(from, true);
            }
            return text2;
        }
        internal static string SubstringFrom(this string text, string startFrom, bool withoutIt = true)
        {
            bool flag = text != null;
            if (flag)
            {
                int num = text.IndexOf(startFrom);
                bool flag2 = num >= 0;
                if (flag2)
                {
                    if (withoutIt)
                    {
                        return text.Substring(num + startFrom.Length);
                    }
                    return text.Substring(num);
                }
            }
            return text;
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

        internal static System.Random zufallswert;
    }
}
