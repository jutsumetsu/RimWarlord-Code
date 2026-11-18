using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Electromagnetic.Compatibility
{
    internal class LegacyTypeCompatibility
    {
        private static bool installed;

        private static readonly Dictionary<string, string> Renamed =
            new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "Electromagnetic.GameComponent_ExpPoolManager", "Electromagnetic.GameComponents.GameComponent_ExpPoolManager" },
            { "Electromagnetic.MapComponent_ExpPool", "Electromagnetic.GameComponents.MapComponent_ExpPool" },
            { "Electromagnetic.GameComponent.GameComponent_RWrdDirector", "Electromagnetic.GameComponents.GameComponent_RWrdDirector" },
            { "Electromagnetic.GameComponent.GameComponent_RWrdVassal", "Electromagnetic.GameComponents.GameComponent_RWrdVassal" },
            { "Electromagnetic.Quest.QuestPart_BaakOfferSettlement", "Electromagnetic.Quests.QuestPart_BaakOfferSettlement" },
            { "Electromagnetic.Quest.QuestPart_BaakSpawnTarget", "Electromagnetic.Quests.QuestPart_BaakSpawnTarget" }
        };

        public static void Install(Harmony harmony)
        {
            if (installed) return;

            var target = AccessTools.Method(
                typeof(BackCompatibility),
                nameof(BackCompatibility.GetBackCompatibleType));

            if (target == null)
            {
                Log.Warning("[RimWarlord] BackCompatibility.GetBackCompatibleType not found; legacy save compatibility disabled.");
                return;
            }

            var prefix = AccessTools.Method(typeof(LegacyTypeCompatibility), nameof(Prefix));
            harmony.Patch(target, prefix: new HarmonyMethod(prefix));
            installed = true;
            Log.Message("[RimWarlord] Legacy type compatibility installed.");
        }

        private static bool Prefix(Type baseType, string providedClassName, ref Type __result)
        {
            if (providedClassName.NullOrEmpty()) return true;

            string newName;
            if (!Renamed.TryGetValue(providedClassName, out newName)) return true;

            Type mapped = AccessTools.TypeByName(newName);
            if (mapped == null || (baseType != null && !baseType.IsAssignableFrom(mapped)))
            {
                Log.Warning("[RimWarlord] Legacy type " + providedClassName + " mapped to " + newName + " but the target is missing or not assignable.");
                return true;
            }

            __result = mapped;
            return false;
        }
    }
}
