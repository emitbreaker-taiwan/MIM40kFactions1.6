using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class CompProperties_CauseHediff_ApparelbyHediff : CompProperties
    {
        public HediffDef hediff;

        public BodyPartDef part;

        public List<HediffDef> hediffRequired;

        public List<HediffDef> hediffIfNotExists;

        public CompProperties_CauseHediff_ApparelbyHediff()
        {
            compClass = typeof(CompCauseHediff_ApparelbyHediff);
        }
    }
}