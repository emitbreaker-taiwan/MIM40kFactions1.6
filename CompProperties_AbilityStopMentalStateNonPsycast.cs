using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;

namespace MIM40kFactions
{

    public class CompProperties_AbilityStopMentalStateNonPsycast : CompProperties_AbilityEffect
    {
        public List<MentalStateDef> exceptions;

        public bool canApplyToHostile = false;
        public bool applyToSelf = false;
        public bool onlyApplyToSelf;

        public CompProperties_AbilityStopMentalStateNonPsycast()
        {
            this.compClass = typeof(CompAbilityEffect_StopMentalStateNonPsycast);
        }

    }
}
