using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using static MIM40kFactions.Utility_TargetValidator;

namespace MIM40kFactions
{
    public class CompProperties_AbilityFlameThrow : CompProperties_AbilityEffect, ITargetingRules
    {
        public float range;

        public float lineWidthEnd;

        public ThingDef postExplosionSpawnThingDef;

        public int damAmount = -1;

        public EffecterDef effecterDef;

        public bool canHitFilledCells;

        public DamageDef damageDef;

        public float armorPenetration = -1f;

        public float postExplosionSpawnChance = -1f;

        public int postExplosionSpawnThingCount = 1;

        public GasType? postExplosionGasType = null;

        public bool applyDamageToExplosionCellsNeighbors = false;

        public ThingDef preExplosionSpawnThingDef;

        public float preExplosionSpawnChance = -1f;

        public int preExplosionSpawnThingCount = 1;

        public float chanceToStartFire = 1f;

        public ThingDef postExplosionSpawnThingDefWater;

        public float screenShakeFactor = 1f;

        public int ticksAwayFromCast = 17;

        public bool isHazardous = false;
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

        public int diceUsed = 0;
        public IntRange fumbleDamageDice;
        public IntRange normalDamageDice;
        public IntRange criticalDamageDice;

        public float randomAttacks;
        public float randomAttacksShot = -1f;

        public bool allowFriendlyFire = false;
        public bool targetHostilesOnly = true; // Default: true to avoid accidents
        public bool targetNeutralBuildings = false; // Default: false to avoid accidents

        public bool TargetHostilesOnly => targetHostilesOnly;
        public bool TargetNeutralBuildings => targetNeutralBuildings;

        public CompProperties_AbilityFlameThrow()
        {
            compClass = typeof(CompAbilityEffect_FlameThrow);
        }
    }
}
