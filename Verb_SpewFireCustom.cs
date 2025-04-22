using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;
using static RimWorld.PsychicRitualRoleDef;
using static HarmonyLib.Code;

namespace MIM40kFactions
{
    public class Verb_SpewFireCustom : Verb
    {
        private EffecterDef effecterDef = EffecterDefOf.Fire_SpewShort;
        private FloatRange? affectedAngle;
        private DamageDef damageDef = DamageDefOf.Flame;
        private int damageAmount = -1;
        private float armorPenetration = -1f;
        private SoundDef explosionSound = null;
        private ThingDef weapon = null;
        private ThingDef postExplosionSpawnThingDef = ThingDefOf.Filth_FlammableBile;
        private float postExplosionSpawnChance = 1f;
        private int postExplosionSpawnThingCount = 1;
        private GasType? postExplosionGasType = null;
        private bool applyDamageToExplosionCellsNeighbors = false;
        private ThingDef preExplosionSpawnThingDef = null;
        private float preExplosionSpawnChance = -1f;
        private int preExplosionSpawnThingCount = 1;
        private float chanceToStartFire = 1f;
        private bool damageFalloff = false;
        private float propagationSpeed = 0.6f;
        private float? direction = null;
        private List<Thing> ignoredThings = null;
        private bool doVisualEffects = false;
        private bool doSoundEffects = false;
        private float excludeRadius = 0f;
        private int tickstoMaintain = 14;

        private float lineWidthEnd = 13f;
        private bool canHitFilledCells = true;

        private readonly List<IntVec3> tmpCells = new List<IntVec3>();

        protected override bool TryCastShot()
        {
            if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map)
            {
                return false;
            }

            if (base.EquipmentSource != null)
            {
                base.EquipmentSource.GetComp<CompChangeableProjectile>()?.Notify_ProjectileLaunched();
                base.EquipmentSource.GetComp<CompApparelReloadable>()?.UsedOnce();
            }

            IntVec3 position = caster.Position;
            float num = Mathf.Atan2(-(currentTarget.Cell.z - position.z), currentTarget.Cell.x - position.x) * 57.29578f;

            affectedAngle = new FloatRange(num - 13f, num + 13f);
            if (verbProps.minRange > 0)
            {
                excludeRadius = verbProps.minRange;
            }
            SpewFirePropertiesExtension modExtension = EquipmentSource.def.GetModExtension<SpewFirePropertiesExtension>();
            if (modExtension != null)
            {
                if (effecterDef == null)
                {
                    effecterDef = modExtension.effeterDef;
                }
                affectedAngle = new FloatRange(num - modExtension.angle, num + modExtension.angle);
                damageDef = modExtension.damageDef;
                damageAmount = modExtension.damageAmount;
                armorPenetration = modExtension.armorPenetration;
                explosionSound = modExtension.explosionSound;
                weapon = EquipmentSource.def;
                postExplosionSpawnThingDef = modExtension.postExplosionSpawnThingDef;
                postExplosionSpawnChance = modExtension.postExplosionSpawnChance;
                postExplosionSpawnThingCount = modExtension.postExplosionSpawnThingCount;
                postExplosionGasType = modExtension.postExplosionGasType;
                preExplosionSpawnThingDef = modExtension.preExplosionSpawnThingDef;
                preExplosionSpawnThingCount = modExtension.preExplosionSpawnThingCount;
                chanceToStartFire = modExtension.chanceToStartFire;
                damageFalloff = modExtension.damageFalloff;
                propagationSpeed = modExtension.propagationSpeed;
                direction = modExtension.direction;
                doVisualEffects = modExtension.doVisualEffects;
                tickstoMaintain = modExtension.tickstoMaintain;

                if (modExtension.ignoredThingsDef != null)
                {
                    foreach (ThingDef thingDef in modExtension.ignoredThingsDef)
                    {
                        Thing thing = ThingMaker.MakeThing(thingDef);
                        ignoredThings.Add(thing);
                    }
                }
                if (modExtension.excludeRadius > 0)
                {
                    excludeRadius = modExtension.excludeRadius;
                }
            }

