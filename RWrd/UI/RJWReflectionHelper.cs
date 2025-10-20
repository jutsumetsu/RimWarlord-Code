using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using HarmonyLib;
using System.Reflection;
using Electromagnetic.Core;
using rjw.Modules.Interactions;
using System.Collections;

namespace Electromagnetic.UI
{
    public static class RJWReflectionHelper
    {
        private static bool rjwLoaded;
        private static Type LewdPartHelper;
        private static Type LewdablePart;
        private static Type lewdPartsType;
        private static Type sexPartHediffType;
        private static Type genitalHelperType;
        private static Type rjwISexPartHediffType;
        private static Type sexPartAdderType;
        private static Type dialogPartcardType;

        // 缓存反射获取的方法和属性
        private static MethodInfo getLewdPartsMethod;
        private static MethodInfo populateAllMethod;
        private static MethodInfo asSexPartHediffsMethod;
        private static MethodInfo getBreastsBPRMethod;
        private static MethodInfo getGenitalsBPRMethod;
        private static MethodInfo getAnusBPRMethod;
        private static MethodInfo makePartMethod;

        static RJWReflectionHelper()
        {
            rjwLoaded = ModDetector.RJWIsLoaded;

            if (!rjwLoaded) return;

            try
            {
                // 加载RJW程序集
                var rjwAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "RJW");
                if (rjwAssembly == null) return;

                // 获取需要的类型
                LewdPartHelper = rjwAssembly.GetType("rjw.Modules.Interactions.LewdPartHelper");
                lewdPartsType = rjwAssembly.GetType("rjw.Modules.Interactions.LewdParts");
                LewdablePart = rjwAssembly.GetType("rjw.Modules.Interactions.RJWLewdablePart");
                sexPartHediffType = rjwAssembly.GetType("rjw.ISexPartHediff");
                genitalHelperType = rjwAssembly.GetType("rjw.Genital_Helper");
                rjwISexPartHediffType = rjwAssembly.GetType("rjw.ISexPartHediff");
                sexPartAdderType = rjwAssembly.GetType("rjw.SexPartAdder");
                dialogPartcardType = rjwAssembly.GetType("rjw.Dialog_Partcard");

                // 获取方法和属性
                getLewdPartsMethod = AccessTools.Method(LewdPartHelper, "GetLewdParts", new Type[] { typeof(Pawn) });
                populateAllMethod = AccessTools.Method(lewdPartsType, "PopulateAll", Type.EmptyTypes);
                Type listOfLewdablePartType = typeof(List<>).MakeGenericType(LewdablePart);
                asSexPartHediffsMethod = AccessTools.Method(LewdPartHelper, "AsSexPartHediffs", new Type[] { listOfLewdablePartType });

                getBreastsBPRMethod = AccessTools.Method(genitalHelperType, "get_breastsBPR", new Type[] { typeof(Pawn) });
                getGenitalsBPRMethod = AccessTools.Method(genitalHelperType, "get_genitalsBPR", new Type[] { typeof(Pawn) });
                getAnusBPRMethod = AccessTools.Method(genitalHelperType, "get_anusBPR", new Type[] { typeof(Pawn) });

                makePartMethod = AccessTools.Method(sexPartAdderType, "MakePart", new Type[] { typeof(HediffDef), typeof(Pawn), typeof(BodyPartRecord) });
            }
            catch (Exception ex)
            {
                Log.Error($"RJW反射初始化失败: {ex}");
                rjwLoaded = false;
            }
        }

