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
    public class CompProperties_ConfiguratorWeaponAccuracy : CompProperties
    {
        public float originalAccuracyTouch = -1f;
        public float originalAccuracyShort = -1f;
        public float originalAccuracyMedium = -1f;
        public float originalAccuracyLong = -1f;

        public CompProperties_ConfiguratorWeaponAccuracy()
        {
            compClass = typeof(CompConfigurator_WeaponAccuracy);
        }
    }
}
