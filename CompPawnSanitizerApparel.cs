using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions
{
    public class CompPawnSanitizerApparel : ThingComp
    {
        public CompProperties_PawnSanitizerApparel Props => (CompProperties_PawnSanitizerApparel)this.props;
        private Pawn wearer;

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            wearer = pawn;

            var tracker = SanitizerTrackerRegistry.Instance.Apparel;
            if (!tracker.IsAlreadyApplied(pawn, parent))
            {
                Utility_ThoughtBlockerRegistry.ApplySanitizerRulesSafe(pawn, Props);
                Utility_ThoughtBlockerRegistry.RegisterSanitizerSource(pawn, $"Apparel:{parent.def.defName}");
                tracker.Register(pawn, parent);
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            if (wearer == pawn)
            {
                Utility_ThoughtBlockerRegistry.RestoreSanitizerEffectsFromSource(pawn, $"Apparel:{parent.def.defName}");
                SanitizerTrackerRegistry.Instance.Apparel.Unregister(pawn, parent);
                wearer = null;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref wearer, "wearer");
        }
    }
}