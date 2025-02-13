using Electromagnetic.Abilities;
using System;
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
    public class Tools
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
            damageAmount += baseDamage + root.energy.availableLevel + masteryOffset;
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
        public static readonly Texture2D AbilitySetNext = ContentFinder<Texture2D>.Get("UI/Gizmos/AbilitySet_Next", true);
    }
}
