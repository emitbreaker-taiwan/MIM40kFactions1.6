using Verse;
using System.Collections.Generic;
using RimWorld;


namespace MIM40kFactions
{
    public class ApparelRestrictionbyHediffExtension : DefModExtension
    {
        [NoTranslate]
        public List<HediffDef> requiredHediffDefs;
        [NoTranslate]
        public string errorMessageAlt;
    }
}