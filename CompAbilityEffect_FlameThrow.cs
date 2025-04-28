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
    public class CompAbilityEffect_FlameThrow : CompAbilityEffect
    {
        private readonly List<IntVec3> tmpCells = new List<IntVec3>();

        private new CompProperties_AbilityFlameThrow Props => (CompProperties_AbilityFlameThrow)props;

        private Pawn Pawn => parent.pawn;

        private Verb ShootVerb => parent.verb;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (!Utility_TargetValidator.IsValidTargetForAbility(target.Thing, Pawn, Props))
            {
                return;
            }
            float armorPenetration = Props.armorPenetration;
            int damAmount = Props.damAmount;
            if (Props.damageDef == null)
                Props.damageDef = DamageDefOf.Flame;
            if (Props.useKeyword && Utility_PawnValidationManager.KeywordValidator(target.Pawn, Props.keywords, Props.isVehicle, Props.isMonster, Props.isPsychic, Props.isPsyker, Props.isCharacter, Props.isAstartes, Props.isInfantry, Props.isWalker, Props.isLeader, Props.isFly, Props.isAircraft, Props.isChaos, Props.isDaemon, Props.isDestroyerCult, Props.isHereticAstartes))
            {
                int rand = Rand.RangeInclusive(1, 6);
                if (Props.keywordBonus > 0 && Props.keywordBonus <= rand)
                {
                    armorPenetration = 999;
                }
            }
            if (Props.diceUsed > 0)
            {
                int diceResult = Rand.RangeInclusive(0, Props.diceUsed);
                damAmount = RandomDamageCalculator(diceResult, Props.fumbleDamageDice, Props.normalDamageDice, Props.criticalDamageDice);
            }
            if (Props.isHazardous && Utility_WeaponStatChanger.IsHazardousCalculator())
            {
                IntVec3 currentTargetCell = Pawn.Position;
                // 🔥 Internal damage to the caster (hazardous backfire)
                Utility_WeaponStatChanger.ApplySurgicalDamage(Pawn, Props.damageDef, damAmount / 2f, null, Pawn);
                MoteMaker.ThrowText(Pawn.DrawPos, Pawn.Map, "EMWH_WeaponAbilities_Hazardous".Translate(), Color.red);
            }
            if (!Utility_WeaponStatChanger.IsHazardousCalculator())
                GenExplosion.DoExplosion(target.Cell, parent.pawn.MapHeld, 0f, Props.damageDef, Pawn, postExplosionSpawnThingDef: Props.postExplosionSpawnThingDef, damAmount: damAmount, armorPenetration: armorPenetration, explosionSound: null, weapon: null, projectile: null, intendedTarget: null, postExplosionSpawnChance: Props.postExplosionSpawnChance, postExplosionSpawnThingCount: Props.postExplosionSpawnThingCount, postExplosionGasType: Props.postExplosionGasType, applyDamageToExplosionCellsNeighbors: Props.applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef: Props.preExplosionSpawnThingDef, preExplosionSpawnChance: Props.preExplosionSpawnChance, preExplosionSpawnThingCount: Props.preExplosionSpawnThingCount, chanceToStartFire: Props.chanceToStartFire, damageFalloff: false, direction: null, ignoredThings: null, affectedAngle: null, doVisualEffects: false, propagationSpeed: 0.6f, excludeRadius: 0f, doSoundEffects: false, postExplosionSpawnThingDefWater: Props.postExplosionSpawnThingDefWater, screenShakeFactor: 1f, flammabilityChanceCurve: ShootVerb.verbProps.flammabilityAttachFireChanceCurve, overrideCells: AffectedCells(target));
            Pawn.records.Increment(RecordDefOf.ShotsFired);
            base.Apply(target, dest);
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

        public override IEnumerable<PreCastAction> GetPreCastActions()
        {
            if (Props.effecterDef != null)
            {
                yield return new PreCastAction
                {
                    action = delegate (LocalTargetInfo a, LocalTargetInfo b)
                    {
                        parent.AddEffecterToMaintain(Props.effecterDef.Spawn(parent.pawn.Position, a.Cell, parent.pawn.Map), Pawn.Position, a.Cell, Props.ticksAwayFromCast, Pawn.MapHeld);
                    },
                    ticksAwayFromCast = Props.ticksAwayFromCast
                };
            }
        }

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            GenDraw.DrawFieldEdges(AffectedCells(target));
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            if (!target.IsValid || !target.HasThing)
                return false;

            // Validate all cells in affected area
            foreach (IntVec3 cell in AffectedCells(target))
            {
                List<Thing> things = cell.GetThingList(Pawn.Map);
                foreach (Thing thing in things)
                {
                    if (thing is Pawn p)
                    {
                        if (!Utility_TargetValidator.IsValidTargetForAbility(p, Pawn, Props))
                        {
                            return false; // ❌ Friendly or invalid pawn would be hit
                        }
                    }
                    else
                    {
                        if (!Utility_TargetValidator.IsValidTargetForAbility(thing, Pawn, Props))
                        {
                            return false; // ❌ Friendly or neutral structure would be hit
                        }
                    }
                }
            }

            return true;
        }

        private List<IntVec3> AffectedCells(LocalTargetInfo target)
        {
            tmpCells.Clear();
            Vector3 vector = Pawn.Position.ToVector3Shifted().Yto0();
            IntVec3 intVec = target.Cell.ClampInsideMap(Pawn.Map);
            if (Pawn.Position == intVec)
            {
                return tmpCells;
            }

            float lineWidthEnd = Props.lineWidthEnd;
            float rand;
            if (Props.randomAttacks > 0)
            {
                float baseShot = 1f;
                if (Props.randomAttacksShot > 0)
                {
                    baseShot = Props.randomAttacksShot;
                }
                rand = Rand.Range(baseShot, Props.randomAttacks);
                lineWidthEnd *= rand;
            }

            float lengthHorizontal = (intVec - Pawn.Position).LengthHorizontal;
            float num = (float)(intVec.x - Pawn.Position.x) / lengthHorizontal;
            float num2 = (float)(intVec.z - Pawn.Position.z) / lengthHorizontal;
            intVec.x = Mathf.RoundToInt((float)Pawn.Position.x + num * Props.range);
            intVec.z = Mathf.RoundToInt((float)Pawn.Position.z + num2 * Props.range);
            float target2 = Vector3.SignedAngle(intVec.ToVector3Shifted().Yto0() - vector, Vector3.right, Vector3.up);
            float num3 = lineWidthEnd / 2f;
            float num4 = Mathf.Sqrt(Mathf.Pow((intVec - Pawn.Position).LengthHorizontal, 2f) + Mathf.Pow(num3, 2f));
            float num5 = 57.29578f * Mathf.Asin(num3 / num4);
            int num6 = GenRadial.NumCellsInRadius(Props.range);
            for (int i = 0; i < num6; i++)
            {
                IntVec3 intVec2 = Pawn.Position + GenRadial.RadialPattern[i];
                if (CanUseCell(intVec2) && Mathf.Abs(Mathf.DeltaAngle(Vector3.SignedAngle(intVec2.ToVector3Shifted().Yto0() - vector, Vector3.right, Vector3.up), target2)) <= num5)
                {
                    tmpCells.Add(intVec2);
                }
            }

            List<IntVec3> list = GenSight.BresenhamCellsBetween(Pawn.Position, intVec);
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

                if (!c.InHorDistOf(Pawn.Position, Props.range))
                {
                    return false;
                }

                ShootLine resultingLine;
                return ShootVerb.TryFindShootLineFromTo(parent.pawn.Position, c, out resultingLine);
            }
        }

    }
}