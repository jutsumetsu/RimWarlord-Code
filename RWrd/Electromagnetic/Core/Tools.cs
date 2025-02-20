using Electromagnetic.Abilities;
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
