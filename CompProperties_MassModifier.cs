using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions
{
    public class CompProperties_MassModifier : CompProperties
    {
        public float massMultiplier = 1f;
        public float flatBonus = 0f;

        public CompProperties_MassModifier()
        {
            this.compClass = typeof(CompMassModifier);
        }
    }
}
