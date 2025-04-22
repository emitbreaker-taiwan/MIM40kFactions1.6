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
    public class SpewFirePropertiesExtension : DefModExtension
    {
        public EffecterDef effeterDef;
        public float angle = 1f;
        public DamageDef damageDef;
        public int damageAmount = -1;
        public float armorPenetration = -1f;
        public SoundDef explosionSound = null;
        public ThingDef postExplosionSpawnThingDef = null;
        public float postExplosionSpawnChance = 1f;
        public int postExplosionSpawnThingCount = 1;
        public GasType? postExplosionGasType = null;
        public bool applyDamageToExplosionCellsNeighbors = false;
        public ThingDef preExplosionSpawnThingDef = null;
        public float preExplosionSpawnChance = -1f;
        public int preExplosionSpawnThingCount = 1;
        public float chanceToStartFire = 1f;
        public bool damageFalloff = false;
        public float propagationSpeed = 0.6f;
        public List<ThingDef> ignoredThingsDef = null;
        public float? direction = null;
        public bool doVisualEffects = false;
        public bool doSoundEffects = false;
        public float excludeRadius = 0f;
        public int tickstoMaintain = 14;

        public bool canHitFilledCells = true;


        public float randomAttacks = -1f;
    }
}
