using RimWorld;
using Verse;

namespace MIM40kFactions.Orks
{
    public class JobDriver_Teef : JobDriver_GatherAnimalBodyResources
    {
        protected override float WorkTotal => 400f;

        protected override CompHasGatherableBodyResource GetComp(Pawn animal)
        {
            return animal.TryGetComp<CompTeef>();
        }
    }
}