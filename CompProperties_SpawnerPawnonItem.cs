using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class CompProperties_UseEffect_SpawnerPawnonItem : CompProperties_UseEffect
    {
        public PawnKindDef pawnKind;
        public int amount = 1;
        public FactionDef forcedFaction;
        public bool usePlayerFaction = true;

        public CompProperties_UseEffect_SpawnerPawnonItem() => this.compClass = typeof(CompUseEffect_SpawnerPawn);
    }
}
