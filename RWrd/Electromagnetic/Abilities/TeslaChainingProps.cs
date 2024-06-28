using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Electromagnetic.Abilities
{
    public class TeslaChainingProps : DefModExtension
    {
        public bool addFire;
        public float bounceRange;
        public int maxBounceCount;
        public DamageDef damageDef;
        public DamageDef explosionDamageDef;
        public float impactRadius;
        public bool targetFriendly;
        public int maxLifetime;
        public SoundDef impactSound;
    }
}
