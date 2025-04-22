using Verse;
using System.Collections.Generic;
using RimWorld;

namespace MIM40kFactions
{
    public class WeaponRestrictionExtension : DefModExtension
    {
        [MayRequireBiotech]
        public List<GeneDef> weaponcanEquipbyGenes = new List<GeneDef>();
        public List<ThingDef> allowedRaces;
        public List<TraitDef> requiredTraits;
        public List<HediffDef> requiredHediffDefs;
        [NoTranslate]
        public string errorMessageAlt;
    }
}
