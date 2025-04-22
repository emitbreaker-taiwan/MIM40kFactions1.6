using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class CompProperties_AbilitySpawnThing : CompProperties_AbilityEffect
    {
        public ThingDef thingDef;
        public ThingDef stuffDef;
        public ThingStyleDef thingStyleDef = null;
        public int amount = 1;
        public bool canBeMinified = false;
        public FactionDef forcedFaction;
        public int stackCount = 1;
        public WipeMode? wipeMode = null;
        public bool direct = false;
        public int totalPowerTicks = 2500;

        public bool sendSkipSignal = true;
        public int secondsToDisappear = 0;
        public bool setKillSwitch = true;

        public CompProperties_AbilitySpawnThing() => compClass = typeof(CompAbilityEffect_SpawnThing);
    }
}