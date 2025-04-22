using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions
{
    public static class Utility_TargetValidator
    {
        public static bool IsValidTargetForAbility(Thing target, Pawn caster, ITargetingRules props = null)
        {
            if (target == null || caster == null)
                return false;

            if (target == caster)
                return false;

            // Pawn target handling
            if (target is Pawn pawn)
            {
                if (props?.TargetHostilesOnly == true && !pawn.HostileTo(caster))
                    return false;

                return true;
            }

            Faction casterFaction = caster.Faction;
            Faction targetFaction = target.Faction;

            if (props?.TargetHostilesOnly == true && targetFaction != null && casterFaction != null)
            {
                if (!targetFaction.HostileTo(casterFaction))
                    return false;
            }

            ThingDef def = target.def;
            if (def?.building != null)
            {
                bool isCombatStructure =
                    def.building.turretGunDef != null ||
                    def.building.isTrap ||
                    (def.building.buildingTags != null && def.building.buildingTags.Contains("Turret")) ||
                    def.HasModExtension<CombatBuildingExtension>();

                if (isCombatStructure)
                    return true;

                if (props?.TargetNeutralBuildings == true && (def.building != null || def.IsWeapon))
                    return true;
            }

            return false;
        }
    }
}