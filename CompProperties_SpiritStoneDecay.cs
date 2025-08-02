using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions.Aeldari
{
    public class CompProperties_SpiritStoneDecay : CompProperties
    {
        public int decayDurationTicks = 120000;
        public ThoughtDef soulLostThought = null;
        public List<ThingDef> aeldariRaces = new List<ThingDef>();
        public List<ThingDef> properStorageDefs = new List<ThingDef>();
        public bool debugMode = false;

        public CompProperties_SpiritStoneDecay()
        {
            compClass = typeof(CompSpiritStoneDecay);
        }
    }
}
