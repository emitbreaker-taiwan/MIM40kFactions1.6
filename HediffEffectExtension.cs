using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class HediffEffectExtension : DefModExtension
    {
        //For Hediff_DeathRefusalRNG Usage
        public string selfresurrectionDefaultLabel;
        public string selfresurrectionDefaultDesc;
        public string selfresurrectionMessage;
        public string messageUsingSelfResurrection;
        public string messageUsingSelfResurrectionFailed;
        public int resurrectionChance;
        public EffecterDef resurrectAvailableEffecter;
        public SoundDef resurrectSustainer;
        public EffecterDef resurrectUsedEffecter;
        public bool selfresurrectionSideEffect = true;
        public HediffDef selfresurrectionSideEffectDef;

        //For BoostbyHealth Usage
        public HediffDef positiveHediffDef;
        public HediffDef negativeHediffDef;
    }
}
