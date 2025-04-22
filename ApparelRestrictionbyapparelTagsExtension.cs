using Verse;
using System.Collections.Generic;
using RimWorld;

namespace MIM40kFactions
{
    public class ApparelRestrictionbyapparelTagsExtension : DefModExtension
    {
        [NoTranslate]
        public List<string> requiredapparelTags;
        [NoTranslate]
        public List<HediffDef> requiredHediffDefs;
        [NoTranslate]
        public string errorMessageAlt;
        [NoTranslate]
        public string errorMessageHediffAlt;
    }
}