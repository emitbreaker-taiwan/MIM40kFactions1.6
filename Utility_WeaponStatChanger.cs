using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static HarmonyLib.Code;

namespace MIM40kFactions
{
    public class Utility_WeaponStatChanger
    {
        private static ThingWithComps magazine;

        public static CompMagazinePouch CompMagazinePouchGetter(CompMagazinePouch CompMagazinePouch, List<VerbProperties> VerbProperties, Pawn pawn, ThingDef magazinePouch)
        {
            foreach (Apparel apparel in pawn.apparel.WornApparel)
            {
                WeaponAbilityHandlingExtension weaponAbilityHandlingExtension = apparel.def.GetModExtension<WeaponAbilityHandlingExtension>();
                if (magazinePouch == apparel.def)
                {
                    magazine = (ThingWithComps)ThingMaker.MakeThing(apparel.def);
                    CompMagazinePouch = magazine.GetComp<CompMagazinePouch>();
                }
            }

            return CompMagazinePouch;
        }

        public static Verb TryGetPrimaryVerb(Pawn pawn)
        {
            return pawn?.equipment?.PrimaryEq?.AllVerbs?.FirstOrDefault();
        }

        public static void ApplyStatOverrides(VerbProperties verbProps, CompMagazinePouch pouch)
        {
            if (verbProps == null || pouch == null || pouch.Props == null) return;

            // Apply projectile override
            if (pouch.Props.selectedProjectile != null)
                verbProps.defaultProjectile = pouch.Props.selectedProjectile;

            // Apply burst shot override
            if (pouch.Props.burstShotCount > 0)
                verbProps.burstShotCount = pouch.Props.burstShotCount;

            // Apply range override
            if (pouch.Props.range > 0)
                verbProps.range = pouch.Props.range;

            // Apply label override
            if (!string.IsNullOrEmpty(pouch.Props.label))
                verbProps.label = pouch.Props.label;

            // Apply line of sight requirement
            verbProps.requireLineOfSight = pouch.Props.requireLineofSight;
        }

        public static void ApplySurgicalDamage(Pawn target, DamageDef damageDef, float amount, ThingDef weaponDef = null, Thing instigator = null)
        {
            if (target == null || target.Dead || !target.RaceProps.IsFlesh || damageDef == null)
                return;

            // 🔧 Pick any viable (not missing) part
            BodyPartRecord part = target.health.hediffSet
                .GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined)
                .Where(p => p.depth == BodyPartDepth.Inside || p.depth == BodyPartDepth.Outside)
                .InRandomOrder()
                .FirstOrDefault();

            if (part == null)
                return;

            DamageInfo dinfo = new DamageInfo(
                damageDef,
                amount,
                armorPenetration: 999f, // surgical: ignore armor
                angle: -1f,
                instigator: instigator,
                hitPart: part,
                weapon: weaponDef
            );

            target.TakeDamage(dinfo);
        }

        public static bool IsHazardousCalculator(int chance = 6)
        {
            return Rand.RangeInclusive(1, chance) == 6;
        }

        public static void DisableMeleeVerbs(Pawn pawn)
        {
            if (pawn?.verbTracker?.AllVerbs == null) return;

            List<Verb> verbs = pawn.verbTracker.AllVerbs;
            for (int i = verbs.Count - 1; i >= 0; i--)
            {
                Verb v = verbs[i];
                if (v != null && v.IsMeleeAttack)
                {
                    verbs.RemoveAt(i);
                }
            }
        }

        public static void RestoreVerbs(Pawn pawn)
        {
            // Optional: call this to rebuild verbs (vanilla does this on equip/refresh)
            pawn?.verbTracker?.InitVerbsFromZero();
        }

        public static void ResetVerbProperties(VerbProperties verbProps, WeaponAbilityHandlingExtension modExtension)
        {
            verbProps.range = modExtension.defaultRange;
            verbProps.defaultProjectile = modExtension.defaultProjectile;
            verbProps.burstShotCount = modExtension.defaultBurstShotCount;
        }
    }
}