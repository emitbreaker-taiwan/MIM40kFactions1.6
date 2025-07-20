using System.Collections.Generic;
using Verse;
using RimWorld;
using static MIM40kFactions.Utility_TargetValidator;

namespace MIM40kFactions
{
    public class CompProperties_AbilityThunderBolt : CompProperties_AbilityEffect
    {
        public bool randomBurst = false;
        public int burstShotCount = 0;
        public bool isHazardous = false;
        public int sustainedHits = 0;
        public float millisecbetwenBurst = 16f; // 1 tick = 16.67 millisec
        public DamageDef damageDef = null;
        public int damageAmount = 10;

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
        public int keywordBonus = 0;

        public bool isHereticAstartes = false;

        public bool allowFriendlyFire = false;
        public bool targetHostilesOnly = true; // Default: true to avoid accidents
        public bool targetNeutralBuildings = false; // Default: false to avoid accidents

        public CompProperties_AbilityThunderBolt()
        {
            compClass = typeof(CompAbilityEffect_ThunderBolt);
        }
    }
}
