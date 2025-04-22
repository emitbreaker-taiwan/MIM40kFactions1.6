using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MIM40kFactions
{
    public class HediffCompProperties_GiveXenotype : HediffCompProperties
    {
        [MayRequireBiotech]
        public XenotypeDef targetxenotypeDef;
        [MayRequireBiotech]
        public List<XenotypeDef> targetxenotypeDefs;
        public float? severityAmount = -1f;
        public ThingDef targetRaceDef;
        public bool removeafterSetXenotype = false;

        public HediffCompProperties_GiveXenotype() => compClass = typeof(HediffComp_GiveXenotype);
    }
}
