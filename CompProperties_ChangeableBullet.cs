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
    public class CompProperties_ChangeableBullet : CompProperties
    {
        public ThingDef targetMagazinePouch;

        public List<ThingDef> targetMagazinePouches;

        //public ThingDef originalProjectile;

        public bool originalLoS;

        //public float originalRange;

        //public int originalBurstShotCount;

        [NoTranslate]
        public string originalLabel;

        public CompProperties_ChangeableBullet()
        {
            compClass = typeof(CompChangeableBullet);
        }
    }
}
