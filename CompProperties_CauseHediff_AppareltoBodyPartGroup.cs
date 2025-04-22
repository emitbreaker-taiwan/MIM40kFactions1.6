using Verse;
using System.Collections.Generic;
using UnityEngine;
using RimWorld;

namespace MIM40kFactions
{
    public class CompProperties_CauseHediff_AppareltoBodyPartGroup : CompProperties
    {
        public HediffDef hediff;
        public List<HediffDef> hediffDefs;
        public List<BodyPartDef> parts;

        public CompProperties_CauseHediff_AppareltoBodyPartGroup()
        {
            compClass = typeof(CompCauseHediff_AppareltoBodyPartGroup);
        }
    }
}