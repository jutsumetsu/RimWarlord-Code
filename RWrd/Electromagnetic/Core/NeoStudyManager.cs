using Electromagnetic.Things;
using RimWorld;
using System;
using System.Collections.Generic;
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
        public void SetStudied(ThingDef thingDef, float amount)
        {
            bool flag = !this.studyProgress.ContainsKey(thingDef);
            if (flag)
            {
                this.studyProgress.Add(thingDef, 0f);
            }
            Dictionary<ThingDef, float> dictionary = this.studyProgress;
            dictionary[thingDef] += amount / (float)thingDef.GetCompProperties<CompProperties_ThingStudiable>().cost;
            this.studyProgress[thingDef] = Mathf.Clamp01(this.studyProgress[thingDef]);
        }
        //设定学习进度
        public void ForceSetStudiedProgress(ThingDef thingDef, float progress)
        {
            bool flag = !this.studyProgress.ContainsKey(thingDef);
            if (flag)
            {
                this.studyProgress.Add(thingDef, 0f);
            }
            this.studyProgress[thingDef] = Mathf.Clamp01(progress);
        }
        //判断是否学习完成
        public bool StudyComplete(ThingDef thingDef)
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
                result = (flag2 && this.studyProgress[thingDef] >= 1f);
            }
            return result;
        }
        //获取学习进度
        public float GetStudyProgress(ThingDef thingDef)
        {
            bool flag = !this.studyProgress.ContainsKey(thingDef);
            float result;
            if (flag)
            {
                result = 0f;
            }
            else
            {
                result = this.studyProgress[thingDef];
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
            Scribe_Collections.Look<ThingDef, float>(ref this.studyProgress, "studyProgress", LookMode.Def, LookMode.Value);
        }
        private Dictionary<ThingDef, float> studyProgress = new Dictionary<ThingDef, float>();
    }
}
