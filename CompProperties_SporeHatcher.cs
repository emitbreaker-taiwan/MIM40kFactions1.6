using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using RimWorld;
using Verse;

namespace MIM40kFactions.Orks
{
    public class CompProperties_SporeHatcher : CompProperties
    {
        public float hatcherDaystoHatch = 1f;

        public ThingDef okoidfungusDef;

        public PawnKindDef hatcherPawnDef;

        public List<ThingDef> squigRaceThingDefs;

        public List<PawnKindDef> squigPawnKindDefs;

        public List<ThingDef> grotRaceThingDefs;

        public List<PawnKindDef> grotPawnKindDefs;

        public List<ThingDef> orkRaceThingDefs;

        public List<PawnKindDef> orkPawnKindDefs;

        public ThingDef filthDef;

        public bool allowRelationship = false;

        public bool enableDebug = false;

        public CompProperties_SporeHatcher()
        {
            compClass = typeof(CompSporeHatcher);
        }
    }
}
