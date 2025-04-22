using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions
{
    public class SanitizerHistory
    {
        public HashSet<NeedDef> removedNeeds = new HashSet<NeedDef>();
        public HashSet<HediffDef> removedHediffs = new HashSet<HediffDef>();
        public HashSet<ThoughtDef> removedThoughts = new HashSet<ThoughtDef>();
        public HashSet<InspirationDef> blockedInspirations = new HashSet<InspirationDef>();
        public HashSet<MentalStateDef> blockedMentalStates = new HashSet<MentalStateDef>();
        public HashSet<string> sources = new HashSet<string>(); // ✅ tracks "Apparel:XXX", "Hediff:YYY", etc.
    }
}
