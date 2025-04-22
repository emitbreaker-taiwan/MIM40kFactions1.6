using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions
{
    public class CompProperties_AbilitybyWeapon : CompProperties
    {
        public List<AbilityDef> abilities = new List<AbilityDef>();

        public CompProperties_AbilitybyWeapon()
        {
            this.compClass = typeof(CompAbilitybyWeapon);
        }
    }
}
