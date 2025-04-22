using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MIM40kFactions
{
    public class HediffCompProperties_Stun : HediffCompProperties
    {
        public float stunSeconds = -1f;
        public HediffCompProperties_Stun() => compClass = typeof(HediffComp_Stun);
    }
}
