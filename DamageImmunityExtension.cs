using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class DamageImmunityExtension : DefModExtension
    {
        public List<HediffDef> hediffs;
        public float severityAdjustment = 0.1f;
        public StatDef exposureStatFactor;
    }
}
