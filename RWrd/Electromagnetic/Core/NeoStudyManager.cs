using Electromagnetic.Things;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Electromagnetic.Core
{
    public sealed class NeoStudyManager : IExposable
    {
        //提高学习进度
        public void SetStudied(ThingDef thingDef, Pawn pawn, float amount)
        {
            bool flag = !this.studyProgress.ContainsKey(thingDef);
            if (flag)
            {
                this.studyProgress[thingDef] = new Dictionary<Pawn, float>();
                bool flag2 = !this.studyProgress[(ThingDef)thingDef].ContainsKey(pawn);
                if (flag2)
                {
                    this.studyProgress[thingDef][pawn] = amount;
                }
            }
            else
            {
                bool flag2 = !this.studyProgress[(ThingDef)thingDef].ContainsKey(pawn);
                if (flag2)
                {
                    this.studyProgress[thingDef][pawn] = amount;
                }
            }
            Dictionary<ThingDef, Dictionary<Pawn, float>> dictionary = this.studyProgress;
            dictionary[thingDef][pawn] += amount / (float)thingDef.GetCompProperties<CompProperties_ThingStudiable>().cost;
            this.studyProgress[thingDef][pawn] = Mathf.Clamp01(this.studyProgress[thingDef][pawn]);
        }
        //设定学习进度
        public void ForceSetStudiedProgress(ThingDef thingDef, Pawn pawn, float progress)
        {
            bool flag = !this.studyProgress.ContainsKey(thingDef);
            if (flag)
            {
                this.studyProgress[thingDef] = new Dictionary<Pawn, float>();
                bool flag2 = !this.studyProgress[(ThingDef)thingDef].ContainsKey(pawn);
                if (flag2)
                {
                    this.studyProgress[thingDef][pawn] = progress;
                }
            }
            else
            {
                bool flag2 = !this.studyProgress[(ThingDef)thingDef].ContainsKey(pawn);
                if (flag2)
                {
                    this.studyProgress[thingDef][pawn] = progress;
                }
            }
            this.studyProgress[thingDef][pawn] = Mathf.Clamp01(progress);
        }
        //判断是否学习完成
        public bool StudyComplete(ThingDef thingDef, Pawn pawn)
        {
            bool flag = thingDef.GetCompProperties<CompProperties_Studiable>() == null;
            bool result;
            if (flag)
            {
                result = false;
            }
            else
            {
                bool flag2 = this.studyProgress.ContainsKey(thingDef);
                if (flag2)
                {
                    bool flag3 = this.studyProgress[(ThingDef)thingDef].ContainsKey(pawn);
                    result = (flag3 && this.studyProgress[thingDef][pawn] >= 1f);
                }
                else
                {
                    result = false;
                }
            }
            return result;
        }
        //获取学习进度
        public float GetStudyProgress(ThingDef thingDef, Pawn pawn)
        {
            bool flag = !this.studyProgress.ContainsKey(thingDef);
            float result;
            if (flag)
            {
                result = 0f;
            }
            else
            {
                bool flag2 = !this.studyProgress[(ThingDef)thingDef].ContainsKey(pawn);
                if (flag2)
                {
                    result = 0f;
                }
                else
                {
                    result = this.studyProgress[thingDef][pawn];
                }
            }
            return result;
        }
        //重置全部进度
        public void ResetAllProgress()
        {
            this.studyProgress.Clear();
        }
        public void ExposeData()
        {
            Scribe_Collections.Look<ThingDef, Dictionary<Pawn, float>>(ref this.studyProgress, "studyProgress", LookMode.Def, LookMode.Value);
        }
        private Dictionary<ThingDef, Dictionary<Pawn, float>> studyProgress = new Dictionary<ThingDef, Dictionary<Pawn, float>>();
    }
}
