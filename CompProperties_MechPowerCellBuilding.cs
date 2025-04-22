using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class CompProperties_MechPowerCellBuilding : CompProperties
    {
        public int totalPowerTicks = 2500;

        public bool killWhenDepleted = true;

        [MustTranslate]
        public string labelOverride;

        [MustTranslate]
        public string tooltipOverride;

        public CompProperties_MechPowerCellBuilding()
        {
            compClass = typeof(CompMechPowerCellBuilding);
        }
    }
}
