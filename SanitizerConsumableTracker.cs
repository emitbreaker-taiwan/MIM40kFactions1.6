using MIM40kFactions.Compatibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions
{
    public class SanitizerConsumableTracker : ISanitizerTracker, IExposable
    {
        private HashSet<string> data = new HashSet<string>(); // Format: "pawnID-defName"

        public bool IsAlreadyApplied(Pawn pawn, object source)
        {
            if (!(source is Thing thing))
            {
                Log.Warning($"[Sanitizer] ConsumableTracker received invalid source: {source?.GetType().Name}");
                return false;
            }
            return data.Contains(Key(pawn, thing));
            //return source is Thing thing && data.Contains(Key(pawn, thing));
        }

        public void Register(Pawn pawn, object source)
        {
            if (!(source is Thing thing))
            {
                Log.Warning($"[Sanitizer] ConsumableTracker tried to register invalid source: {source?.GetType().Name}");
                return;
            }
            data.Add(Key(pawn, thing));
            //if (source is Thing thing)
            //    data.Add(Key(pawn, thing));
        }

        public void Unregister(Pawn pawn, object source)
        {
            if (!(source is Thing thing)) return;
            data.Remove(Key(pawn, thing));
            //if (source is Thing thing)
            //    data.Remove(Key(pawn, thing));
        }

        private string Key(Pawn p, Thing t) => $"{p.ThingID}-{t.def.defName}";

        public void ClearForPawn(Pawn pawn)
        {
            string prefix = pawn.ThingID + "-";
            data.RemoveWhere(entry => entry.StartsWith(prefix));
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref data, "sanitizerConsumableTracker", LookMode.Value);
        }
    }
}
