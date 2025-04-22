using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class CompProperties_AbilityGiveMentalStateAICanTarget : CompProperties_AbilityEffect
    {
        public MentalStateDef stateDef;
        public MentalStateDef stateDefForMechs;
        public StatDef durationMultiplier;
        public bool applyToSelf;
        public EffecterDef casterEffect;
        public EffecterDef targetEffect;
        public bool excludeNPCFactions;
        public bool forced;
        public float range = 1f;
    }
}
