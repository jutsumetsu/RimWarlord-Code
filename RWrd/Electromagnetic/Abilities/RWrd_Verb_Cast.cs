using System;
using System.Collections.Generic;
using Electromagnetic.Core;
using RimWorld;
using UnityEngine;
using Verse;

namespace Electromagnetic.Abilities
{
    public class RWrd_Verb_Cast : Verb_CastAbility
    {
        public RWrd_PsycastOrigin Psycast
        {
            get
            {
                return this.ability as RWrd_PsycastOrigin;
            }
        }
        private const float StatLabelOffsetY = 1f;
        private static float MoteCastFadeTime = 0.4f;
        private float timeSet = Time.time;
        private float timePast;
        private static float MoteCastScale = 1f;
        private static Vector3 MoteCastOffset = new Vector3(0f, 0f, 0.87f);
    }
}
