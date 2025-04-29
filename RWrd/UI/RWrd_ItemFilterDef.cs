using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Electromagnetic.UI
{
    public class RWrd_ItemFilterDef : Def
    {
        public List<ThingCategoryDef> categoryWhiteList;
        public List<ThingCategoryDef> categoryBlackList;
        public List<ThingDef> thingWhiteList;
        public List<ThingDef> thingBlackList;
    }
}
