using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions
{
    public interface ISanitizerTracker
    {
        bool IsAlreadyApplied(Pawn pawn, object source);
        void Register(Pawn pawn, object source);
        void Unregister(Pawn pawn, object source);
        void ClearForPawn(Pawn pawn); // <- NEW
    }
}