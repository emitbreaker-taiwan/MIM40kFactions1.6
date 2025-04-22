using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace MIM40kFactions
{
    public class CompProperties_AbilityLivingLightning : CompProperties_AbilityEffect
    {
        public int initialStrikeDelayMin = 60;
        public int initialStrikeDelayMax = 180;

        public CompProperties_AbilityLivingLightning()
        {
            compClass = typeof(CompAbilityEffect_LivingLightning);
        }
    }
}
