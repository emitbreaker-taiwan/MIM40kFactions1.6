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
    public class CompConfigurator_WeaponAccuracy : ThingComp
    {
        private float accuracyModifier => LoadedModManager.GetMod<Mod_MIMWH40kFactions>().GetSettings<ModSettings_MIMWH40kFactions>().accuracyModifier;

        public CompProperties_ConfiguratorWeaponAccuracy Props => (CompProperties_ConfiguratorWeaponAccuracy)props;

        public override void Initialize(CompProperties props)
        {
            this.props = props;

            if (parent.def.statBases.NullOrEmpty())
            {
                return;
            }

            if (Props.originalAccuracyTouch < 0 && Props.originalAccuracyShort < 0 && Props.originalAccuracyMedium < 0 && Props.originalAccuracyLong < 0)
            {
                SaveOriginalStatValue();
            }

            UpdateStatValue();
        }

        private void SaveOriginalStatValue()
        {
            foreach (StatModifier modifier in parent.def.statBases)
            {
                if (modifier.stat == StatDefOf.AccuracyTouch && Props.originalAccuracyTouch < 0)
                {
                    Props.originalAccuracyTouch = modifier.value;
                }
                if (modifier.stat == StatDefOf.AccuracyShort && Props.originalAccuracyShort < 0)
                {
                    Props.originalAccuracyShort = modifier.value;
                }
                if (modifier.stat == StatDefOf.AccuracyMedium && Props.originalAccuracyMedium < 0)
                {
                    Props.originalAccuracyMedium = modifier.value;
                }
                if (modifier.stat == StatDefOf.AccuracyLong && Props.originalAccuracyLong < 0)
                {
                    Props.originalAccuracyLong = modifier.value;
                }
            }
        }

        private void UpdateStatValue()
        {
            foreach (StatModifier modifier in parent.def.statBases)
            {
                if (modifier.stat == StatDefOf.AccuracyTouch)
                {
                    modifier.value = Props.originalAccuracyTouch * (1f + accuracyModifier / 100f);
                }
                if (modifier.stat == StatDefOf.AccuracyShort)
                {
                    modifier.value = Props.originalAccuracyShort * (1f + accuracyModifier / 100f);
                }
                if (modifier.stat == StatDefOf.AccuracyMedium)
                {
                    modifier.value = Props.originalAccuracyMedium * (1f + accuracyModifier / 100f);
                }
                if (modifier.stat == StatDefOf.AccuracyLong)
                {
                    modifier.value = Props.originalAccuracyLong * (1f + accuracyModifier / 100f);
                }
            }
        }
    }
}