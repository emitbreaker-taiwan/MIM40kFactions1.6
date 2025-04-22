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
    public class CompChangeableBullet : ThingComp
    {
        public CompProperties_ChangeableBullet Props => (CompProperties_ChangeableBullet)props;

        public List<VerbProperties> VerbProperties => parent.def.Verbs;

        private CompMagazinePouch CompMagazinePouch;

        public bool debugMode = false;

        public override void Notify_Equipped(Pawn pawn)
        {
            WeaponAbilityHandlingExtension modExtension = parent.def.GetModExtension<WeaponAbilityHandlingExtension>();

            if (modExtension == null)
            {
                base.Notify_Equipped(pawn);
                return;
            }

            SetOriginalValues(modExtension);

            CompMagazinePouchGetter(pawn);

            Verb verb = Utility_WeaponStatChanger.TryGetPrimaryVerb(pawn);
            if (verb != null && CompMagazinePouch != null)
            {
                Utility_WeaponStatChanger.ApplyStatOverrides(verb.verbProps, CompMagazinePouch);
            }

            base.Notify_Equipped(pawn);
        }

        public override void Notify_UsedWeapon(Pawn pawn)
        {
            WeaponAbilityHandlingExtension modExtension = parent.def.GetModExtension<WeaponAbilityHandlingExtension>();
            if (modExtension == null)
            {
                base.Notify_UsedWeapon(pawn);
                return;
            }

            Verb verb = Utility_WeaponStatChanger.TryGetPrimaryVerb(pawn);
            if (verb != null)
            {
                Utility_WeaponStatChanger.ApplyStatOverrides(verb.verbProps, CompMagazinePouch);
            }

            base.Notify_UsedWeapon(pawn);
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            WeaponAbilityHandlingExtension modExtension = parent.def.GetModExtension<WeaponAbilityHandlingExtension>();
            if (modExtension == null)
            {
                return;
            }

            Verb verb = Utility_WeaponStatChanger.TryGetPrimaryVerb(pawn);
            if (verb != null && Props != null)
            {
                // Reset to original stats
                Utility_WeaponStatChanger.ResetVerbProperties(verb.verbProps, modExtension);
                verb.verbProps.requireLineOfSight = Props.originalLoS;
                verb.verbProps.label = Props.originalLabel;
            }
        }

        private void SetOriginalValues(WeaponAbilityHandlingExtension modExtension)
        {
            Props.originalLoS = VerbProperties[0].requireLineOfSight;
            Props.originalLabel = VerbProperties[0].label;
        }

        private void CompMagazinePouchGetter(Pawn pawn)
        {
            if (Props.targetMagazinePouch != null)
            {
                CompMagazinePouch = Utility_WeaponStatChanger.CompMagazinePouchGetter(CompMagazinePouch, VerbProperties, pawn, Props.targetMagazinePouch);
            }

            if (Props.targetMagazinePouches != null && CompMagazinePouch == null)
            {
                foreach (ThingDef targetMagazinePouch in Props.targetMagazinePouches)
                {
                    CompMagazinePouch = Utility_WeaponStatChanger.CompMagazinePouchGetter(CompMagazinePouch, VerbProperties, pawn, targetMagazinePouch);
                }
            }
        }

        //private void ChangeVerbProps(Pawn pawn, WeaponAbilityHandlingExtension weaponAbilityHandlingExtension)
        //{
        //    if (CompMagazinePouch != null && CompMagazinePouch.Props.label != null && VerbProperties[0].label != CompMagazinePouch.Props.label)
        //    {
        //        VerbProperties[0].label = Utility_WeaponStatChanger.CompMagazinePouchrequireLabel(CompMagazinePouch, VerbProperties, pawn, Props.targetMagazinePouch);
        //    }
        //    if (CompMagazinePouch != null && CompMagazinePouch.Props.selectedProjectile != null && VerbProperties[0].defaultProjectile != CompMagazinePouch.Props.selectedProjectile)
        //    {
        //        VerbProperties[0].defaultProjectile = Utility_WeaponStatChanger.CompMagazinePouchProjectile(CompMagazinePouch, VerbProperties, pawn, Props.targetMagazinePouch);
        //    }
        //    if (CompMagazinePouch != null && VerbProperties[0].requireLineOfSight != CompMagazinePouch.Props.requireLineofSight)
        //    {
        //        VerbProperties[0].requireLineOfSight = Utility_WeaponStatChanger.CompMagazinePouchrequireLineOfSight(CompMagazinePouch, VerbProperties, pawn, Props.targetMagazinePouch);
        //    }
        //    if (CompMagazinePouch != null && CompMagazinePouch.Props.range < 0 && Props.originalRange != VerbProperties[0].range)
        //    {
        //        VerbProperties[0].range = Utility_WeaponStatChanger.CompMagazinePouchrequireRange(CompMagazinePouch, VerbProperties, pawn, Props.targetMagazinePouch, weaponAbilityHandlingExtension);
        //    }
        //    if (CompMagazinePouch != null && CompMagazinePouch.Props.burstShotCount > 0 && VerbProperties[0].burstShotCount != CompMagazinePouch.Props.burstShotCount)
        //    {
        //        VerbProperties[0].burstShotCount = Utility_WeaponStatChanger.CompMagazinePouchrequireBurstshotCount(CompMagazinePouch, VerbProperties, pawn, Props.targetMagazinePouch);
        //    }
        //}

        //private void ResettoOriginalValues(WeaponAbilityHandlingExtension weaponAbilityHandlingExtension)
        //{
        //    VerbProperties[0].range = weaponAbilityHandlingExtension.defaultRange;
        //    VerbProperties[0].defaultProjectile = weaponAbilityHandlingExtension.defaultProjectile;
        //    VerbProperties[0].burstShotCount = weaponAbilityHandlingExtension.defaultBurstShotCount;
        //    VerbProperties[0].requireLineOfSight = Props.originalLoS;
        //    VerbProperties[0].label = Props.originalLabel;
        //}
    }
}