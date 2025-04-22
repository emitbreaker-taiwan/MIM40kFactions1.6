using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace MIM40kFactions
{

    public class Comp_TargetableEMNC_Necron : CompTargetable
    {
        protected override bool PlayerChoosesTarget => true;

        protected override TargetingParameters GetTargetingParameters() => new TargetingParameters()
        {
            canTargetPawns = true,
            canTargetBuildings = true,
            canTargetItems = true,
            mapObjectTargetsMustBeAutoAttackable = false,
            validator = (x => x.Thing is Corpse && TargetValidator((Corpse)x.Thing))
        };

        public override IEnumerable<Thing> GetTargets(Thing targetChosenByPlayer = null)
        {
            yield return targetChosenByPlayer;
        }

        private bool TargetValidator(Corpse t)
        {           
            Pawn pawn = t?.InnerPawn ?? null;
            return pawn != null && pawn.def.HasModExtension<EMNC_Necron_ValidatiorExtension>();
        }
    }

}
