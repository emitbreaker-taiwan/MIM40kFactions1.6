using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class CompProperties_AbilitySpawnPawn : CompProperties_AbilityEffect
    {
        public PawnKindDef pawnKind;
        public int amount = 1;
        public FactionDef forcedFaction;
        public bool sendSkipSignal = true;
        public int secondsToDisappear = 0;
        public int ticksToDisappear = 0;
        public bool setKillSwitch = true;
        public bool destroywhenKills = true;

        // props for mental state
        public MentalStateDef stateDef;
        public MentalStateDef stateDefForMechs;
        public StatDef durationMultiplier;
        public EffecterDef casterEffect;
        public EffecterDef targetEffect;
        public bool forced = true;

        public CompProperties_AbilitySpawnPawn() => compClass = typeof(CompAbilityEffect_SpawnPawn);
    }
}
