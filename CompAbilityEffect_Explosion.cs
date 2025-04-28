using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class CompAbilityEffect_Explosion : CompAbilityEffect
    {
        private readonly List<IntVec3> tmpCells = new List<IntVec3>();

        private Pawn Pawn => parent.pawn;

        public new CompProperties_AbilityExplosion Props => (CompProperties_AbilityExplosion)props;

        private Verb ShootVerb => parent.verb;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            IntVec3 targetCell;
            int damage = Props.damAmount;
            if (Props.damageDef == null)
                Props.damageDef = DamageDefOf.Bullet;
            if (Props.diceUsed > 0)
            {
                int diceResult = Rand.RangeInclusive(0, Props.diceUsed);
                target = IsSelfTarget(diceResult, Props.fumbleDamageDice, target);
                damage = RandomDamageCalculator(diceResult, Props.fumbleDamageDice, Props.normalDamageDice, Props.criticalDamageDice);
            }
            else
            {
                target = IsHazardousTarget(Props.isHazardous, target);
            }
            targetCell = target.Cell;
            CastEffectOnTarget(target, targetCell, Props.isHazardous, damage);
            base.Apply(target, dest);
        }
        private LocalTargetInfo IsSelfTarget(int diceResult, IntRange fumbleDamageDice, LocalTargetInfo target)
        {
            if (fumbleDamageDice == null)
                return target;
            if (diceResult == 0)
                return Pawn;
            else
                return target;
        }
        private int RandomDamageCalculator(int diceResult, IntRange fumbleDamageDice, IntRange normalDamageDice, IntRange criticalDamageDice)
        {
            int damageResult;
            if (normalDamageDice == null)
                normalDamageDice = new IntRange(1, 6);
            if (criticalDamageDice == null)
                criticalDamageDice = new IntRange(4, 6);

            if (diceResult == 0 && fumbleDamageDice != null)
            {
                damageResult = fumbleDamageDice.RandomInRange;
                return damageResult * Props.damAmount;
            }
            if (diceResult == 0 && fumbleDamageDice == null)
            {
                damageResult = normalDamageDice.RandomInRange;
                return damageResult * Props.damAmount;
            }
            if (diceResult == 6)
            {
                damageResult = criticalDamageDice.RandomInRange;
                return damageResult * Props.damAmount;
            }
            else
                damageResult = normalDamageDice.RandomInRange;
            return damageResult * Props.damAmount;
        }
        private void CastEffectOnTarget(LocalTargetInfo target, IntVec3 targetCell, bool isHazardous = false, int damageAmount = 0)
        {
            int burstShotCount = Props.burstShotCount;
            float armorPenetration = Props.armorPenetration;
            if (Props.randomBurst)
            {
                int baseShot = 1;
                if (Props.randomBurstFixedShot > 0)
                {
                    baseShot = Props.randomBurstFixedShot;
                }
                burstShotCount = Rand.RangeInclusive(baseShot, Props.burstShotCount);
            }
            if (burstShotCount > 0)
            {
                for (int i = 0; i < burstShotCount; i++)
                {
                    if (Time.time > Props.millisecbetwenBurst)
                    {
                        if (isHazardous && target == Pawn)
                        {
                            Utility_WeaponStatChanger.ApplySurgicalDamage(Pawn, Props.damageDef, damageAmount / 2f, null, Pawn);
                            // Cease fire: log, maybe visual feedback, then exit
                            MoteMaker.ThrowText(Pawn.DrawPos, Pawn.Map, "EMWH_WeaponAbilities_Hazardous".Translate(), Color.red);
                        }
                        else GenExplosion.DoExplosion(targetCell, parent.pawn.MapHeld, Props.effectRadius, Props.damageDef, Pawn, postExplosionSpawnThingDef: Props.postExplosionSpawnThingDef, damAmount: damageAmount, armorPenetration: KeywordEffect(target.Pawn, armorPenetration), explosionSound: null, weapon: null, projectile: null, intendedTarget: null, postExplosionSpawnChance: Props.postExplosionSpawnChance, postExplosionSpawnThingCount: Props.postExplosionSpawnThingCount, postExplosionGasType: Props.postExplosionGasType, applyDamageToExplosionCellsNeighbors: Props.applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef: Props.preExplosionSpawnThingDef, preExplosionSpawnChance: Props.preExplosionSpawnChance, preExplosionSpawnThingCount: Props.preExplosionSpawnThingCount, chanceToStartFire: Props.chanceToStartFire, damageFalloff: false, direction: null, ignoredThings: null, affectedAngle: null, doVisualEffects: false, propagationSpeed: 0.6f, excludeRadius: 0f, doSoundEffects: false, postExplosionSpawnThingDefWater: Props.postExplosionSpawnThingDefWater, screenShakeFactor: 1f, flammabilityChanceCurve: ShootVerb.verbProps.flammabilityAttachFireChanceCurve, overrideCells: AffectedCells(target));
                        targetCell.x = targetCell.x + Rand.RangeInclusive(0, 5);
                        targetCell.y = targetCell.y + Rand.RangeInclusive(0, 5);
                        Pawn.records.Increment(RecordDefOf.ShotsFired);
                    }
                }
            }
            else
            {
                if (isHazardous && target == Pawn)
                {
                    Utility_WeaponStatChanger.ApplySurgicalDamage(Pawn, Props.damageDef, damageAmount / 2f, null, Pawn);
                }
                else GenExplosion.DoExplosion(targetCell, parent.pawn.MapHeld, Props.effectRadius, Props.damageDef, Pawn, postExplosionSpawnThingDef: Props.postExplosionSpawnThingDef, damAmount: damageAmount, armorPenetration: KeywordEffect(target.Pawn, armorPenetration), explosionSound: null, weapon: null, projectile: null, intendedTarget: null, postExplosionSpawnChance: Props.postExplosionSpawnChance, postExplosionSpawnThingCount: Props.postExplosionSpawnThingCount, postExplosionGasType: Props.postExplosionGasType, applyDamageToExplosionCellsNeighbors: Props.applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef: Props.preExplosionSpawnThingDef, preExplosionSpawnChance: Props.preExplosionSpawnChance, preExplosionSpawnThingCount: Props.preExplosionSpawnThingCount, chanceToStartFire: Props.chanceToStartFire, damageFalloff: false, direction: null, ignoredThings: null, affectedAngle: null, doVisualEffects: false, propagationSpeed: 0.6f, excludeRadius: 0f, doSoundEffects: false, postExplosionSpawnThingDefWater: Props.postExplosionSpawnThingDefWater, screenShakeFactor: 1f, flammabilityChanceCurve: ShootVerb.verbProps.flammabilityAttachFireChanceCurve, overrideCells: AffectedCells(target));
                Pawn.records.Increment(RecordDefOf.ShotsFired);
            }
        }
        private float KeywordEffect(Pawn pawn, float armorPenetration)
        {
            if (Props.useKeyword && Utility_PawnValidationManager.KeywordValidator(pawn, Props.keywords, Props.isVehicle, Props.isMonster, Props.isPsychic, Props.isPsyker, Props.isCharacter, Props.isAstartes, Props.isInfantry, Props.isWalker, Props.isLeader, Props.isFly, Props.isAircraft, Props.isChaos, Props.isDaemon, Props.isDestroyerCult, Props.isHereticAstartes))
            {
                int rand = Rand.RangeInclusive(1, 6);
                if (Props.keywordBonus > 0 && Props.keywordBonus <= rand)
                {
                    armorPenetration = 999;
                }
            }
            return armorPenetration;
        }

        private LocalTargetInfo IsHazardousTarget(bool isHazardous, LocalTargetInfo target)
        {
            if (!isHazardous)
                return target;
            int diceGod = Rand.RangeInclusive(1, 6);
            if (diceGod == 1)
            {
                Utility_WeaponStatChanger.ApplySurgicalDamage(Pawn, Props.damageDef, Props.damAmount / 2f, null, Pawn);
                return Pawn;
            }
            return target;
        }

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            GenDraw.DrawFieldEdges(AffectedCells(target));
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            if (Pawn.Faction == Faction.OfPlayer)
            {
                return false;
            }

            if (Pawn.Faction != null)
            {
                foreach (IntVec3 item in AffectedCells(target))
                {
                    List<Thing> thingList = item.GetThingList(Pawn.Map);
                    for (int i = 0; i < thingList.Count; i++)
                    {
                        if (thingList[i].Faction == Pawn.Faction)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private List<IntVec3> AffectedCells(LocalTargetInfo target)
        {
            tmpCells.Clear();
            IntVec3 intVec = target.Cell.ClampInsideMap(Pawn.Map);
            if (Pawn.Position == intVec)
            {
                return tmpCells;
            }

            List<IntVec3> list = GenRadial.RadialCellsAround(intVec, Props.effectRadius, true).OfType<IntVec3>().ToList();
            for (int i = 0; i < list.Count; i++)
            {
                IntVec3 intVec3 = list[i];
                if (!tmpCells.Contains(intVec3) && CanUseCell(intVec3))
                {
                    tmpCells.Add(intVec3);
                }
            }

            return tmpCells;

            bool CanUseCell(IntVec3 c)
            {
                if (!c.InBounds(Pawn.Map))
                {
                    return false;
                }

                if (c == Pawn.Position)
                {
                    return false;
                }

                if (!Props.canHitFilledCells && c.Filled(Pawn.Map))
                {
                    return false;
                }

                if (!c.InHorDistOf(Pawn.Position, ShootVerb.verbProps.range))
                {
                    return false;
                }

                ShootLine resultingLine;
                return ShootVerb.TryFindShootLineFromTo(parent.pawn.Position, c, out resultingLine);
            }
        }
        private List<IntVec3> AffectedCellsHazardous(LocalTargetInfo target)
        {
            tmpCells.Clear();
            IntVec3 intVec = target.Cell.ClampInsideMap(Pawn.Map);

            List<IntVec3> list = GenRadial.RadialCellsAround(intVec, Props.effectRadius, true).OfType<IntVec3>().ToList();
            for (int i = 0; i < list.Count; i++)
            {
                IntVec3 intVec3 = list[i];
                if (!tmpCells.Contains(intVec3) && CanUseCell(intVec3))
                {
                    tmpCells.Add(intVec3);
                }
            }

            return tmpCells;

            bool CanUseCell(IntVec3 c)
            {
                if (!c.InBounds(Pawn.Map))
                {
                    return false;
                }

                if (!Props.canHitFilledCells && c.Filled(Pawn.Map))
                {
                    return false;
                }

                if (!c.InHorDistOf(Pawn.Position, ShootVerb.verbProps.range))
                {
                    return false;
                }

                ShootLine resultingLine;
                return ShootVerb.TryFindShootLineFromTo(parent.pawn.Position, c, out resultingLine);
            }
        }
    }
}