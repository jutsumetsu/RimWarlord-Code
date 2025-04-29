using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Electromagnetic.HarmonyPatchs;
using Electromagnetic.Setting;
using HarmonyLib;
using static Electromagnetic.HarmonyPatchs.HarmonyInit;

namespace Electromagnetic.Effect
{
    public class FragmentsWorker
    {
        public static bool IsActive()
        {
            return RWrdSettings.PowerfulPersonFragments;
        }

        public void Start()
        {
            if (this.patchedMethods == null)
            {
                this.patchedMethods = new Dictionary<MethodBase, List<MethodInfo>>();
            }
            bool flag = IsActive();
            if (flag)
            {
                bool flag2 = !this.patchedMethods.Any<KeyValuePair<MethodBase, List<MethodInfo>>>();
                if (flag2)
                {
                    this.DoPatches();
                }
                this.OnActivate();
            }
            else
            {
                this.Disactivate();
            }
        }

        public void Disactivate()
        {
            bool flag = this.patchedMethods.Any<KeyValuePair<MethodBase, List<MethodInfo>>>();
            if (flag)
            {
                this.OnDisactivate();
                foreach (KeyValuePair<MethodBase, List<MethodInfo>> keyValuePair in this.patchedMethods)
                {
                    MethodBase key = keyValuePair.Key;
                    foreach (MethodInfo patch in keyValuePair.Value)
                    {
                        PatchMain.instance.Unpatch(key, patch);
                    }
                }
            }
            this.patchedMethods.Clear();
        }

        public virtual void OnActivate()
        {
        }

        public virtual void OnDisactivate()
        {
        }

        public virtual void DoPatches()
        {
        }

        public void Patch(Type type, string methodName, MethodInfo prefix = null, MethodInfo postfix = null, MethodInfo transpiler = null)
        {
            this.Patch(AccessTools.Method(type, methodName, null, null), prefix, postfix, transpiler);
        }

        // Token: 0x0600010C RID: 268 RVA: 0x00006684 File Offset: 0x00004884
        public void Patch(MethodBase original, MethodInfo prefix = null, MethodInfo postfix = null, MethodInfo transpiler = null)
        {
            PatchMain.instance.Patch(original, (prefix != null) ? new HarmonyMethod(prefix) : null, (postfix != null) ? new HarmonyMethod(postfix) : null, (transpiler != null) ? new HarmonyMethod(transpiler) : null, null);
            List<MethodInfo> list = new List<MethodInfo>();
            bool flag = prefix != null;
            if (flag)
            {
                list.Add(prefix);
            }
            bool flag2 = postfix != null;
            if (flag2)
            {
                list.Add(postfix);
            }
            bool flag3 = transpiler != null;
            if (flag3)
            {
                list.Add(transpiler);
            }
            bool flag4 = this.patchedMethods.ContainsKey(original);
            if (flag4)
            {
                this.patchedMethods[original].AddRange(list);
            }
            else
            {
                this.patchedMethods[original] = list;
            }
        }


        protected Dictionary<MethodBase, List<MethodInfo>> patchedMethods = new Dictionary<MethodBase, List<MethodInfo>>();
    }
}
