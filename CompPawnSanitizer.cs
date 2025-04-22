using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions
{
    public class CompPawnSanitizer : ThingComp
    {
        public CompProperties_PawnSanitizer Props => (CompProperties_PawnSanitizer)this.props;

        private bool applied;

        public override void CompTick()
        {
            base.CompTick();

            if (applied || parent == null || parent.Destroyed || !(parent is Pawn pawn))
                return;

            if (pawn.def?.race != null)
            {
                if (!Props.onlyForHumanlike || (Props.onlyForHumanlike && pawn.RaceProps?.Humanlike == true))
                {
                    Utility_ThoughtBlockerRegistry.ApplySanitizerRulesSafe(pawn, Props);

                    // ✅ Optional: register race source
                    SanitizerTrackerRegistry.Instance.Race.Register(pawn, "Race:" + pawn.def.defName);

                    applied = true;
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref applied, "sanitizerApplied", false);
        }
    }
}