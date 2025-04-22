using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class HediffCompProperties_GiveGenes : HediffCompProperties
    {
        [MayRequireBiotech]
        public List<GeneDef> geneDefs;

        [MayRequireBiotech]
        public List<GiveGenesSet> givegenesSets;

        public HediffCompProperties_GiveGenes() => compClass = typeof(HediffComp_GiveGenes);
    }

    public class GiveGenesSet
    {
        [MayRequireBiotech]
        public GeneDef geneDef;
        public float? severity;
    }
}
