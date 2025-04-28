using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace MIM40kFactions
{
    public class Hediff_Mutation_Tzaangor : HediffWithComps
    {
        public override void Tick()
        {
            if (!ModsConfig.IsActive("emitbreaker.MIM.WH40k.CSM.TS"))
                return;
            ++ageTicks;
            if ((double)Severity < 0.99)
                return;
            if (pawn.RaceProps.Humanlike)
            {
                if (pawn.story?.headType == Utility_HeadTypeDefManagement.Named("EMNC_Invisible") || pawn.story?.headType == Utility_HeadTypeDefManagement.Named("EMWH_Invisible") || pawn.RaceProps.IsAnomalyEntity)
                    DoRespawn(pawn);
                else 
                    DoMutation(pawn);
            }                
            if (!pawn.RaceProps.Humanlike)
            {
                DoRespawn(pawn);
            }
            else
            {
                Log.Error("Something went wrong with Pawn Race.");
            }
        }
        
        private static void DoMutation(Pawn EMTS_victim)
        {
            if (EMTS_victim == null)
                return;
            if(ModsConfig.IsActive("emitbreaker.MIM.WH40k.CSM.TS"))
            {
                EMTS_victim.kindDef = DoPawnKindConsideration(EMTS_victim);

                if (ModsConfig.BiotechActive)
                {
                    EMTS_victim.genes.SetXenotype(DoXenotypeConsideration(EMTS_victim));
                    if (ModsConfig.RoyaltyActive && EMTS_victim.kindDef == PawnKindDef.Named("EMTS_Mutation_TzaangorShaman"))
                        ChangePsylinkLevel(EMTS_victim, false);

                }

                Hediff mutationHediff = EMTS_victim.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("EMTS_Mutation_Tzaangor"));
                EMTS_victim.health.RemoveHediff(mutationHediff);
            }
        }

        private static void DoRespawn(Pawn EMTS_victim)
        {
            Faction faction = Faction.OfAncientsHostile;
            Ideo ideo = faction.ideos.PrimaryIdeo;

            if (EMTS_victim.Faction != null && !EMTS_victim.RaceProps.IsAnomalyEntity && !EMTS_victim.RaceProps.IsMechanoid)
            {
                faction = EMTS_victim.Faction;
                ideo = EMTS_victim.Faction.ideos.PrimaryIdeo;
            }

            if (ModsConfig.IdeologyActive && ModsConfig.IsActive("emitbreaker.MIM.WH40k.CSM.TS"))
            {
                Utility_NonHumanlikeMutation.RespawnHumanlike(EMTS_victim, DoPawnKindConsideration(EMTS_victim), faction, DoXenotypeConsideration(EMTS_victim), HediffDef.Named("EMTS_Mutation_Tzaangor"), ideo);
                return;
            }
            if (!ModsConfig.IdeologyActive && ModsConfig.IsActive("emitbreaker.MIM.WH40k.CSM.TS"))
            {
                Utility_NonHumanlikeMutation.RespawnHumanlike(EMTS_victim, DoPawnKindConsideration(EMTS_victim), faction, DoXenotypeConsideration(EMTS_victim), HediffDef.Named("EMTS_Mutation_Tzaangor"));
                return;
            }
        }

        private static PawnKindDef DoPawnKindConsideration(Pawn EMTS_victim, PawnKindDef mutationkindDef = null)
        {
            if (ModsConfig.IsActive("emitbreaker.MIM.WH40k.CSM.TS"))
            {
                if (ModsConfig.RoyaltyActive == true)
                {
                    if (PawnUtility.GetPsylinkLevel(EMTS_victim) <= 0)
                    {
                        mutationkindDef = PawnKindDef.Named("EMTS_Mutation_Tzaangor");
                    }
                    else if (PawnUtility.GetPsylinkLevel(EMTS_victim) > 2)
                    {
                        mutationkindDef = PawnKindDef.Named("EMTS_Mutation_TzaangorShaman");
                    }
                    else
                    {
                        mutationkindDef = PawnKindDef.Named("EMTS_Mutation_TzaangorEnlightened");
                    }
                }
                else
                {
                    mutationkindDef = PawnKindDef.Named("EMTS_Mutation_Tzaangor");
                }
            }
            return mutationkindDef;
        }

        private static XenotypeDef DoXenotypeConsideration(Pawn EMTS_victim, XenotypeDef mutationXenotype = null)
        {
            if (ModsConfig.IsActive("emitbreaker.MIM.WH40k.CSM.TS"))
            {
                if (ModsConfig.RoyaltyActive == true)
                {
                    if (PawnUtility.GetPsylinkLevel(EMTS_victim) <= 0)
                    {
                        mutationXenotype = Utility_XenotypeManager.XenotypeDefNamed("EMTS_Tzaangor");
                    }
                    else if (PawnUtility.GetPsylinkLevel(EMTS_victim) > 2)
                    {
                        mutationXenotype = Utility_XenotypeManager.XenotypeDefNamed("EMTS_TzaangorShaman");
                    }
                    else
                    {
                        mutationXenotype = Utility_XenotypeManager.XenotypeDefNamed("EMTS_TzaangorEnlightened");
                    }
                }
                else
                {
                    mutationXenotype = Utility_XenotypeManager.XenotypeDefNamed("EMTS_Tzaangor");
                }
            }
            return mutationXenotype;
        }

        private static void ChangePsylinkLevel(Pawn EMTS_victim, bool sendLetter)
        {
            if (ModsConfig.RoyaltyActive == true)
            {
                Hediff_Psylink mainPsylinkSource = EMTS_victim.GetMainPsylinkSource();
                if (mainPsylinkSource == null)
                {
                    Hediff_Psylink hediffPsylink = (Hediff_Psylink)HediffMaker.MakeHediff(HediffDefOf.PsychicAmplifier, EMTS_victim);
                    try
                    {
                        hediffPsylink.suppressPostAddLetter = !sendLetter;
                        EMTS_victim.health.AddHediff(hediffPsylink, EMTS_victim.health.hediffSet.GetBrain());
                    }
                    finally
                    {
                        hediffPsylink.suppressPostAddLetter = false;
                    }
                    mainPsylinkSource = EMTS_victim.GetMainPsylinkSource();
                    mainPsylinkSource.ChangeLevel(4, sendLetter);
                }
                else
                    mainPsylinkSource.ChangeLevel(4, sendLetter);
            }
        }
    }
}
