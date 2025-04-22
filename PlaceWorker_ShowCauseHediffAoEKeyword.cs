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
    public class PlaceWorker_ShowCauseHediffAoEKeyword : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            CompProperties_CauseHediff_AoEbyKeyword compProperties = def.GetCompProperties<CompProperties_CauseHediff_AoEbyKeyword>();
            if (compProperties != null)
            {
                GenDraw.DrawRadiusRing(center, compProperties.range, Color.white);
            }
        }
    }
}
