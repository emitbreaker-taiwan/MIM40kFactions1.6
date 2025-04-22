using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace MIM40kFactions
{
    public class CompUseEffect_SpawnerPawn : CompUseEffect
    {
        public override float OrderPriority => 1000f;

        public CompProperties_UseEffect_SpawnerPawnonItem PawnSpawnerProps => props as CompProperties_UseEffect_SpawnerPawnonItem;

        public void PawnNecron()
        {
            PawnGenerationRequest request = new PawnGenerationRequest
                (
                    PawnSpawnerProps.pawnKind,
                    Faction.OfPlayer,
                    PawnGenerationContext.NonPlayer,
                    -1,
                    false,
                    false,
                    false,
                    false,
                    true,
                    1f,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
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
                    Faction.OfPlayer.ideos.PrimaryIdeo,
                    false,
                    false,
                    false,
                    false,
                    null,
                    null,
                    Utility_XenotypeManagement.Named("EMNC_Necrons"),
                    null,
                    null,
                    999f,
                    DevelopmentalStage.Adult,
                    null,
                    null,
                    null,
                    false
                );
            Pawn pawn = PawnGenerator.GeneratePawn(request);
            if (pawn != null)
            {
                GenPlace.TryPlaceThing(pawn, parent.Position, parent.Map, ThingPlaceMode.Near);
            }
            if (pawn == null)
                return;
        }

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            for (int index = 0; index < PawnSpawnerProps.amount; ++index)
                PawnNecron();
        }
    }
}