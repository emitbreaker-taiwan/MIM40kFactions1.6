using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    
    public class HediffCompProperties_GiveHediffsInRangeToEnemyOnly : HediffCompProperties
    {
        public float range;
        public HediffDef hediff;
        public ThingDef mote;
        public TargetingParameters targetingParameters;
        public bool hideMoteWhenNotDrafted;
        public bool onlyTargetHostileFactions;
        public bool onlyTargetNonPlayerFactions = true;
        public float initialSeverity = 1f;

        public HediffCompProperties_GiveHediffsInRangeToEnemyOnly()
        {
            this.compClass = typeof(HediffComp_GiveHediffsInRangeToEnemyOnly);
        }
    }
}
