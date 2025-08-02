using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class PlaceWorker_OnHammerfallBunker : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            if (!Utility_DependencyManager.IsAACoreActive())
                return false;
            Thing thing2 = map.thingGrid.ThingAt(loc, ThingDef.Named("EMSM_HammerfallBunker"));
            if (thing2 == null || thing2.Position != loc)
            {
                return "MustPlaceOnHammerfallBunker".Translate();
            }

            return true;
        }

        public override bool ForceAllowPlaceOver(BuildableDef otherDef)
        {
            if (!Utility_DependencyManager.IsAACoreActive())
                return false;
            return otherDef == ThingDef.Named("EMSM_HammerfallBunker");
        }
    }
}
