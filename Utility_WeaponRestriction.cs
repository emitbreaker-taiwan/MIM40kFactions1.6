using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace MIM40kFactions
{
    public static class Utility_WeaponRestriction
    {
        private static StringBuilder errorPart = new StringBuilder();

        public static bool PawnCannotEquipby(Thing thing, Pawn pawn, ref string cantReason)
        {
            cantReason = null;

            if (ModsConfig.BiotechActive == true && DoGeneConsideration(thing, pawn) == false)
            {
                cantReason = "EMWH_CannotEquipbyGene".Translate();
                return false;
            }
            if (!CheckRaceAllowance(thing, pawn))
            {
                cantReason = "EMWH_CannotEquipbyRaceWeapon".Translate();
                return false;
            }
            if (!CheckTraitAllowance(thing, pawn))
            {
                errorPart.Clear();
                errorPart.Append((string)"EMWH_RequiredTraitisMissing".Translate() + ": ");
                string stringSub = string.Empty;
                WeaponRestrictionExtension modExtension = thing.def.GetModExtension<WeaponRestrictionExtension>();
                foreach (TraitDef traitDef in modExtension.requiredTraits)
                {
                    stringSub = stringSub.NullOrEmpty() ? traitDef.label.ToString() : stringSub + ", " + traitDef.label.ToString();
                }
                errorPart.Append(stringSub);
                cantReason = errorPart.ToString();
                return false;
            }
            if (!CheckHediffRequirement(thing, pawn))
            {
                errorPart.Clear();
                errorPart.Append((string)"EMWH_RequiredHediffisMissing".Translate() + ": ");
                string stringSub = string.Empty;
                WeaponRestrictionExtension modExtension = thing.def.GetModExtension<WeaponRestrictionExtension>();
                if (modExtension.errorMessageAlt != null)
                {
                    stringSub = stringSub.NullOrEmpty() ? modExtension.errorMessageAlt.Translate() : stringSub + ", " + modExtension.errorMessageAlt.Translate();
                }
                else
                {
                    foreach (HediffDef hediffdef in modExtension.requiredHediffDefs)
                    {
                        stringSub = stringSub.NullOrEmpty() ? hediffdef.label.ToString() : stringSub + ", " + hediffdef.label.ToString();
                    }
                }
                errorPart.Append(stringSub);
                cantReason = errorPart.ToString();
                return false;
            }
            return true;
        }

        private static bool DoGeneConsideration(Thing thing, Pawn pawn)
        {
            List<Gene> pawnGenes = pawn.genes.GenesListForReading;
            int i = 0;
            int j = 0;

            foreach (Gene candidateGene in pawnGenes)
            {
                i++;
                if (candidateGene.def.GetModExtension<WeaponRestrictionExtension>() != null)
                {
                    j++;
                    if (CheckRestrictionGene(thing, candidateGene) == true)
                        return true;
                }
            }
            if (i == pawn.genes.GenesListForReading.Count && j == 0)
            {
                return true;
            }

            return false;
        }

        private static bool CheckRestrictionGene(Thing thing, Gene candidateGene)
        {
            WeaponRestrictionExtension modExtensiononThing = thing.def.GetModExtension<WeaponRestrictionExtension>();

            if (modExtensiononThing == null || modExtensiononThing.weaponcanEquipbyGenes.NullOrEmpty())
            {
                return false;
            }

            if (modExtensiononThing.weaponcanEquipbyGenes.NullOrEmpty() || modExtensiononThing.weaponcanEquipbyGenes != null)
                foreach (GeneDef allowedGenes in modExtensiononThing.weaponcanEquipbyGenes)
                {
                    if (allowedGenes == candidateGene.def)
                    {
                        return true;
                    }
                }

            return false;
        }

        private static bool CheckRaceAllowance(Thing thing, Pawn pawn)
        {
            WeaponRestrictionExtension modExtensiononThing = thing.def.GetModExtension<WeaponRestrictionExtension>();

            if (modExtensiononThing == null || modExtensiononThing.allowedRaces.NullOrEmpty())
            {
                return true;
            }

            if (modExtensiononThing.allowedRaces != null)
                foreach (ThingDef race in modExtensiononThing.allowedRaces)
                {
                    if (race == pawn.kindDef.race)
                    {
                        return true;
                    }
                }

            return false;
        }

        private static bool CheckTraitAllowance(Thing thing, Pawn pawn)
        {
            WeaponRestrictionExtension modExtensiononThing = thing.def.GetModExtension<WeaponRestrictionExtension>();

            if (modExtensiononThing == null || modExtensiononThing.requiredTraits.NullOrEmpty())
            {
                return true;
            }

            if (modExtensiononThing.requiredTraits != null && pawn.story?.traits != null && pawn.story.traits.allTraits.Any(x => modExtensiononThing.requiredTraits.Contains(x.def)))
            {
                return true;
            }

            return false;
        }

        private static bool CheckHediffRequirement(Thing thing, Pawn pawn)
        {
            WeaponRestrictionExtension modExtensiononThing = thing.def.GetModExtension<WeaponRestrictionExtension>();

            if (modExtensiononThing == null || modExtensiononThing.requiredHediffDefs.NullOrEmpty())
            {
                return true;
            }

            if (modExtensiononThing.requiredHediffDefs != null && pawn.health.hediffSet.hediffs.Any(x => modExtensiononThing.requiredHediffDefs.Contains(x.def)))
            {
                return true;
            }

            return false;
        }
    }
}