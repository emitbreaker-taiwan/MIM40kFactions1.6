using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class Utility_GeneManager
    {
        public static GeneDef GeneDefNamed(string defName)
        {
            return DefDatabase<GeneDef>.GetNamed(defName);
        }

        /// <summary>
        /// Validates if the pawn has the required gene defined in the modExtension.
        /// </summary>
        public static bool GeneValidator(Pawn pawn, BodySnatcherExtension modExtension)
        {
            if (!ModsConfig.BiotechActive || modExtension?.requiredGene == null)
            {
                return true;
            }

            if (pawn.genes?.GenesListForReading == null)
            {
                Log.Warning($"{pawn.LabelShort} has no gene.");
                return false;
            }

            foreach (Gene gene in pawn.genes.GenesListForReading)
            {
                if (gene.Active && gene.def == modExtension.requiredGene)
                {
                    return true; // ✅ Early exit: found matching gene
                }
            }

            // ✅ Only warn if validation failed after full check
            Log.Warning($"Selected pawn ({pawn.LabelShort ?? "unknown"}) does not have the required gene: {modExtension.requiredGene.defName}");
            return false;
        }

    }
}
