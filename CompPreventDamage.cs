using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions
{
    public class CompPreventDamage : ThingComp
    {
        public CompProperties_PreventDamage Props => props as CompProperties_PreventDamage;

        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            // Check if the damage type is fire
            if (Props.damageDefstoPrevent.Contains(dinfo.Def))
            {
                absorbed = true; // Absorb the damage, preventing it
                return;
            }

            absorbed = false; // Allow other types of damage
        }
    }
}
