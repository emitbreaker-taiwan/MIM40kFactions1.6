using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class AbilityRestrictionExtension : DefModExtension
    {
        [MayRequireBiotech]
        public List<GeneDef> requiredGenes;
        [MayRequireBiotech]
        public List<XenotypeDef> requiredXenotypes;
        public List<HediffDef> requiredHediffs;
    }
}
