using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace MIM40kFactions
{
    public static class Utility_GeneRestriction
    {
        public static bool CanHaveNecronGene(GeneDef gene, ThingDef thing)
        {
            EMNC_Necron_ValidatiorExtension modExtension = thing.GetModExtension<EMNC_Necron_ValidatiorExtension>();

            if (modExtension == null && gene == Utility_GeneDefManagement.Named("EMNC_Biotransference"))
                return false;

            return true;
        }

        public static bool CanAddGenetoPawnKind(GeneDef gene, Pawn pawn)
        {
            GeneRestrictionExtension modExtension = pawn.kindDef.GetModExtension<GeneRestrictionExtension>();

            if (modExtension == null)
            {
                return true;
            }

            if (modExtension.allowedGeneDefs == null)
            {
                Log.Error("At least one allowed gene requires to avoid error.");
                return true;
            }

            if (modExtension.allowedGeneDefs != null)
            {
                for (int i = 0; i < modExtension.allowedGeneDefs.Count; i++)
                {
                    if (gene == modExtension.allowedGeneDefs[i])
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}