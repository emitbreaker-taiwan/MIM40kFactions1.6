using RimWorld;
using Verse;
using System.Collections.Generic;

namespace MIM40kFactions
{
    public class CompProperties_AbilityGiveRandomHediffWithDuration : CompProperties_AbilityEffectWithDuration
    {
        public List<HediffOption> options;
        public bool allowDuplicates;

        public bool onlyBrain;
        public bool applyToSelf;
        public bool onlyApplyToSelf;
        public bool applyToTarget = true;
        public bool replaceExisting;
        public float severity = -1f;

        [MayRequireBiotech]
        public XenotypeDef excludeXenotype;
        [MayRequireBiotech]
        public List<XenotypeDef> excludeXenotypes;
        [MayRequireBiotech]
        public XenotypeDef targetXenotype;
        [MayRequireBiotech]
        public List<XenotypeDef> targetXenotypes;

        public HediffDef requiredHediff;
        public List<HediffDef> requiredHediffs;

        public bool onlyPawnsInSameFaction = false;
        public bool onlyTargetHostileFactions = false;
        public bool onlyTargetNonPlayerFactions = false;
        public bool onlyTargetNotinSameFactions = false;

        public bool useKeyword = false;
        public bool isVehicle = false;
        public bool isMonster = false;
        public bool isPsychic = false;
        public bool isPsyker = false;
        public bool isCharacter = false;
        public bool isAstartes = false;
        public bool isInfantry = false;
        public bool isWalker = false;
        public bool isLeader = false;
        public bool isFly = false;
        public bool isAircraft = false;
        public bool isChaos = false;
        public bool isDaemon = false;
        public bool isDestroyerCult = false;
        public List<string> keywords = new List<string>();

        public bool isHazardous = false;

        public CompProperties_AbilityGiveRandomHediffWithDuration() => this.compClass = typeof(CompAbilityEffect_GiveRandomHediffWithDuration);
    }
}
