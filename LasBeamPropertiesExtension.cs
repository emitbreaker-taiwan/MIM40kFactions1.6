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
    public class LasBeamPropertiesExtension : DefModExtension
    {
        public bool canHitFilledCells = true;
        public bool penetratePawns = false;

        public float lineWidthEnd = -1f;

        //For ArcSpray
        public ThingDef incineratorSprayThingDef;
        public ThingDef incineratorMoteDef;

        //For Better Beam Control
        public float? beamWidth;
        public float? beamCurvature;
        public float? beamMaxDeviation;
        public bool? beamHitsNeighborCells;

        // Piercing beam
        public bool? isPiercingBeam = false;
        public float? piercingDamageFalloff;

        // Chain beam
        public int? chainMaxBounces;
        public float? chainBounceRange;
        public float? chainDamageFalloff;

        // Radial Arc beam
        public bool? radialArcBlast;
        public float? radialArcRange;
        public float? radialArcFalloff;
    }
}
