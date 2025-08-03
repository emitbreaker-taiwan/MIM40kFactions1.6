using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;

namespace MIM40kFactions.Orks
{
    public class JobGiver_LaySpore : ThinkNode_JobGiver
    {
        private const float LayRadius = 5f;

        private const float MaxSearchRadius = 30f;

        private const int MinSearchRegions = 10;

        private JobDef laySporeJobDef;

        protected override Job TryGiveJob(Pawn pawn)
        {
            CompSporeLayer compSporeLayer = pawn.TryGetComp<CompSporeLayer>();
            if (compSporeLayer == null || !compSporeLayer.CanLayNow)
            {
                return null;
            }

            ThingDef singleDef = compSporeLayer.NextSporeType();
            PathEndMode peMode = PathEndMode.OnCell;
            TraverseParms traverseParms = TraverseParms.For(pawn, Danger.Some);

            Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(singleDef), peMode, traverseParms, 30f, (Thing x) => pawn.GetRoom() == null || x.GetRoom() == pawn.GetRoom());
            if (laySporeJobDef == null)
            {
                laySporeJobDef = DefDatabase<JobDef>.GetNamed("EMOK_LaySpore");
            }
            return JobMaker.MakeJob(laySporeJobDef, thing?.Position ?? RCellFinder.RandomWanderDestFor(pawn, pawn.Position, 5f, null, Danger.Some));
        }
    }
}
