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
    public class Utility_NonHumanlikeMutation
    {
        public static void RespawnHumanlike(Pawn mechanoid, PawnKindDef pawnKind, Faction faction, XenotypeDef xenotype, HediffDef mutationHediffDef = null, Ideo ideo = null, BackstoryDef childhood = null, BackstoryDef adulthood = null)
        {
            if (pawnKind == null)
            {
                Log.Warning("PawnKind is not set. Automatically set as Colonist.");
                pawnKind = PawnKindDefOf.Colonist;
            }
            if (faction == null)
            {
                Log.Warning("Faction is not set. Automatically set as Hostile Ancient.");
                faction = Faction.OfAncientsHostile;
            }
            if (ModsConfig.IdeologyActive && ideo == null)
            {
                Log.Warning("Ideo is not set. Automatically set as Hostile Ancient.");
                ideo = Faction.OfAncientsHostile.ideos.PrimaryIdeo;
            }
            if (!ModsConfig.IdeologyActive)
            {
                ideo = null;
            }
            PawnGenerationRequest request = new PawnGenerationRequest
            (
                pawnKind,
                faction,
                PawnGenerationContext.NonPlayer,
                -1,
                forceGenerateNewPawn: true,
                allowDead: false,
                allowDowned: false,
                canGeneratePawnRelations: false,
                true,
                1f,
                forceAddFreeWarmLayerIfNeeded: false,
                allowGay: true,
                allowPregnant: true,
                allowFood: true,
                allowAddictions: true,
                inhabitant: false,
                certainlyBeenInCryptosleep: false,
                forceRedressWorldPawnIfFormerColonist: false,
                worldPawnFactionDoesntMatter: false,
                0.0f,
                0.0f,
                null,
                1f,
                null,
                null,
                null,
                null,
                new float?(),
                new float?(),
                new float?(),
                new Gender?(),
                null,
                null,
                null,
                ideo,
                false,
                false,
                false,
                false,
                null,
                null,
                xenotype,
                null,
                null,
                1f,
                DevelopmentalStage.Adult,
                null,
                null,
                null,
                false
            );

            IntVec3 position = mechanoid.Position;
            Map map = mechanoid.Map;
            mechanoid.Destroy();

            Pawn pawn = PawnGenerator.GeneratePawn(request);
            if (pawn != null)
            {
                if (childhood != null)
                {
                    pawn.story.Childhood = childhood;
                }
                if (adulthood != null)
                {
                    pawn.story.Adulthood = adulthood;
                }

                GenPlace.TryPlaceThing(pawn, position, map, ThingPlaceMode.Near);
                if (mutationHediffDef == null)
                {
                    return;
                }
                Hediff mutationHediff = pawn.health.hediffSet.GetFirstHediffOfDef(mutationHediffDef);
                if (mutationHediff != null)
                {
                    pawn.health.RemoveHediff(mutationHediff);
                }
            }
            if (pawn == null)
            {
                return;
            }
        }
    }
}