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
    public class JobDriver_LaySpore : JobDriver
    {
        private int LayEgg = 100;

        private const TargetIndex LaySpotOrEggBoxInd = TargetIndex.A;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
            yield return Toils_General.Wait(100);
            yield return Toils_General.Do(delegate
            {
                Thing thing = pawn.GetComp<CompSporeLayer>().ProduceSpore();
                GenPlace.TryPlaceThing(thing, pawn.Position, pawn.Map, ThingPlaceMode.Near, delegate (Thing t, int i)
                {
                    if (pawn.Faction != Faction.OfPlayer)
                    {
                        t.SetForbidden(value: true);
                    }
                });
            });
        }
    }
}
