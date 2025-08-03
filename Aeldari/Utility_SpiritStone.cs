using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions.Aeldari
{
    public static class Utility_SpiritStone
    {
        public static CompSpiritStone TryGetComp(Thing thing)
        {
            return thing?.TryGetComp<CompSpiritStone>();
        }

        public static string GetStoredPawnName(Thing thing)
        {
            var comp = TryGetComp(thing);
            return comp?.pawnNameFull ?? "Unknown";
        }

        public static Pawn GetStoredPawn(Thing thing)
        {
            return TryGetComp(thing)?.GetOriginalPawn();
        }

        public static bool HasStoredPawn(Thing thing)
        {
            return TryGetComp(thing)?.pawnNameFull != null;
        }
    }
}
