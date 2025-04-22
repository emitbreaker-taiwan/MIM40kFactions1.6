using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class CompProperties_WeaponAbilities : CompProperties
    {
        public Type verbClass;

        public float randomSeed = 50f;
        public int hazardousSeed = 6;

        public bool isRapidFire = false;
        public int rapidFireAmount = 0;

        public bool isDevastatingWounds = false;

        public bool isSustainedHits = false;
        public int sustainedHitsAmount;

        public bool isMelta = false;
        public int meltaAmount;

        public bool isPistol = false;

        public bool isHazardous = false;
        public int hazardousDamageAmount = 50;
        public float hazardousArmorPenetration = 0f;
        public float hazardousRadius = 0f;
        public DamageDef hazardousDamageDef;
        public ThingDef preExplosionSpawnThingDef;
        public float preExplosionSpawnChance = 0f;
        public int preExplosionSpawnThingCount = 0;
        public ThingDef postExplosionSpawnThingDef;
        public ThingDef postExplosionSpawnThingDefWater;
        public float postExplosionSpawnChance = 0f;
        public int postExplosionSpawnThingCount = 0;
        public GasType? postExplosionGasType;
        public bool applyDamageToExplosionCellsNeighbors = false;
        public float chanceToStartFire = 0f;
        public float screenShakeFactor = 1f;

        public bool isPenetration = false;

        //public int originalBurstShot;

        public int originalDamageAmount;

        public int rapidfireBurstShot;

        public int sustainedHitsBurstShot;

        //public int adjustedDamageAmount;

        public ThingDef projectile;

        public Type originalVerbClass;

        public bool debugMode = false;

        public CompProperties_WeaponAbilities()
        {
            compClass = typeof(CompWeaponAbilities);
        }
    }
}
