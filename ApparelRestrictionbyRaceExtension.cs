using Verse;
using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace MIM40kFactions
{
    public class ApparelRestrictionbyRaceExtension : DefModExtension
    {
        public List<ThingDef> allowedRaces = new List<ThingDef>();
    }
}
