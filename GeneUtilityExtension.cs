using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class GeneUtilityExtension : DefModExtension
    {
        public List<StartingHediff> startingHediffDefs;
        public List<HediffDef> randomHediffDefs;
        public float? severityRandomHediffDefs;
        public List<HediffDef> excludeifHediffs;
        public List<HedifftoBodyParts> hediffDefstoBodyParts;
    }

    public class HedifftoBodyParts
    {
        public HediffDef def;

        public float? severity;

        public float? chance;

        public IntRange? durationTicksRange;

        public List<BodyPartDef> parts;

    }
}