            GenExplosion.DoExplosion(affectedAngle: affectedAngle, center: position, map: caster.MapHeld, radius: verbProps.range, damType: damageDef, instigator: caster, damAmount: damageAmount, armorPenetration: armorPenetration, explosionSound: explosionSound, weapon: weapon, projectile: null, intendedTarget: null, postExplosionSpawnThingDef: postExplosionSpawnThingDef, postExplosionSpawnChance: postExplosionSpawnChance, postExplosionSpawnThingCount: postExplosionSpawnThingCount, postExplosionGasType: postExplosionGasType, applyDamageToExplosionCellsNeighbors: applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef: preExplosionSpawnThingDef, preExplosionSpawnChance: preExplosionSpawnChance, preExplosionSpawnThingCount: preExplosionSpawnThingCount, chanceToStartFire: chanceToStartFire, damageFalloff: damageFalloff, direction: direction, ignoredThings: ignoredThings, doVisualEffects: doVisualEffects, propagationSpeed: propagationSpeed, excludeRadius: excludeRadius, doSoundEffects: doSoundEffects);
            AddEffecterToMaintain(effecterDef.Spawn(caster.Position, currentTarget.Cell, caster.Map), caster.Position, currentTarget.Cell, tickstoMaintain, caster.Map);
            lastShotTick = Find.TickManager.TicksGame;
            return true;
        }

        public override bool Available()
        {
            if (!base.Available())
            {
                return false;
            }

            if (CasterIsPawn)
            {
                Pawn casterPawn = CasterPawn;
                if (casterPawn.Faction != Faction.OfPlayer && casterPawn.mindState.MeleeThreatStillThreat && casterPawn.mindState.meleeThreat.Position.AdjacentTo8WayOrInside(casterPawn.Position))
                {
                    return false;
                }
            }

            return true;
        }

        public override void DrawHighlight(LocalTargetInfo target)
        {
            base.DrawHighlight(target);
            GenDraw.DrawFieldEdges(AffectedCells(target));
        }

        private List<IntVec3> AffectedCells(LocalTargetInfo target)
        {
            tmpCells.Clear();
            Vector3 vector = Caster.Position.ToVector3Shifted().Yto0();
            IntVec3 intVec = target.Cell.ClampInsideMap(Caster.Map);
            if (Caster.Position == intVec)
            {
                return tmpCells;
            }

            SpewFirePropertiesExtension modExtension = EquipmentSource.def.GetModExtension<SpewFirePropertiesExtension>();
            if (modExtension != null)
            {
                lineWidthEnd = modExtension.angle;
                canHitFilledCells = modExtension.canHitFilledCells;
                if (modExtension.randomAttacks > 0)
                {
                    lineWidthEnd *= Rand.Range(1f, modExtension.randomAttacks);
                }
            }

            float lengthHorizontal = (intVec - Caster.Position).LengthHorizontal;
            float num = (float)(intVec.x - Caster.Position.x) / lengthHorizontal;
            float num2 = (float)(intVec.z - Caster.Position.z) / lengthHorizontal;
            intVec.x = Mathf.RoundToInt((float)Caster.Position.x + num * verbProps.range);
            intVec.z = Mathf.RoundToInt((float)Caster.Position.z + num2 * verbProps.range);
            float target2 = Vector3.SignedAngle(intVec.ToVector3Shifted().Yto0() - vector, Vector3.right, Vector3.up);
            float num3 = lineWidthEnd / 2f;
            float num4 = Mathf.Sqrt(Mathf.Pow((intVec - Caster.Position).LengthHorizontal, 2f) + Mathf.Pow(num3, 2f));
            float num5 = 57.29578f * Mathf.Asin(num3 / num4);
            int num6 = GenRadial.NumCellsInRadius(verbProps.range);
            for (int i = 0; i < num6; i++)
            {
                IntVec3 intVec2 = Caster.Position + GenRadial.RadialPattern[i];
                if (CanUseCell(intVec2) && Mathf.Abs(Mathf.DeltaAngle(Vector3.SignedAngle(intVec2.ToVector3Shifted().Yto0() - vector, Vector3.right, Vector3.up), target2)) <= num5)
                {
                    tmpCells.Add(intVec2);
                }
            }

            List<IntVec3> list = GenSight.BresenhamCellsBetween(Caster.Position, intVec);
            for (int j = 0; j < list.Count; j++)
            {
                IntVec3 intVec3 = list[j];
                if (!tmpCells.Contains(intVec3) && CanUseCell(intVec3))
                {
                    tmpCells.Add(intVec3);
                }
            }

            return tmpCells;

            bool CanUseCell(IntVec3 c)
            {
                if (!c.InBounds(Caster.Map))
                {
                    return false;
                }

                if (c == Caster.Position)
                {
                    return false;
                }

                if (!canHitFilledCells && c.Filled(Caster.Map))
                {
                    return false;
                }

                if (!c.InHorDistOf(Caster.Position, verbProps.range))
                {
                    return false;
                }

                ShootLine resultingLine;
                return TryFindShootLineFromTo(Caster.Position, c, out resultingLine);
            }
        }
    }
}