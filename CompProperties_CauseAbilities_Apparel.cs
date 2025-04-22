using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class CompProperties_CauseAbilities_Apparel : CompProperties
    {
        public AbilityDef abilityDef;
        public List<AbilityDef> abilityDefs;
        public BodyPartDef part;

        public CompProperties_CauseAbilities_Apparel()
        {
            compClass = typeof(CompCauseAbilities_Apparel);
        }
    }
}