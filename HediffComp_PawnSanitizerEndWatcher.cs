using MIM40kFactions.Compatibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions
{
    public class HediffComp_PawnSanitizerEndWatcher : HediffComp
    {
        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();

            if (Pawn != null && !Pawn.Destroyed)
            {
                Utility_ThoughtBlockerRegistry.RestoreSanitizerEffects(Pawn);

                // 🔄 Optional: unregister from tracker if consumable tracking was used
                var tracker = SanitizerTrackerRegistry.Instance.Consumable;
                if (tracker.IsAlreadyApplied(Pawn, parent))
                {
                    tracker.Unregister(Pawn, parent);
                }
            }
        }
    }
}
