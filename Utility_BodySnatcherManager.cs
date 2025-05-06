using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace MIM40kFactions
{
    public static class Utility_BodySnatcherManager
    {
        public static bool Initialized { get; private set; }

        /// <summary>
        /// Initializes default values for BodySnatcherExtension to avoid premature null states.
        /// </summary>
        public static void EnsureInitialized()
        {
            if (Initialized) return;

            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                BodySnatcherExtension modExtension = def.GetModExtension<BodySnatcherExtension>();
                if (modExtension != null)
                {
                    if (modExtension.drawSize == default) modExtension.drawSize = Vector2.one;
                    if (modExtension.headdrawSize == default) modExtension.headdrawSize = Vector2.one;
                    // Optional: Validate any other properties
                }
            }

            Initialized = true;
        }

        public static BodySnatcherExtension GetBodySnatcherExtension(Pawn pawn)
        {
            if (!Utility_PawnValidationManager.IsNotDessicatedHumanlikePawn(pawn))
            {
                return null;
            }

            EnsureInitialized();

            BodySnatcherExtension modExtension = pawn.def.GetModExtension<BodySnatcherExtension>();

            if (modExtension == null && pawn.kindDef != null)
            {
                modExtension = pawn.kindDef.GetModExtension<BodySnatcherExtension>();
            }

            if (ModsConfig.BiotechActive && modExtension == null && pawn.genes.GenesListForReading != null)
            {
                foreach (Gene gene in pawn.genes.GenesListForReading)
                {
                    if (gene.Active && gene.def.GetModExtension<BodySnatcherExtension>() != null)
                    {
                        modExtension = gene.def.GetModExtension<BodySnatcherExtension>();
                    }
                }
            }

            if ((modExtension == null || (modExtension != null && modExtension.useClothing)) && pawn.apparel.WornApparel != null)
            {
                foreach (Thing apparel in pawn.apparel.WornApparel)
                {
                    if (apparel.def.GetModExtension<BodySnatcherExtension>() != null)
                    {
                        modExtension = apparel.def.GetModExtension<BodySnatcherExtension>();
                    }
                }
            }

            return modExtension;
        }

        /// <summary>
        /// Gets safe scaling multiplier for a pawn's part based on BodySnatcherExtension.
        /// </summary>
        public static float GetScaling(Pawn pawn, bool isHeadPart)
        {
            if (!Utility_PawnValidationManager.IsNotDessicatedHumanlikePawn(pawn))
            {
                return 1f;
            }

            BodySnatcherExtension modExtension = GetBodySnatcherExtension(pawn);
            if (modExtension == null || !Utility_GeneManager.GeneValidator(pawn, modExtension))
            {
                return 1f;
            }

            float scaling = isHeadPart
                ? (modExtension.headdrawSize.x > 0f ? modExtension.headdrawSize.x : 1f)
                : (modExtension.drawSize.x > 0f ? modExtension.drawSize.x : 1f);

            return scaling;
        }
    }
}
