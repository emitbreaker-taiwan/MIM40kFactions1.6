using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    [DefOf]
    public class MIM40kFactionsGameConditionDefOf
    {
        public static GameConditionDef EMWH_LivingLightning;

        static MIM40kFactionsGameConditionDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MIM40kFactionsGameConditionDefOf));
        }
    }
}
