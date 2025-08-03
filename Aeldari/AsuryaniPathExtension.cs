using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace MIM40kFactions.Aeldari
{
    public class AsuryaniPathExtension : DefModExtension
    {
        public float scoreToDisciplined = 0.4f;
        public float scoreToConsumed = 0.8f;
        public float scoreToExarch = 0.9f;

        public ThoughtDef noviceLeftThoughtDef;
        public ThoughtDef disciplinedLeftThoughtDef;
        public ThoughtDef consumedLeftThoughtDef;

        public MentalStateDef defaultMentalStateFailed;
        [MayRequireAnomaly]
        public MentalStateDef defaultMentalStateFailedAnomaly;

        public AsuryaniPathDef outcastPathDef;
        public AsuryaniPathDef damnationPathDef;

        public float outcastPathChanceOnEarlyExit = 0.3f;
        public float damnationPathChanceOnEarlyExit = 0.5f;

        public bool debugMode = false;
    }
}
