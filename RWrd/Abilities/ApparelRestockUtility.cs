using Electromagnetic.Core;
using Electromagnetic.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Electromagnetic.Abilities
{
    public static class ApparelRestockUtility
    {
        private static HashSet<ThingDef> craftableApparel;

        public static HashSet<ThingDef> CraftableApparel
        {
            get
            {
                if (craftableApparel == null)
                {
                    List<ThingDef> source = PowerRootUtillity.spawnableThings;
                    if (source == null) source = Dialog_SelectThings.AllThingsDefs.ToList();
                    craftableApparel = new HashSet<ThingDef>(source.Where(d => d != null && d.IsApparel));
                }
                return craftableApparel;
            }
        }
        /// <summary>
        /// 是可制造的衣物吗
        /// </summary>
        /// <param name="def"></param>
        /// <returns></returns>
        public static bool IsCraftableApparel(ThingDef def)
        {
            return def != null && def.IsApparel && CraftableApparel.Contains(def);
        }

        public static float Cost(ThingDef def, RWrd_AbilityBase atomSplit)
        {
            float offset = atomSplit != null ? atomSplit.mastery / 100f * 0.5f : 0f;
            return Dialog_SelectThings.CalculateValueToShow(def, offset);
        }
    }
}
