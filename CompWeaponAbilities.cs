using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using Verse;
using static HarmonyLib.Code;

namespace MIM40kFactions
{
    public class CompWeaponAbilities : ThingComp
    {
        public CompProperties_WeaponAbilities Props => (CompProperties_WeaponAbilities)props;
        private bool debugMode => Props.debugMode;
        private int originalBurstShot;

        private bool IsCombatExtendedActive()
        {
            return ModsConfig.IsActive("CETeam.CombatExtended");
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            if (IsCombatExtendedActive())
            {
                return;
            }

            WeaponAbilityHandlingExtension modExtension = parent.def.GetModExtension<WeaponAbilityHandlingExtension>();
            if (modExtension == null)
            {
                return;
            }

            Verb verb = Utility_WeaponStatChanger.TryGetPrimaryVerb(pawn);
            if (verb == null)
            {
                return;
            }

            if (Props.isRapidFire)
            {
                Props.rapidfireBurstShot = modExtension.defaultBurstShotCount + Props.rapidFireAmount;
            }

            if (Props.isSustainedHits)
                Props.sustainedHitsBurstShot = modExtension.defaultBurstShotCount + Props.sustainedHitsAmount;

            // Pistol-specific adjustments
            if (Props.isPistol)
            {
                // Store original values if not already saved
                if (Props.originalVerbClass == null)
                {
                    Props.originalVerbClass = verb.verbProps.verbClass;
                }

                Type current = verb.verbProps.verbClass;

                if (current == typeof(Verb_ShootLasBeam))
                {
                    verb.verbProps.verbClass = typeof(Verb_ShootLasBeamPistol);
                }
                else if (current == typeof(Verb_ArcSprayPenetrate))
                {
                    verb.verbProps.verbClass = typeof(Verb_ArcSprayPenetratePistol);
                }
                else if (current == typeof(Verb_ShootChainBeam))
                {
                    verb.verbProps.verbClass = typeof(Verb_ShootChainBeamPistol);
                }
                else if (verb.verbProps.defaultProjectile != null)
                {
                    verb.verbProps.verbClass = typeof(Verb_ShootPistol);
                }

                // Remove all melee verbs from verbTracker
                Utility_WeaponStatChanger.DisableMeleeVerbs(pawn);
            }
        }

