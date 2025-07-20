using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions
{
    public class CompProperties_PreventDamage : CompProperties
    {
        public List<DamageDef> damageDefstoPrevent = new List<DamageDef>();

        public CompProperties_PreventDamage()
        {
            compClass = typeof(CompPreventDamage);
        }
    }
}
