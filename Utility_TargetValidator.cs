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
        public static bool IsValidTargetForAbility(Thing target, Pawn caster, bool targetHostilesOnly = false, bool targetNeutralBuildings = false, bool allowNoTarget = false)
        {
            if (caster == null)
                return false;

            if (target == null)
                return allowNoTarget;

            if (target == caster)
                return false;

            // Pawn target handling
            if (target is Pawn pawn)
            {
                if (caster.Faction == Faction.OfPlayer)
                    return true;

                if (targetHostilesOnly == true && !pawn.HostileTo(caster))
                    return false;

                return true;
            }

            Faction casterFaction = caster.Faction;
            Faction targetFaction = target.Faction;

            if (targetHostilesOnly == true && targetFaction != null && casterFaction != null)
            {
                if (caster.Faction == Faction.OfPlayer)
                    return true;

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

                // For neutral buildings
                if (targetNeutralBuildings || caster.Faction == Faction.OfPlayer)
                    return true;

                // For non-neutral buildings, must be hostile
                if (targetFaction != null && casterFaction != null)
                    return targetFaction.HostileTo(casterFaction);
            }

            return false;
        }
    }
}