        public override void Notify_UsedWeapon(Pawn pawn)
        {
            base.Notify_UsedWeapon(pawn);
            if (IsCombatExtendedActive())
            {
                return;
            }

            WeaponAbilityHandlingExtension modExtension = parent.def.GetModExtension<WeaponAbilityHandlingExtension>();
            if (modExtension == null)
            {
                return;
            }

            Verb verb = Utility_WeaponStatChanger.TryGetPrimaryVerb(pawn);
            if (verb == null)
            {
                return;
            }

            IntVec3 targetPos = pawn.TargetCurrentlyAimingAt.Cell;
            float distance = IntVec3Utility.DistanceTo(pawn.Position, targetPos);

            CompChangeableBullet compChangeableBullet = parent.GetComp<CompChangeableBullet>();
            if (compChangeableBullet != null)
            {
                originalBurstShot = verb.verbProps.burstShotCount;
            }
            else if (modExtension != null)
            {
                originalBurstShot = modExtension.defaultBurstShotCount;
            }

            // Rapid Fire
            if (Props.isRapidFire && RapidShotValidator(distance, verb.verbProps.range, verb.verbProps.burstShotCount, originalBurstShot, Props.rapidfireBurstShot))
            {
                verb.verbProps.burstShotCount = originalBurstShot + Props.rapidfireBurstShot;
            }

            // Sustained Hits
            if (Props.isSustainedHits && SustainedHitsValidator(verb.verbProps.burstShotCount, originalBurstShot, Props.sustainedHitsAmount))
            {
                verb.verbProps.burstShotCount = originalBurstShot + Props.sustainedHitsAmount;
            }

            // Melta Bonus
            // Now handled by Verb_ShootBeam


            // Pistol Bonus
            //if (Props.isPistol && verb != null)
            //{

            //}

            // Devastating Wounds
            if (Props.isDevastatingWounds && IsCriticalCalculator(Props.randomSeed))
            {
                GenExplosion.DoExplosion(
                    pawn.TargetCurrentlyAimingAt.Cell,
                    pawn.MapHeld,
                    1,
                    DamageDefOf.Bomb,
                    pawn,
                    Props.hazardousDamageAmount,
                    999f,
                    null,
                    parent.def,
                    Props.projectile);

                if (pawn.TargetCurrentlyAimingAt.HasThing && pawn.TargetCurrentlyAimingAt.Thing is Pawn victim)
                {
                    Utility_WeaponStatChanger.ApplySurgicalDamage(victim, DamageDefOf.Burn, Props.hazardousDamageAmount, parent.def, pawn);
                }
            }

            // Hazardous (Backfire) Effect
            if (Props.isHazardous && Utility_WeaponStatChanger.IsHazardousCalculator())
            {
                GenExplosion.DoExplosion(
                    pawn.Position,
                    pawn.MapHeld,
                    Props.hazardousRadius,
                    Props.hazardousDamageDef,
                    pawn,
                    Props.hazardousDamageAmount,
                    Props.hazardousArmorPenetration,
                    null,
                    parent.def,
                    Props.projectile);

                Utility_WeaponStatChanger.ApplySurgicalDamage(pawn, Props.hazardousDamageDef, Props.hazardousDamageAmount / 2f, parent.def, pawn);
                MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "EMWH_WeaponAbilities_Hazardous".Translate(), Color.red);
            }

            if (originalBurstShot != verb.verbProps.burstShotCount)
            {
                verb.verbProps.burstShotCount = originalBurstShot;
            }
        }

        private bool RapidShotValidator(float distance, float range, int burstShotCount, int originalBurstShot, int rapidfireBurstShot)
        {
            return distance <= range / 2 && burstShotCount < originalBurstShot + rapidfireBurstShot;
        }

        private bool SustainedHitsValidator(int burstShotCount, int originalBurstShot, int sustainedHitsAmount)
        {
            return IsCriticalCalculator(Props.randomSeed) && burstShotCount < originalBurstShot + sustainedHitsAmount;
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            if (IsCombatExtendedActive())
            {
                return;
            }

            WeaponAbilityHandlingExtension modExtension = parent.def.GetModExtension<WeaponAbilityHandlingExtension>();
            if (modExtension == null)
            {
                return;
            }

            Verb verb = Utility_WeaponStatChanger.TryGetPrimaryVerb(pawn);
            if (verb == null)
            {
                return;
            }

            CompChangeableBullet compChangeableBullet = parent.GetComp<CompChangeableBullet>();
            if (compChangeableBullet == null)
            {
                Utility_WeaponStatChanger.ResetVerbProperties(verb.verbProps, modExtension);
            }

            if (modExtension.defaultBurstShotCount > 1 && verb.verbProps.burstShotCount <= 1)
            {
                Log.Error($"Weapon '{parent.Label}' has an invalid burst shot count when unequipped: {verb.verbProps.burstShotCount}");
            }

            RecoverOriginalValues(modExtension);

            // Restores vanilla behavior (CE auto-refreshes these anyway)
            Utility_WeaponStatChanger.RestoreVerbs(pawn);
        }

        private void RecoverOriginalValues(WeaponAbilityHandlingExtension modExtension)
        {
            if (modExtension != null)
            {
                Props.projectile = modExtension.defaultProjectile;
            }

            if (debugMode)
            {
                Log.Warning("ChaneableBullet - Projectile =" + Props.projectile);
            }

            if (Props.projectile == null)
            {
                Log.Error($"Weapon '{parent.Label}' has no valid projectile. Please contact to the mod developer.");
            }
        }

