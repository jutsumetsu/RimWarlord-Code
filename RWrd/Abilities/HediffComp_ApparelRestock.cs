using Electromagnetic.Core;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Electromagnetic.Abilities
{
    public class HediffCompProperties_ApparelRestock : HediffCompProperties
    {
        public HediffCompProperties_ApparelRestock()
        {
            compClass = typeof(HediffComp_ApparelRestock);
        }
    }

    public class ApparelRecord : IExposable
    {
        public ThingDef def;
        public ThingDef stuff;
        public bool hasColor;
        public float colorR, colorG, colorB, colorA;
        public bool hasQuality;
        public QualityCategory quality;

        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Defs.Look(ref stuff, "stuff");
            Scribe_Values.Look(ref hasColor, "hasColor", false);
            Scribe_Values.Look(ref colorR, "colorR", 1f);
            Scribe_Values.Look(ref colorG, "colorG", 1f);
            Scribe_Values.Look(ref colorB, "colorB", 1f);
            Scribe_Values.Look(ref colorA, "colorA", 1f);
            Scribe_Values.Look(ref hasQuality, "hasQuality", false);
            Scribe_Values.Look(ref quality, "quality", QualityCategory.Normal);
        }
    }

    public class HediffComp_ApparelRestock : HediffComp
    {
        public const int MaxPending = 12;
        public List<ApparelRecord> pending = new List<ApparelRecord>();

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Collections.Look(ref pending, "pendingApparel", LookMode.Deep);

            if (pending == null) pending = new List<ApparelRecord>();

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                pending.RemoveAll(r => r == null || r.def == null);
                if (pending.Count > MaxPending) pending.RemoveRange(0, pending.Count - MaxPending);
            }
        }

        public void TryRecord(Apparel apparel)
        {
            if (apparel == null || !apparel.Destroyed) return;
            if (!ApparelRestockUtility.IsCraftableApparel(apparel.def)) return;
            if (pending.Count >= MaxPending) pending.RemoveAt(0);
            ApparelRecord record = new ApparelRecord
            {
                def = apparel.def,
                stuff = apparel.Stuff
            };
            CompColorable color = apparel.TryGetComp<CompColorable>();
            if (color != null && color.Active)
            {
                Color c = color.Color;
                record.hasColor = true;
                record.colorR = c.r; record.colorG = c.g;
                record.colorB = c.b; record.colorA = c.a;
            }
            CompQuality quality = apparel.TryGetComp<CompQuality>();
            if (quality != null)
            {
                record.hasQuality = true;
                record.quality = quality.Quality;
            }
            pending.Add(record);
        }
        /// <summary>
        /// 付得起吗
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="atomSplit"></param>
        /// <returns></returns>
        public bool HasAffordable(Pawn pawn, RWrd_AbilityBase atomSplit)
        {
            Hediff_RWrd_PowerRoot root = parent.pawn.GetPowerRoot();
            if (root == null || atomSplit == null) return false;
            foreach (ApparelRecord r in pending)
            {
                if (r == null || r.def == null) continue;
                if (!ApparelUtility.HasPartsToWear(pawn, r.def)) continue;
                if (root.energy.energy >= ApparelRestockUtility.Cost(r.def, atomSplit)) return true;
            }
            return false;
        }
        public void RestockAll(Pawn pawn, RWrd_AbilityBase atomSplit)
        {
            if (pawn == null || atomSplit == null || parent.pawn != pawn) return;
            Hediff_RWrd_PowerRoot root = pawn.GetPowerRoot();
            if (root == null) return;

            for (int i = pending.Count - 1; i >= 0; i--)
            {
                ApparelRecord r = pending[i];
                if (r == null || r.def == null) { pending.RemoveAt(i); continue; }
                if (!ApparelUtility.HasPartsToWear(pawn, r.def)) continue;

                float cost = ApparelRestockUtility.Cost(r.def, atomSplit);
                if (cost <= 0f || root.energy.energy < cost) continue;

                ThingDef stuff = r.stuff;
                if (stuff == null && r.def.MadeFromStuff) stuff = GenStuff.DefaultStuffFor(r.def);
                Apparel apparel = ThingMaker.MakeThing(r.def, stuff) as Apparel;
                if (apparel == null) continue;

                if (r.hasQuality)
                {
                    CompQuality q = apparel.TryGetComp<CompQuality>();
                    if (q != null) q.SetQuality(r.quality, null);
                }
                if (r.hasColor)
                {
                    CompColorable c = apparel.TryGetComp<CompColorable>();
                    if (c != null) c.SetColor(new Color(r.colorR, r.colorG, r.colorB, r.colorA));
                }

                pawn.apparel.Wear(apparel, false);
                if (!pawn.apparel.WornApparel.Contains(apparel))
                {
                    apparel.Destroy(DestroyMode.Vanish);
                    continue;
                }

                root.energy.SetEnergy(-cost);
                root.energy.SetExp(0.1f * -cost);
                atomSplit.SetMastery(0.05f);
                pending.RemoveAt(i);
            }
        }
    }
}
