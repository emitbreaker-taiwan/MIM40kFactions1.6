using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MIM40kFactions
{
    public class HediffCompProperties_GiveSecondaryTrait : HediffCompProperties
    {
        public TraitDef traitDef;
        public int? degree;
        public float? severityAmount = -1f;
        
        public HediffCompProperties_GiveSecondaryTrait() => compClass = typeof(HediffComp_GiveSecondaryTrait);
    }
}
