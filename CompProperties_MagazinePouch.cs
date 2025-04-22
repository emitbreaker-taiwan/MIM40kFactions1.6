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
    public class CompProperties_MagazinePouch : CompProperties
    {
        public List<ThingDef> applicableWeapons;

        public List<ChangeableProjectileSets> changeableProjectiles;

        public ThingDef selectedProjectile;

        public bool requireLineofSight = true;

        public float range = -1f;

        public float additionalRange = -1f;

        public bool negativeRange = false;

        public int burstShotCount = -1;

        public string label;

        public CompProperties_MagazinePouch()
        {
            compClass = typeof(CompMagazinePouch);
        }
    }

    public class ChangeableProjectileSets
    {
        public ThingDef changeableProjectile;

        public ResearchProjectDef researchProjectRequired = null;

        public bool requiredLineofSight = true;

        public float range = -1f;

        public int burstShotCount = -1;

        public float additionalRange = -1f;

        public bool negativeRange = false;
    }
}
