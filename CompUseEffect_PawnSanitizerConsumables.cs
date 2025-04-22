using MIM40kFactions.Compatibility;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions
{
    public class CompUseEffect_PawnSanitizerConsumables : CompUseEffect
    {
        public CompProperties_PawnSanitizerConsumables Props => (CompProperties_PawnSanitizerConsumables)this.props;

        public override void DoEffect(Pawn user)
        {
            base.DoEffect(user);

            if (user != null && !user.Dead)
            {
                var tracker = SanitizerTrackerRegistry.Instance.Consumable;
                if (!tracker.IsAlreadyApplied(user, parent))
                {
                    Utility_ThoughtBlockerRegistry.ApplySanitizerRulesSafe(user, Props);
                    Utility_ThoughtBlockerRegistry.RegisterSanitizerSource(user, $"Consumable:{parent.def.defName}");
                    tracker.Register(user, parent);
                }
            }
        }
    }
}
