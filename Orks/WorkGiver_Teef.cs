using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace MIM40kFactions.Orks
{
    public class WorkGiver_Teef : WorkGiver_GatherAnimalBodyResources
    {
        protected override JobDef JobDef => Utility_JobDefManagement.Named("EMOK_Teef");

        protected override CompHasGatherableBodyResource GetComp(Pawn animal)
        {
            return animal.TryGetComp<CompTeef>();
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            List<Pawn> list = pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction);
            for (int i = 0; i < list.Count; i++)
            {
                CompHasGatherableBodyResource comp = GetComp(list[i]);
                if (comp != null && comp.ActiveAndFull)
                {
                    return false;
                }
            }

            return true;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Pawn pawn2) || t == pawn)
            {
                return false;
            }

            CompHasGatherableBodyResource comp = GetComp(pawn2);
            if (comp == null || !comp.ActiveAndFull || pawn2.Downed || (pawn2.roping != null && pawn2.roping.IsRopedByPawn) || !pawn2.CanCasuallyInteractNow() || !pawn.CanReserve(pawn2, 1, -1, null, forced))
            {
                return false;
            }

            return true;
        }
    }
}