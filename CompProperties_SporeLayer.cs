using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class CompProperties_SporeLayer : CompProperties
    {
        public float sporeLayIntervalDays = 1f;

        public IntRange sporeCountRange = IntRange.one;

        public ThingDef sporeUnfertilizedDef;

        public ThingDef sporeFertilizedDef;

        public int sporeFertilizationCountMax = 1;

        public bool sporeLayFemaleOnly = false;

        public bool sporeLaySterile = true;

        public bool sporeLayShambler = true;

        public float sporeProgressUnfertilizedMax = 1f;

        public bool sporeProgressCanBeStoppedBecauseUnfertilized = false;

        public bool canHaveRelation = false;

        public bool requireFertilization = false;

        public bool enableDebug = false;

        public List<ThingDef> targetRaceDefstoCount;

        public CompProperties_SporeLayer()
        {
            compClass = typeof(CompSporeLayer);
        }
    }
}
