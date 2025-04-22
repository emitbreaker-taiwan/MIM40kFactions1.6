using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MIM40kFactions
{
    public class HediffCompProperties_GiveHediffs : HediffCompProperties
    {
        public HediffOption hediffOption;
        public List<HediffOption> hediffOptions;
        public bool givetoAllbodyparts = false;
        public float? severityAmount = -1f;

        public HediffCompProperties_GiveHediffs() => compClass = typeof(HediffComp_GiveHediffs);    
    }
}
