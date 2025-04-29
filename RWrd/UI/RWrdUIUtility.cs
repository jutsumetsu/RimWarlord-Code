using System;
using System.Collections.Generic;
using System.Linq;
using Electromagnetic.Core;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Electromagnetic.UI
{
    [StaticConstructorOnStartup]
	public static class RWrdUIUtility
    {
        static RWrdUIUtility()
        {

        }
        public static void LabelWithIcon(this Listing_Standard listing, Texture2D icon, string label)
        {
            float num = Text.CalcHeight(label, listing.ColumnWidth);
            Rect rect = listing.GetRect(num, 1f);
            float pixels = (float)icon.width * (num / (float)icon.height);
            GUI.DrawTexture(rect.TakeLeftPart(pixels), icon);
            rect.xMin += 3f;
            Widgets.Label(rect, label);
            listing.Gap(3f);
        }
        public static void StatDisplay(this Listing_Standard listing, Texture2D icon, StatDef stat, Thing thing)
        {
            listing.LabelWithIcon(icon, stat.LabelCap + ": " + stat.Worker.GetStatDrawEntryLabel(stat, thing.GetStatValue(stat, true, -1), stat.toStringNumberSense, StatRequest.For(thing), true));
        }
        public static Rect CenterRect(this Rect rect, Vector2 size)
        {
            return new Rect(rect.center - size / 2f, size);
        }

        private static bool EnsureInit()
        {
            bool flag = RWrdUIUtility.Hediff != null && RWrdUIUtility.CompAbilities != null;
            bool result;
            if (flag)
            {
                result = true;
            }
            else
            {
                Log.Error("[RWrd] RWrdUIUtility was used without being initialized.");
                result = false;
            }
            return result;
        }
        public static void DoPathAbilities(Rect inRect, RWrd_RouteDef path, Dictionary<AbilityDef, Vector2> abilityPos, Action<Rect, AbilityDef> doAbility)
        {
            bool flag = !RWrdUIUtility.EnsureInit();
            if (!flag)
            {
                // 使用具名的委托变量来替代 <>9__0
                Func<AbilityDef, bool> predicate = (AbilityDef abilityDef) => abilityPos.ContainsKey(abilityDef);

                foreach (AbilityDef abilityDef4 in path.AllAbilities)
                {
                    
                    List<AbilityDef> list = (abilityExtension_Psycast != null) ? abilityExtension_Psycast.prerequisites : null;
                    bool flag2 = list != null && abilityPos.ContainsKey(abilityDef4);
                    if (flag2)
                    {
                        IEnumerable<AbilityDef> source = list;

                        foreach (AbilityDef abilityDef2 in source.Where(predicate))
                        {
                            Widgets.DrawLine(abilityPos[abilityDef4], abilityPos[abilityDef2], path.HasAbility(abilityDef2) ? Color.white : Color.grey, 2f);
                        }
                    }
                }

                for (int i = 0; i < path.routeNodes..Length; i++)
                {
                    Rect rect = new Rect(inRect.x, inRect.y + (float)(path.MaxLevel - 1 - i) * inRect.height / (float)path.MaxLevel + 10f, inRect.width, inRect.height / 5f);
                    VFECore.Abilities.AbilityDef[] array = path.abilityLevelsInOrder[i];
                    for (int j = 0; j < array.Length; j++)
                    {
                        Rect arg = new Rect(rect.x + rect.width / 2f + PsycastsUIUtility.abilityTreeXOffsets[array.Length - 1][j], rect.y, 36f, 36f);
                        VFECore.Abilities.AbilityDef abilityDef3 = array[j];
                        bool flag3 = abilityDef3 == PowerfulpersonPathDef.Blank;
                        if (!flag3)
                        {
                            abilityPos[abilityDef3] = arg.center;
                            doAbility(arg, abilityDef3);
                        }
                    }
                }
            }
        }
        public static void DrawAbility(Rect inRect, AbilityDef ability)
        {
            Color color = Mouse.IsOver(inRect) ? GenUI.MouseoverColor : Color.white;
            MouseoverSounds.DoRegion(inRect, SoundDefOf.Mouseover_Command);
            GUI.color = color;
            GUI.DrawTexture(inRect, Command.BGTexShrunk);
            GUI.color = Color.white;
            GUI.DrawTexture(inRect, ability.icon);
        }
        private static readonly float[][] abilityTreeXOffsets = new float[][]
        {
            new float[]
            {
                -18f
            },
            new float[]
            {
                -47f,
                11f
            },
            new float[]
            {
                -69f,
                -18f,
                33f
            }
        };

        public static Hediff_RWrd_PowerRoot Hediff;

        // Token: 0x04000178 RID: 376
        public static CompAbilities CompAbilities;
    }
}
