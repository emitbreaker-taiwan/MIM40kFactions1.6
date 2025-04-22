using MIM40kFactions.Compatibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions
{
    public class SanitizerRaceTracker : ISanitizerTracker, IExposable
    {
        private HashSet<string> data = new HashSet<string>(); // "pawnID-Race:defName"

        public bool IsAlreadyApplied(Pawn pawn, object source)
        {
            return source is string id && data.Contains(Key(pawn, id));
        }

        public void Register(Pawn pawn, object source)
        {
            if (source is string id)
                data.Add(Key(pawn, id));
        }

        public void Unregister(Pawn pawn, object source)
        {
            if (source is string id)
                data.Remove(Key(pawn, id));
        }

        private string Key(Pawn pawn, string id) => $"{pawn.ThingID}-{id}";

        public void ClearForPawn(Pawn pawn)
        {
            string prefix = pawn.ThingID + "-";
            data.RemoveWhere(d => d.StartsWith(prefix));
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref data, "sanitizerRaceTracker", LookMode.Value);
        }
    }
}
