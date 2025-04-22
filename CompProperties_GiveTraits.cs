using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class CompProperties_GiveTraits : CompProperties
    {
        public List<TraitDegreeSet> traitDefSets;

        public CompProperties_GiveTraits() => compClass = typeof(CompGiveTraits);
    }

    public class TraitDegreeSet
    {
        public TraitDef traitDef;
        public int? degree;
    }
}
