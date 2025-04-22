using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace MIM40kFactions
{
    public class Utility_GetModExtension
    {
        public static BodySnatcherExtension GetBodySnatcherExtension(Pawn pawn)
        {
            if (pawn == null || pawn.IsDessicated() || !pawn.RaceProps.Humanlike)
            {
                return null;
            }

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

        public static MassCarriedExtension GetMassCarriedExtension(Pawn pawn)
        {
            if (pawn == null || pawn.IsDessicated())
                return null;

            if (!pawn.RaceProps?.Humanlike ?? true)
                return null;

            MassCarriedExtension modExtension = pawn.def.GetModExtension<MassCarriedExtension>();

            if (modExtension == null && pawn.kindDef != null)
            {
                modExtension = pawn.kindDef.GetModExtension<MassCarriedExtension>();
            }

            if (ModsConfig.BiotechActive && modExtension == null && pawn.genes.GenesListForReading != null)
            {
                foreach (Gene gene in pawn.genes.GenesListForReading)
                {
                    if (gene.Active && gene.def.GetModExtension<MassCarriedExtension>() != null)
                    {
                        modExtension = gene.def.GetModExtension<MassCarriedExtension>();
                    }
                }
            }

            if ((modExtension?.useClothing ?? true) && pawn.apparel?.WornApparel != null)
            {
                foreach (Thing apparel in pawn.apparel.WornApparel)
                {
                    if (apparel.def.GetModExtension<MassCarriedExtension>() != null)
                    {
                        modExtension = apparel.def.GetModExtension<MassCarriedExtension>();
                    }
                }
            }

            return modExtension;
        }

        public static ChangeBodyTypeExtension GetChangeBodyTypeExtension(Pawn pawn)
        {
            ChangeBodyTypeExtension modExtension = new ChangeBodyTypeExtension();
            if (pawn.apparel.WornApparel != null)
            {
                foreach (Thing apparel in pawn.apparel.WornApparel)
                {
                    if (apparel.def.GetModExtension<ChangeBodyTypeExtension>() != null)
                    {
                        modExtension = apparel.def.GetModExtension<ChangeBodyTypeExtension>();
                    }
                }
            }
            return modExtension;
        }
    }
}