        private bool IsCriticalCalculator(float percentCritical)
        {
            return Rand.Range(10f, 60f) > percentCritical;
        }

        public override string CompInspectStringExtra()
        {
            if (parent.def == null || !parent.def.IsWeapon || ModsConfig.IsActive("CETeam.CombatExtended")) return null;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("EMWH_WeaponAbilities".Translate());

            if (Props.isPistol)
                sb.AppendLine("EMWH_WeaponAbilities_isPistol".Translate());

            if (Props.isRapidFire)
                sb.AppendLine("EMWH_WeaponAbilities_isRapidFire".Translate() + $" ({Props.rapidFireAmount})");

            if (Props.isSustainedHits)
                sb.AppendLine("EMWH_WeaponAbilities_isSustainedHits".Translate() + $" ({Props.sustainedHitsAmount})");

            if (Props.isHazardous)
                sb.AppendLine("EMWH_WeaponAbilities_isHazardous".Translate());

            if (Props.isDevastatingWounds)
                sb.AppendLine("EMWH_WeaponAbilities_isDevastatingWounds".Translate());

            if (Props.isMelta)
                sb.AppendLine("EMWH_WeaponAbilities_isMelta".Translate() + $" ({Props.meltaAmount})");

            if (Props.isPenetration)
                sb.AppendLine("EMWH_WeaponAbilities_isPenetration".Translate());

            return sb.ToString().TrimEnd();
        }

        public override string GetDescriptionPart()
        {
            if (parent.def == null || !parent.def.IsWeapon || ModsConfig.IsActive("CETeam.CombatExtended")) return null;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("EMWH_WeaponAbilities".Translate());

            if (Props.isRapidFire)
            {
                sb.AppendLine();
                sb.AppendLine("EMWH_WeaponAbilities_isRapidFire".Translate() + ":");
                sb.AppendLine("EMWH_WeaponAbilities_isRapidFireDesc".Translate() + $" {Props.rapidFireAmount}");
            }

            if (Props.isSustainedHits)
            {
                sb.AppendLine();
                sb.AppendLine("EMWH_WeaponAbilities_isSustainedHits".Translate() + ":");
                sb.AppendLine("EMWH_WeaponAbilities_isSustainedHitsDesc".Translate() + $" {Props.sustainedHitsAmount}");
            }

            if (Props.isHazardous)
            {
                sb.AppendLine();
                sb.AppendLine("EMWH_WeaponAbilities_isHazardous".Translate() + ":");
                sb.AppendLine("EMWH_WeaponAbilities_isHazardousDesc".Translate() + $" {Props.hazardousDamageAmount}");
            }

            if (Props.isDevastatingWounds)
            {
                sb.AppendLine();
                sb.AppendLine("EMWH_WeaponAbilities_isDevastatingWounds".Translate() + ":");
                sb.AppendLine("EMWH_WeaponAbilities_isDevastatingWoundsDesc".Translate());
            }

            if (Props.isMelta)
            {
                sb.AppendLine();
                sb.AppendLine("EMWH_WeaponAbilities_isMelta".Translate() + ":");
                sb.AppendLine("EMWH_WeaponAbilities_isMeltaDesc".Translate());
            }

            if (Props.isPistol)
            {
                sb.AppendLine();
                sb.AppendLine("EMWH_WeaponAbilities_isPistol".Translate() + ":");
                sb.AppendLine("EMWH_WeaponAbilities_isPistolDesc".Translate());
            }

            if (Props.isPenetration)
            {
                sb.AppendLine();
                sb.AppendLine("EMWH_WeaponAbilities_isPenetration".Translate() + ":");
                sb.AppendLine("EMWH_WeaponAbilities_isPenetrationDesc".Translate());
            }

            return sb.ToString().TrimEnd();
        }
    }
}