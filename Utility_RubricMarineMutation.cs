using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class Utility_RubricMarineMutation
    {
        public static void DoMutationConsideration(Pawn EMCM_TSvictim)
        {
            Faction homeFaction = EMCM_TSvictim.HomeFaction;
            BackstoryDef childhood = EMCM_TSvictim.story.Childhood;
            BackstoryDef adulthood = EMCM_TSvictim.story.Adulthood;

            if (ModsConfig.RoyaltyActive && EMCM_TSvictim.HasPsylink)
            {
                DoMakeSorcerer(EMCM_TSvictim, homeFaction, childhood, adulthood);
                return;
            }

            if (EMCM_TSvictim.story.traits.HasTrait(TraitDef.Named("PsychicSensitivity"), 2) || EMCM_TSvictim.story.traits.HasTrait(TraitDef.Named("PsychicSensitivity"), 1))
            {
                DoMakeSorcerer(EMCM_TSvictim, homeFaction, childhood, adulthood);
                return;
            }

            Hediff tsGeneseed1 = EMCM_TSvictim.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("EMCM_TSGeneSeed"));
            Hediff tsGeneseed2 = EMCM_TSvictim.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("EMCM_TSGeneSeedNPC"));

            if (tsGeneseed1 != null || tsGeneseed2 != null)
            {
                System.Random rand = new System.Random();

                if (EMCM_TSvictim.story.traits.HasTrait(TraitDef.Named("PsychicSensitivity"), -1) || EMCM_TSvictim.story.traits.HasTrait(TraitDef.Named("PsychicSensitivity"), -2))
                {
                    DoMakeRubricae(EMCM_TSvictim, homeFaction, childhood, adulthood);
                    return;
                }

                int mutationSeed = rand.Next(1, 100);

                if (mutationSeed > 99)
                {
                    DoMakeSorcerer(EMCM_TSvictim, homeFaction, childhood, adulthood);
                    return;
                }

                if (mutationSeed > 0)
                {
                    DoMakeRubricae(EMCM_TSvictim, homeFaction, childhood, adulthood);
                    return;
                }
            }
            else
            {
                Log.Error("Pawn has no Thousand Sons Gene-Seed in props");
                return;
            }
        }
        private static void DoMakeSorcerer(Pawn EMCM_TSvictim, Faction homeFaction, BackstoryDef childhood, BackstoryDef adulthood)
        {
            if (ModsConfig.BiotechActive == true)
                EMCM_TSvictim.genes.SetXenotype(Utility_XenotypeManager.XenotypeDefNamed("EMCM_HereticAstartes_ThousandSons"));

            EMCM_TSvictim.kindDef = PawnKindDef.Named("EMTS_Mutation_Sorcerer");
            SetPawnBaseStats(EMCM_TSvictim, homeFaction, childhood, adulthood);

            if (ModsConfig.RoyaltyActive == true)
                ChangePsylinkLevel(EMCM_TSvictim, false);

            BodyPartRecord targetPart = EMCM_TSvictim.health.hediffSet.GetBrain();
            EMCM_TSvictim.health.AddHediff(HediffDef.Named("EMTS_WarpPotential"), targetPart);
            return;
        }
        private static void DoMakeRubricae(Pawn EMCM_TSvictim, Faction homeFaction, BackstoryDef childhood, BackstoryDef adulthood)
        {
            if (ModsConfig.BiotechActive == true)
                EMCM_TSvictim.genes.SetXenotype(Utility_XenotypeManager.XenotypeDefNamed("EMCM_HereticAstartes"));

            EMCM_TSvictim.kindDef = PawnKindDef.Named("EMTS_Mutation_RubricMarine");
            SetPawnBaseStats(EMCM_TSvictim, homeFaction, childhood, adulthood);
            SetCosmetics(EMCM_TSvictim);

            BodyPartRecord targetPart = EMCM_TSvictim.health.hediffSet.GetNotMissingParts().FirstOrDefault(part => part.def == BodyPartDefOf.Torso);
            EMCM_TSvictim.health.AddHediff(HediffDef.Named("EMTS_RubricofAhriman"), targetPart);
            return;
        }
        private static void SetPawnBaseStats(Pawn EMCM_TSvictim, Faction homeFaction, BackstoryDef childhood, BackstoryDef adulthood)
        {
            if (EMCM_TSvictim.Faction != homeFaction)
                EMCM_TSvictim.SetFaction(homeFaction);
            if (EMCM_TSvictim.story.Childhood != childhood)
                EMCM_TSvictim.story.Childhood = childhood;
            if (EMCM_TSvictim.story.Adulthood != adulthood)
                EMCM_TSvictim.story.Adulthood = adulthood;
        }
        private static void SetCosmetics(Pawn EMCM_TSvictim)
        {
            EMCM_TSvictim.story.hairDef = HairDefOf.Bald;
            EMCM_TSvictim.style.beardDef = BeardDefOf.NoBeard;
            EMCM_TSvictim.style.FaceTattoo = TattooDefOf.NoTattoo_Face;
            EMCM_TSvictim.style.BodyTattoo = TattooDefOf.NoTattoo_Body;
        }
        public static void DoIdeoConsideration(Pawn EMCM_TSvictim)
        {
            if (EMCM_TSvictim.ideo.Ideo != null)
            {
                Ideo ideo = EMCM_TSvictim.ideo.Ideo;
                EMCM_TSvictim.ideo.SetIdeo(ideo);
            }
            else
            {
                EMCM_TSvictim.ideo.SetIdeo(Faction.OfPlayer.ideos.PrimaryIdeo);
            }
            return;
        }
        private static void ChangePsylinkLevel(Pawn EMCM_TSvictim, bool sendLetter)
        {
            if (ModsConfig.RoyaltyActive)
            {
                Hediff_Psylink mainPsylinkSource = EMCM_TSvictim.GetMainPsylinkSource();
                if (mainPsylinkSource == null)
                {
                    Hediff_Psylink hediffPsylink = (Hediff_Psylink)HediffMaker.MakeHediff(HediffDefOf.PsychicAmplifier, EMCM_TSvictim);
                    try
                    {
                        hediffPsylink.suppressPostAddLetter = !sendLetter;
                        EMCM_TSvictim.health.AddHediff(hediffPsylink, EMCM_TSvictim.health.hediffSet.GetBrain());
                    }
                    finally
                    {
                        hediffPsylink.suppressPostAddLetter = false;
                    }
                    mainPsylinkSource = EMCM_TSvictim.GetMainPsylinkSource();
                    mainPsylinkSource.ChangeLevel(6, sendLetter);
                }
                else
                    mainPsylinkSource.ChangeLevel(6, sendLetter);
            }
        }
        public static void DoPostAction(Pawn EMCM_TSvictim)
        {
            BodySnatcherExtension modExtension = EMCM_TSvictim.kindDef.GetModExtension<BodySnatcherExtension>();
            if (modExtension != null)
            {
                EMCM_TSvictim.story.bodyType.bodyNakedGraphicPath = modExtension.pathBody;
                EMCM_TSvictim.story.headType = modExtension.headTypeDef;
                EMCM_TSvictim.Drawer.renderer.SetAllGraphicsDirty();
            }
        }
    }
}