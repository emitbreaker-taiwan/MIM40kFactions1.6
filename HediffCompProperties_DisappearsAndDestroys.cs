using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class HediffCompProperties_DisappearsAndDestroys : HediffCompProperties_DisappearsDisableable
    {
        public HediffCompProperties_DisappearsAndDestroys()
        {
            compClass = typeof(HediffComp_DisappearsAndDestroys);
        }
    }
}
