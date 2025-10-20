using Electromagnetic.Core;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Electromagnetic.UI
{
    internal class Selected
    {
        internal static Selected ByThingDef(ThingDef _thingDef)
        {
            return new Selected(_thingDef);
        }
        private Selected(ThingDef t)
        {
            this.tempThing = null;
            this.Init(t, true, null, null);
        }
        internal void Init(ThingDef _thingDef, bool random = true, Thing thing = null, Action postProcess = null)
        {
            this.thingDef = _thingDef;
            int num = Tools.zufallswert.Next(100);
            this.gender = ((num > 50) ? Gender.Female : Gender.Male);
            num = Tools.zufallswert.Next(30);
            this.age = num;
            this.pkd = ((this.thingDef != null && this.thingDef.race != null) ? this.thingDef.race.AnyPawnKind : null);
            bool flag = thing != null;
            if (flag)
            {
                this.pkd = ((thing.def.race != null) ? ((thing.GetType() == typeof(Pawn)) ? ((Pawn)thing).kindDef : thing.def.race.AnyPawnKind) : null);
            }
            this.stuff = ((this.thingDef != null) ? this.thingDef.GetStuff(ref this.lOfStuff, ref this.stuffIndex, random) : null);
            this.DrawColor = ((this.stuff != null) ? this.thingDef.GetColor(this.stuff) : ((this.thingDef != null) ? this.thingDef.uiIconColor : Color.white));
            this.style = ((this.thingDef != null) ? this.thingDef.GetStyle(ref this.lOfStyle, ref this.styleIndex, random) : null);
            this.RandomQuality();
            bool flag2 = thing != null;
            if (flag2)
            {
                Dialog_​BodyRemold.tDefName = thing.def.defName;
                this.Set(thing.Stuff, thing.StyleDef, thing.DrawColor, thing.GetQuality(), thing.stackCount);
            }
            this.stuffIndex = ((this.stuff == null) ? 0 : this.stuffIndex);
            this.oldStuffIndex = ((this.stuff == null) ? 0 : this.stuffIndex);
            this.styleIndex = ((this.style == null) ? 0 : this.styleIndex);
            this.oldstyleIndex = ((this.style == null) ? 0 : this.styleIndex);
            this.buyPrice = 0;
            this.stackVal = ((thing != null) ? thing.stackCount : 1);
            this.oldStackVal = ((thing != null) ? thing.stackCount : 1);
            this.UpdateBuyPrice();
            bool flag3 = postProcess != null;
            if (flag3)
            {
                postProcess();
            }
        }
        internal void RandomQuality()
        {
            this.quality = (int)QualityUtility.AllQualityCategories.RandomElement<QualityCategory>();
        }
        private void Set(ThingDef _stuff, ThingStyleDef _style, Color col, int quali, int _stackVal)
        {
            this.stuff = _stuff;
            this.style = _style;
            this.stackVal = _stackVal;
            this.stuffIndex = ((this.stuff != null) ? this.lOfStuff.IndexOf(this.stuff) : 0);
            this.styleIndex = ((this.style != null) ? this.lOfStyle.IndexOf(this.style) : 0);
            this.quality = ((quali < 0) ? ((int)QualityUtility.AllQualityCategories.RandomElement<QualityCategory>()) : quali);
            this.DrawColor = col;
        }
        internal void UpdateBuyPrice()
        {
            bool flag = this.thingDef == null;
            if (!flag)
            {
                bool flag2 = this.thingDef.HasComp(typeof(CompQuality));
                double num;
                if (flag2)
                {
                    num = 0.333333333 + (double)this.quality * 0.333333333;
                    bool flag3 = this.quality == 6;
                    if (flag3)
                    {
                        num += 1.0;
                    }
                }
                else
                {
                    num = 1.0;
                }
                double num2 = (double)this.thingDef.GetStatValueAbstract(StatDefOf.MarketValue, this.stuff) * num;
                double num3 = num2 * (double)this.stackVal;
                bool flag4 = num3 < 1.0;
                if (flag4)
                {
                    this.buyPrice = 1;
                }
                else
                {
                    this.buyPrice = (int)Math.Round(num3);
                }
            }
        }
        internal Gender gender = Gender.None;
        internal int age = 1;
        internal PawnKindDef pkd = null;
        internal Thing tempThing;
        internal ThingDef thingDef;
        internal ThingDef stuff;
        internal ThingStyleDef style;
        internal HashSet<ThingDef> lOfStuff;
        internal int stuffIndex;
        internal Color DrawColor;
        internal HashSet<ThingStyleDef> lOfStyle;
        internal int styleIndex;
        internal int quality;
        internal int stackVal;
        internal int oldStuffIndex;
        internal int oldstyleIndex;
        internal int buyPrice;
        internal int oldStackVal;
    }
}
