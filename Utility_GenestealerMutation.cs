﻿using RimWorld;
using Verse;

namespace MIM40kFactions.GenestealerCult
{
    public class Utility_GenestealerMutation
    {
        private static float mutationSeed;
        public static void DoMutationConsideration(Pawn EMGC_victim, Faction faction = null)
        {
            if (ModsConfig.BiotechActive && Utility_DependencyManager.IsGCCoreActive())
            {
                if (EMGC_victim.HasPsylink)
                {
                    EMGC_victim.kindDef = PawnKindDef.Named("EMGC_Mutation_Magus");
                    EMGC_victim.genes.SetXenotype(Utility_XenotypeManager.XenotypeDefNamed("EMGC_Magus"));
                    return;
                }

                Hediff firstGen = EMGC_victim.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("EMGC_GSKiss_FirstGen"));
                Hediff secondGen = EMGC_victim.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("EMGC_GSKiss_SecondGen"));
                Hediff thirdGen = EMGC_victim.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("EMGC_GSKiss_ThirdGen"));
                Hediff forthGen = EMGC_victim.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("EMGC_GSKiss_ForthGen"));

                if (firstGen != null)
                {
                    EMGC_victim.kindDef = PawnKindDef.Named("EMGC_Mutation_BroodBrother");
                    EMGC_victim.genes.SetXenotype(Utility_XenotypeManager.XenotypeDefNamed("EMGC_BroodBrothers"));
                    return;
                }

                if (secondGen != null)
                {
                    mutationSeed = Rand.Range(0f, 100f);

                    if (mutationSeed > 89f)
                    {
                        EMGC_victim.kindDef = PawnKindDef.Named("EMGC_Mutation_AberrantHypermorph");
                        EMGC_victim.genes.SetXenotype(Utility_XenotypeManager.XenotypeDefNamed("EMGC_AberrantHypermorph"));
                        return;
                    }

                    if (mutationSeed > 74f)
                    {
                        EMGC_victim.kindDef = PawnKindDef.Named("EMGC_Mutation_Aberrant");
                        EMGC_victim.genes.SetXenotype(Utility_XenotypeManager.XenotypeDefNamed("EMGC_Aberrant"));
                        return;
                    }

                    if (mutationSeed > 54f)
                    {
                        EMGC_victim.kindDef = PawnKindDef.Named("EMGC_Mutation_AcolyteIconward");
                        EMGC_victim.genes.SetXenotype(Utility_XenotypeManager.XenotypeDefNamed("EMGC_AcolyteIconward"));
                        return;
                    }

                    else
                    {
                        EMGC_victim.kindDef = PawnKindDef.Named("EMGC_Mutation_Acolyte");
                        EMGC_victim.genes.SetXenotype(Utility_XenotypeManager.XenotypeDefNamed("EMGC_AcolyteHybrids"));
                        return;
                    }
                }

                if (thirdGen != null)
                {
                    EMGC_victim.kindDef = PawnKindDef.Named("EMGC_Mutation_Neophyte");
                    EMGC_victim.genes.SetXenotype(Utility_XenotypeManager.XenotypeDefNamed("EMGC_NeophyteHybrids"));
                    return;
                }

                if (forthGen != null)
                {
                    mutationSeed = Rand.Range(10f, 60f);

                    Faction targetFaction = EMGC_victim.Faction;
                    if (faction != null)
                    {
                        targetFaction = faction;
                    }

                    if (mutationSeed > 50f)
                    {
                        EMGC_victim.apparel.DropAllOrMoveAllToInventory();

                        Utility_NonHumanlikeMutation.RespawnHumanlike(EMGC_victim, PawnKindDef.Named("EMGC_Mutation_GenestealerPatriarch"), targetFaction, Utility_XenotypeManager.XenotypeDefNamed("EMGC_Patriarch"), null, EMGC_victim.ideo.Ideo, EMGC_victim.story.Childhood, Utility_BackstoryManagement.Named("EMGC_Adulthood_Patriarch"));
                        return;
                    }
                    else
                    {
                        EMGC_victim.apparel.DropAllOrMoveAllToInventory();
                        Utility_NonHumanlikeMutation.RespawnHumanlike(EMGC_victim, PawnKindDef.Named("EMGC_Mutation_PurestrainGenestealer"), targetFaction, Utility_XenotypeManager.XenotypeDefNamed("EMGC_PurestrainGenestealer"), null, EMGC_victim.ideo.Ideo, EMGC_victim.story.Childhood, Utility_BackstoryManagement.Named("EMGC_Adulthood_Purestrain"));
                        return;
                    }
                }

                else
                {
                    Log.Error("Victim has no genestealer mutation hediff in props");
                    return;
                }
            }
        }
        public static void DoIdeoConsideration(Pawn EMGC_victim, Faction faction = null)
        {
            if (faction != null)
            {
                EMGC_victim.ideo.SetIdeo(faction.ideos.PrimaryIdeo);
            }
            else
            {
                EMGC_victim.ideo.SetIdeo(Faction.OfPlayer.ideos.PrimaryIdeo);
            }
        }

        public static void DoPostAction(Pawn EMGC_victim, Faction faction = null)
        {
            EMGC_victim.Drawer.renderer.SetAllGraphicsDirty();
            if (faction != null)
            {
                EMGC_victim.SetFaction(faction);
            }
            else
            {
                EMGC_victim.SetFaction(Faction.OfPlayer);
            }

            RemoveTrigger(EMGC_victim);
        }

        public static void RemoveTrigger(Pawn EMGC_victim)
        {
            if (Utility_DependencyManager.IsGCCoreActive())
            {
                Hediff firstGen = EMGC_victim.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("EMGC_GSKiss_FirstGen"));
                Hediff secondGen = EMGC_victim.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("EMGC_GSKiss_SecondGen"));
                Hediff thirdGen = EMGC_victim.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("EMGC_GSKiss_ThirdGen"));
                Hediff forthGen = EMGC_victim.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("EMGC_GSKiss_ForthGen"));

                if (firstGen != null)
                {
                    EMGC_victim.health.RemoveHediff(firstGen);
                    return;
                }

                if (secondGen != null)
                {
                    EMGC_victim.health.RemoveHediff(secondGen);
                    return;
                }

                if (thirdGen != null)
                {
                    EMGC_victim.health.RemoveHediff(thirdGen);
                    return;
                }

                if (forthGen != null)
                {
                    EMGC_victim.health.RemoveHediff(forthGen);
                    return;
                }

                else
                {
                    Log.Error("Victim has no genestealer mutation hediff in props");
                    return;
                }
            }
        }
    }
}