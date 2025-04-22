using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using static MIM40kFactions.Utility_TargetValidator;

namespace MIM40kFactions
{
    public class CompProperties_AbilityShootBeam : CompProperties_AbilityEffect, ITargetingRules
    {
        public int burstShotCount = 1;

        public int ticksBetweenBurstShots = 15;

        public float lineWidthEnd;

        public bool canHitFilledCells = true;

        public DamageDef beamDamageDef;

        public float beamWidth = 1f;

        public float beamMaxDeviation;

        public FleckDef beamGroundFleckDef;

        public EffecterDef beamEndEffecterDef;

        public ThingDef beamMoteDef;

        public float beamFleckChancePerTick;

        public float beamCurvature;

        public float beamChanceToStartFire;

        public float beamChanceToAttachFire;

        public float beamStartOffset;

        public float beamFullWidthRange;

        public FleckDef beamLineFleckDef;

        public SimpleCurve beamLineFleckChanceCurve;

        public bool beamSetsGroundOnFire;

        public bool beamHitsNeighborCells;

        public bool beamCantHitWithinMinRange;

        public bool beamHitsNeighborCellsRequiresLOS;

        public FloatRange beamFireSizeRange = FloatRange.ZeroToOne;

        public SoundDef soundCastBeam;

        public SimpleCurve flammabilityAttachFireChanceCurve;

        public Color? highlightColor;

        public Color? secondaryHighlightColor;

        public int ticksAwayFromCast = 17;

        public bool isPenetration = false;

        //Reserve for explosion
        public float range;

        public ThingDef postExplosionSpawnThingDef;

        public int damAmount = -1;

        public EffecterDef effecterDef;

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

        //For ArcSpray
        public bool isArcSprayAbility = false;
        public ThingDef incineratorSprayThingDef;
        public ThingDef incineratorMoteDef;

        //Keywords

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

        //Random Damage

        public int diceUsed = 0;
        public IntRange fumbleDamageDice;
        public IntRange normalDamageDice;
        public IntRange criticalDamageDice;

        public float randomAttacks;

        public bool allowFriendlyFire = false;
        public bool targetHostilesOnly = true; // Default: true to avoid accidents
        public bool targetNeutralBuildings = false; // Default: false to avoid accidents

        public bool TargetHostilesOnly => targetHostilesOnly;
        public bool TargetNeutralBuildings => targetNeutralBuildings;

        public CompProperties_AbilityShootBeam()
        {
            compClass = typeof(CompAbilityEffect_ShootBeam);
        }
    }
}