        public static void DrawRJWButtons(Listing_Standard listing, Pawn pawn)
        {
            if (!rjwLoaded) return;
            try
            {
                // 1. 获取LewdParts实例 - 确保传递正确参数
                object pawnParts = getLewdPartsMethod.Invoke(null, new object[] { pawn });
                if (pawnParts == null)
                {
                    Log.Error("GetLewdParts 返回 null");
                    return;
                }

                // 2. 调用PopulateAll - 确保传递正确参数（无参数）
                populateAllMethod.Invoke(pawnParts, null);

                // 3. 获取身体部位属性
                PropertyInfo breastsProp = lewdPartsType.GetProperty("Breasts");
                PropertyInfo penisesProp = lewdPartsType.GetProperty("Penises");
                PropertyInfo vaginasProp = lewdPartsType.GetProperty("Vaginas");
                PropertyInfo anusesProp = lewdPartsType.GetProperty("Anuses");

                if (breastsProp == null || penisesProp == null || vaginasProp == null || anusesProp == null)
                {
                    Log.Error("无法找到身体部位属性");
                    return;
                }

                // 4. 获取身体部位值
                object breasts = breastsProp.GetValue(pawnParts);
                object penises = penisesProp.GetValue(pawnParts);
                object vaginas = vaginasProp.GetValue(pawnParts);
                object anuses = anusesProp.GetValue(pawnParts);

                if (asSexPartHediffsMethod == null)
                {
                    Log.Error("无法找到asSexPartHediffs");
                    return;
                }

                System.Collections.IList breastsList = asSexPartHediffsMethod.Invoke(null, new object[] { breasts }) as System.Collections.IList;
                System.Collections.IList penisesList = asSexPartHediffsMethod.Invoke(null, new object[] { penises }) as System.Collections.IList;
                System.Collections.IList vaginasList = asSexPartHediffsMethod.Invoke(null, new object[] { vaginas }) as System.Collections.IList;
                System.Collections.IList anusesList = asSexPartHediffsMethod.Invoke(null, new object[] { anuses }) as System.Collections.IList;

                if (breastsList == null)
                {
                    Log.Error("Breasts are null!");
                    return;
                }

                // 6. 创建按钮
                Rect rect1 = listing.GetRect(24f).ContractedBy(4f, 0);
                Rect rect2 = listing.GetRect(24f).ContractedBy(4f, 0);
                Rect rect3 = listing.GetRect(24f).ContractedBy(4f, 0);
                Rect rect4 = listing.GetRect(24f).ContractedBy(4f, 0);

                MakeButton(rect1, pawn, breastsList, HediffDef.Named("Breasts").LabelCap);
                MakeButton(rect2, pawn, penisesList, HediffDef.Named("Penis").LabelCap);
                MakeButton(rect3, pawn, vaginasList, HediffDef.Named("Vagina").LabelCap);
                MakeButton(rect4, pawn, anusesList, HediffDef.Named("Anus").LabelCap);
            }
            catch (Exception ex)
            {
                Log.Error($"绘制RJW按钮失败: {ex}");
            }
        }
        private static void MakeButton(Rect rect, Pawn pawn, System.Collections.IList parts, string label)
        {
            if (parts == null) 
            {
                Log.Warning(label + "is null!");
                return;
            }

            try
            {
                // 过滤可调整大小的部位
                Type listType = typeof(List<>).MakeGenericType(sexPartHediffType);
                IList filteredParts = (IList)Activator.CreateInstance(listType);
                foreach (var part in parts)
                {
                    filteredParts.Add(part);
                }

                if (filteredParts.Count == 0)
                {
                    // 部位添加逻辑
                    var partMap = new Dictionary<string, (string defName, Func<Pawn, BodyPartRecord> getBprFunc)>
                    {
                        { HediffDef.Named("Breasts").LabelCap, ("Breasts", p => (BodyPartRecord)getBreastsBPRMethod.Invoke(null, new object[] { p })) },
                        { HediffDef.Named("Penis").LabelCap, ("Penis", p => (BodyPartRecord)getGenitalsBPRMethod.Invoke(null, new object[] { p })) },
                        { HediffDef.Named("Vagina").LabelCap, ("Vagina", p => (BodyPartRecord)getGenitalsBPRMethod.Invoke(null, new object[] { p })) },
                        { HediffDef.Named("Anus").LabelCap, ("Anus", p => (BodyPartRecord)getAnusBPRMethod.Invoke(null, new object[] { p })) }
                    };

                    if (Widgets.ButtonText(rect, "PartsAddLabel".Translate(label), true, true, true, null))
                    {
                        if (partMap.TryGetValue(label, out var partInfo))
                        {
                            BodyPartRecord partBPR = partInfo.getBprFunc(pawn);
                            HediffDef hediffDef = HediffDef.Named(partInfo.defName);

                            // 创建新的身体部位
                            object newPart = makePartMethod.Invoke(null, new object[] { hediffDef, pawn, partBPR });
                            pawn.health.AddHediff((Hediff)newPart, partBPR);
                        }
                    }
                }
                else if (Widgets.ButtonText(rect, "PartsEditLabel".Translate(label), true, true, true, null))
                {
                    // 创建编辑对话框
                    object dialog = Activator.CreateInstance(dialogPartcardType, new object[] { pawn, filteredParts, label });
                    Find.WindowStack.Add((Window)dialog);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"创建RJW按钮失败: {ex}");
            }
        }
    }
}
