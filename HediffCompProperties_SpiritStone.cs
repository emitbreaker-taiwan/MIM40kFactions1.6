using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions.Aeldari
{
    public class HediffCompProperties_SpiritStone : HediffCompProperties
    {
        public ThingDef spiritStoneDef;
        public bool debugMode = false;

        public HediffCompProperties_SpiritStone()
        {
            compClass = typeof(HediffComp_SpiritStone);
        }
    }
}
