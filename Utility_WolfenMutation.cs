using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions
{
    public class Utility_WolfenMutation
    {
        private static readonly Random rand = new Random();
        public static void DoMutationConsideration(Pawn EMSM_SWvictim)
        {          
            Faction homeFaction = EMSM_SWvictim.HomeFaction;
            BackstoryDef childhood = EMSM_SWvictim.story.Childhood;
            BackstoryDef adulthood = EMSM_SWvictim.story.Adulthood;

            if (EMSM_SWvictim.HasPsylink)
            {
                DoMakeSpaceWolves(EMSM_SWvictim, homeFaction, childhood, adulthood);
                return;
            }

            if (EMSM_SWvictim.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("EMSM_SWGeneSeedWulfen")) != null)
            {
                DoMakeWulfen(EMSM_SWvictim, homeFaction, childhood, adulthood);
                return;
            }

            int mutationSeed = rand.Next(1, 100);

            if (mutationSeed > 90)
            {
                DoMakeWulfen(EMSM_SWvictim, homeFaction, childhood, adulthood);
                return;
            }
            if (mutationSeed > 60)
            {
                DoMakeLongFang(EMSM_SWvictim, homeFaction, childhood, adulthood);
                return;
            }
            if (mutationSeed > 0)
            {
                DoMakeSpaceWolves(EMSM_SWvictim, homeFaction, childhood, adulthood);
                return;
            }
            else
            {
                Log.Error("Pawn has no Space Wolves Gene-Seed in props");
                return;
            }
        }
        private static void DoMakeSpaceWolves(Pawn EMSM_SWvictim, Faction homeFaction, BackstoryDef childhood, BackstoryDef adulthood)
        {
            if (ModsConfig.BiotechActive == true)
                EMSM_SWvictim.genes.SetXenotype(Utility_XenotypeManager.XenotypeDefNamed("EMSM_Astartes_SpaceWolves"));

            EMSM_SWvictim.kindDef = PawnKindDef.Named("EMSM_Mutation_SpaceWolves");
            SetPawnBaseStats(EMSM_SWvictim, homeFaction, childhood, adulthood);

            BodyPartRecord targetPart = EMSM_SWvictim.health.hediffSet.GetNotMissingParts().FirstOrDefault(part => part.def == BodyPartDefOf.Torso);
            EMSM_SWvictim.health.AddHediff(HediffDef.Named("EMSM_SpaceWolves_CanisHelix"), targetPart);
            return;
        }
        private static void DoMakeWulfen(Pawn EMSM_SWvictim, Faction homeFaction, BackstoryDef childhood, BackstoryDef adulthood)
        {
            if (ModsConfig.BiotechActive)
            {
                EMSM_SWvictim.genes.SetXenotype(Utility_XenotypeManager.XenotypeDefNamed("EMSM_Astartes_Wulfen"));
            }

            if (!Utility_DependencyManager.IsSpaceWolvesActive() || homeFaction == Faction.OfPlayer)
                EMSM_SWvictim.kindDef = PawnKindDef.Named("EMSM_Mutation_Wulfen");
            else
                EMSM_SWvictim.kindDef = PawnKindDef.Named("EMSM_Mutation_Wulfen");

            SetPawnBaseStats(EMSM_SWvictim, homeFaction, childhood, adulthood);
            SetCosmetics(EMSM_SWvictim);

            BodyPartRecord targetPart = EMSM_SWvictim.health.hediffSet.GetNotMissingParts().FirstOrDefault(part => part.def == BodyPartDefOf.Torso);
            EMSM_SWvictim.health.AddHediff(HediffDef.Named("EMSM_SpaceWolves_CanisHelix"), targetPart);
            if (EMSM_SWvictim.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("EMSM_SpaceWolves_CanisHelix")) != null)
            {
                HealthUtility.AdjustSeverity(EMSM_SWvictim, HediffDef.Named("EMSM_SpaceWolves_CanisHelix"), 1f);
            }
            return;
        }
        private static void DoMakeLongFang(Pawn EMSM_SWvictim, Faction homeFaction, BackstoryDef childhood, BackstoryDef adulthood)
        {
            if (ModsConfig.BiotechActive == true)
                EMSM_SWvictim.genes.SetXenotype(Utility_XenotypeManager.XenotypeDefNamed("EMSM_Astartes_LongFang"));

            EMSM_SWvictim.kindDef = PawnKindDef.Named("EMSM_Mutation_Longfang");
            SetPawnBaseStats(EMSM_SWvictim, homeFaction, childhood, adulthood);

            BodyPartRecord targetPart = EMSM_SWvictim.health.hediffSet.GetNotMissingParts().FirstOrDefault(part => part.def == BodyPartDefOf.Torso);
            EMSM_SWvictim.health.AddHediff(HediffDef.Named("EMSM_SpaceWolves_CanisHelix"), targetPart);
            if (EMSM_SWvictim.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("EMSM_SpaceWolves_CanisHelix")) != null)
            {
                HealthUtility.AdjustSeverity(EMSM_SWvictim, HediffDef.Named("EMSM_SpaceWolves_CanisHelix"), 1f);
            }
            return;
        }
        private static void SetPawnBaseStats(Pawn EMSM_SWvictim, Faction homeFaction, BackstoryDef childhood, BackstoryDef adulthood)
        {
            if (EMSM_SWvictim.Faction != homeFaction)
                EMSM_SWvictim.SetFaction(homeFaction);
            if (EMSM_SWvictim.story.Childhood != childhood)
                EMSM_SWvictim.story.Childhood = childhood;
            if (EMSM_SWvictim.story.Adulthood != adulthood)
                EMSM_SWvictim.story.Adulthood = adulthood;
        }
        private static void SetCosmetics(Pawn EMSM_SWvictim)
        {
            EMSM_SWvictim.story.hairDef = HairDefOf.Bald;
            EMSM_SWvictim.style.beardDef = BeardDefOf.NoBeard;
        }
        public static void DoIdeoConsideration(Pawn EMSM_SWvictim)
        {
            if (EMSM_SWvictim.ideo.Ideo != null)
            {
                Ideo ideo = EMSM_SWvictim.ideo.Ideo;
                EMSM_SWvictim.ideo.SetIdeo(ideo);
            }
            else
            {
                EMSM_SWvictim.ideo.SetIdeo(Faction.OfPlayer.ideos.PrimaryIdeo);
            }
            return;
        }
        public static void DoPostAction(Pawn EMSM_SWvictim)
        {
            BodySnatcherExtension modExtension = EMSM_SWvictim.kindDef.GetModExtension<BodySnatcherExtension>();
            if (modExtension != null)
            {
                EMSM_SWvictim.story.TryGetRandomHeadFromSet((IEnumerable<HeadTypeDef>)Utility_GeneManager.GeneDefNamed("EMSM_Jaw_WulfenHead").forcedHeadTypes);
            }
            EMSM_SWvictim.Drawer.renderer.SetAllGraphicsDirty();
        }
    }
}
