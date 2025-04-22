using MIM40kFactions.Compatibility;
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
    public class HediffComp_PawnSanitizer : HediffComp
    {
        public HediffCompProperties_PawnSanitizer Props => (HediffCompProperties_PawnSanitizer)this.props;
        private bool applied;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);

            if (applied || Pawn == null || Props == null)
                return;

            if (Props.onlyForHumanlike && !Pawn.RaceProps?.Humanlike == true)
                return;

            Utility_ThoughtBlockerRegistry.ApplySanitizerRulesSafe(Pawn, Props);
            Utility_ThoughtBlockerRegistry.RegisterSanitizerSource(Pawn, $"Hediff:{parent.def.defName}");
            SanitizerTrackerRegistry.Instance.Race.Register(Pawn, $"Hediff:{parent.def.defName}");

            applied = true;
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();

            if (Pawn != null && !Pawn.Destroyed)
            {
                Utility_ThoughtBlockerRegistry.RestoreSanitizerEffectsFromSource(Pawn, $"Hediff:{parent.def.defName}");
                SanitizerTrackerRegistry.Instance.Race.Unregister(Pawn, $"Hediff:{parent.def.defName}");
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref applied, "sanitizerHediffApplied", false);
        }
    }
}