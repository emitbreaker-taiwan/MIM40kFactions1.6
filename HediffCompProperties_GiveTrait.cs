using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MIM40kFactions
{
    public class HediffCompProperties_GiveTrait : HediffCompProperties
    {
        public TraitDef traitDef;
        public int? degree;
        public float? severityAmount = -1f;

        public HediffCompProperties_GiveTrait() => compClass = typeof(HediffComp_GiveTrait);
    }
}
