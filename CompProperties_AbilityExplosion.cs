using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class CompProperties_AbilityExplosion : CompProperties_AbilityEffect
    {
        public ThingDef postExplosionSpawnThingDef;

        public int damAmount = -1;

        public bool canHitFilledCells = false;

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

        public float effectRadius = -1f;

        public int burstShotCount = 0;

        public bool isHazardous = false;

        public bool randomBurst = false;
        public int randomBurstFixedShot = 0;

        public float millisecbetwenBurst = 16f; // 1 tick = 16.67 millisec

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

        public int diceUsed = 0;
        public IntRange fumbleDamageDice;
        public IntRange normalDamageDice;
        public IntRange criticalDamageDice;

        public CompProperties_AbilityExplosion()
        {
            compClass = typeof(CompAbilityEffect_Explosion);
        }
    }
}
