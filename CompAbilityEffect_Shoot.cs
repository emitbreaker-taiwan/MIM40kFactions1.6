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
    public class CompAbilityEffect_Shoot : CompAbilityEffect
    {
        public new CompProperties_AbilityShoot Props => (CompProperties_AbilityShoot)props;

        private Verb ShootVerb => parent.verb;

        private int burstShotsLeft;
        private int ticksUntilNextBurst;
        private LocalTargetInfo currentBurstTarget;
        private Pawn caster;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            caster = parent.pawn;
            currentBurstTarget = target;
            burstShotsLeft = Props.burstShotCount;
            ticksUntilNextBurst = 0;

            // Optional: randomize burst count
            if (Props.randomBurst)
            {
                int baseShot = (Props.randomBurstFixedShot > 0) ? Props.randomBurstFixedShot : 1;
                burstShotsLeft = Rand.RangeInclusive(baseShot, Props.burstShotCount);
            }

            // Optional: sustained hits bonus
            if (Props.sustainedHits > 0 && Rand.RangeInclusive(1, 6) == 6)
            {
                burstShotsLeft += Props.sustainedHits;
            }

            // Start firing now
            TryFireNextShot();
        }

        public override void CompTick()
        {
            base.CompTick();

            if (burstShotsLeft > 0)
            {
                if (ticksUntilNextBurst <= 0)
                {
                    TryFireNextShot();
                    ticksUntilNextBurst = Props.ticksBetweenBurstShots > 0 ? Props.ticksBetweenBurstShots : 10; // Default to 10 if unset
                }
                else
                {
                    ticksUntilNextBurst--;
                }
            }
        }

        private void TryFireNextShot()
        {
            if (currentBurstTarget == null || !currentBurstTarget.IsValid)
                return;

            // 🔒 Prevent targeting self or friendly unit
            if (currentBurstTarget.Pawn != null)
            {
                if (currentBurstTarget.Pawn == caster)
                {
                    burstShotsLeft = 0;
                    return;
                }

                if (Props.targetHostilesOnly && !Utility_TargetValidator.IsValidTargetForAbility(currentBurstTarget.Thing, caster, Props))
                {
                    burstShotsLeft = 0;
                    return;
                }
            }

            float explosionRadius = Props.projectileDef.projectile.explosionRadius;
            if (explosionRadius <= 0f)
                explosionRadius = 0.1f;

            Thing projectile = ThingMaker.MakeThing(Props.projectileDef);

            // ☢️ Hazardous shot check
            if (Props.isHazardous && Utility_WeaponStatChanger.IsHazardousCalculator())
            {
                GenExplosion.DoExplosion(caster.Position, caster.Map, explosionRadius, Props.projectileDef.projectile.damageDef,
                    instigator: null,
                    damAmount: Props.projectileDef.projectile.GetDamageAmount(projectile));
                caster.records.Increment(RecordDefOf.ShotsFired);
                burstShotsLeft--;
                return;
            }

            // 🎯 Normal shot
            bool fired = TryCastShot(currentBurstTarget, caster);
            if (fired)
            {
                caster.records.Increment(RecordDefOf.ShotsFired);
            }

            // 🎉 Keyword explosion bonus (only on last shot)
            if (burstShotsLeft == 1 && Props.useKeyword &&
                Utility_PawnValidator.KeywordValidator(
                    currentBurstTarget.Pawn, Props.keywords,
                    Props.isVehicle, Props.isMonster, Props.isPsychic, Props.isPsyker, Props.isCharacter,
                    Props.isAstartes, Props.isInfantry, Props.isWalker, Props.isLeader,
                    Props.isFly, Props.isAircraft, Props.isChaos, Props.isDaemon,
                    Props.isDestroyerCult, Props.isHereticAstartes))
            {
                int rand = Rand.RangeInclusive(1, 6);
                if (Props.keywordBonus > 0 && Props.keywordBonus <= rand)
                {
                    GenExplosion.DoExplosion(currentBurstTarget.Cell, caster.Map, explosionRadius,
                        Props.projectileDef.projectile.damageDef, instigator: null,
                        damAmount: Props.projectileDef.projectile.GetDamageAmount(projectile));
                }
            }

            burstShotsLeft--;
        }

        private bool TryCastShot(LocalTargetInfo currentTarget, Pawn caster)
        {
            if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map)
            {
                return false;
            }

            ThingDef projectile = Props.projectileDef;
            if (projectile == null)
            {
                return false;
            }

            ShootLine resultingLine;
            bool flag = ShootVerb.TryFindShootLineFromTo(caster.Position, currentTarget, out resultingLine);
            if (ShootVerb.verbProps.stopBurstWithoutLos && !flag)
            {
                return false;
            }

            Vector3 drawPos = caster.DrawPos;
            Projectile projectile2 = (Projectile)GenSpawn.Spawn(projectile, resultingLine.Source, caster.Map);
            ShotReport shotReport = ShotReport.HitReportFor(caster, ShootVerb, currentTarget);
            Thing randomCoverToMissInto = shotReport.GetRandomCoverToMissInto();
            ThingDef targetCoverDef = randomCoverToMissInto?.def;
            if (ShootVerb.verbProps.canGoWild && !Rand.Chance(shotReport.AimOnTargetChance_IgnoringPosture))
            {
                bool flyOverhead = projectile2?.def?.projectile != null && projectile2.def.projectile.flyOverhead;
                resultingLine.ChangeDestToMissWild_NewTemp(shotReport.AimOnTargetChance_StandardTarget, flyOverhead, caster.Map);
                ThrowDebugText("Wild\nDest", resultingLine.Dest);
                ProjectileHitFlags projectileHitFlags2 = ProjectileHitFlags.NonTargetWorld;
                if (Rand.Chance(0.5f))
                {
                    projectileHitFlags2 |= ProjectileHitFlags.NonTargetPawns;
                }

                projectile2.Launch(caster, drawPos, resultingLine.Dest, currentTarget, projectileHitFlags2, Props.preventFriendlyFire, null, targetCoverDef);
                return true;
            }

            if (currentTarget.Thing != null && currentTarget.Thing.def.CanBenefitFromCover && !Rand.Chance(shotReport.PassCoverChance))
            {
                ThrowDebugText("Cover\nDest", randomCoverToMissInto.Position);
                ProjectileHitFlags projectileHitFlags3 = ProjectileHitFlags.NonTargetWorld;

                projectile2.Launch(caster, drawPos, randomCoverToMissInto, currentTarget, projectileHitFlags3, Props.preventFriendlyFire, null, targetCoverDef);
                return true;
            }

            ProjectileHitFlags projectileHitFlags4 = ProjectileHitFlags.IntendedTarget;
            if (!currentTarget.HasThing || currentTarget.Thing.def.Fillage == FillCategory.Full)
            {
                projectileHitFlags4 |= ProjectileHitFlags.NonTargetWorld;
            }

            if (!currentTarget.HasThing || currentTarget.Thing.def.Fillage == FillCategory.Full)
            {
                projectileHitFlags4 |= ProjectileHitFlags.NonTargetWorld;
            }

            if (currentTarget.Thing != null)
            {
                projectile2.Launch(caster, drawPos, currentTarget, currentTarget, projectileHitFlags4, Props.preventFriendlyFire, null, targetCoverDef);
                ThrowDebugText("Hit\nDest", currentTarget.Cell);
            }
            else
            {
                projectile2.Launch(caster, drawPos, resultingLine.Dest, currentTarget, projectileHitFlags4, Props.preventFriendlyFire, null, targetCoverDef);
                ThrowDebugText("Hit\nDest", resultingLine.Dest);
            }

            return true;
        }
        private void ThrowDebugText(string text, IntVec3 c)
        {
            if (DebugViewSettings.drawShooting)
            {
                MoteMaker.ThrowText(c.ToVector3Shifted(), parent.pawn.Map, text);
            }
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            if (parent.pawn.Faction == Faction.OfPlayer)
                return false;

            if (!target.IsValid || !target.HasThing)
                return false;

            // ✅ Prevent friendly fire for pawns
            if (target.Pawn != null)
            {
                if (target.Pawn == parent.pawn)
                    return false;

                if (Props.targetHostilesOnly && !Utility_TargetValidator.IsValidTargetForAbility(currentBurstTarget.Thing, caster, Props))
                    return false;
            }
            else if (Props.targetHostilesOnly && !Utility_TargetValidator.IsValidTargetForAbility(currentBurstTarget.Thing, caster, Props)) // ✅ Add this
            {
                return false;
            }

            TargetingParameters targetParm = ShootVerb.verbProps.targetParams;

            if (targetParm.canTargetPawns && target.Pawn != null)
                return true;

            if (targetParm.canTargetLocations)
                return true;

            return false;
        }

        private float ForcedMissRadiusCalculator(IntVec3 a, IntVec3 b)
        {
            return Mathf.Sqrt((b.x - a.x) * (b.x - a.x) + (b.z - a.z) * (b.z - a.z));
        }
    }
}