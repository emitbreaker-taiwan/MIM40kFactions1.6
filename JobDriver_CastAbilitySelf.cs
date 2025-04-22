using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using Verse.AI;

namespace MIM40kFactions
{
    public class JobDriver_CastAbilitySelf : JobDriver_CastAbility
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOn(() => !job.ability.CanCast && job.ability.def.charges > 1);

            Toil toil = ToilMaker.MakeToil("MakeNewToils");
            toil.initAction = delegate
            {
                pawn.pather.StopDead();
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return toil;

            Ability ability = ((Verb_CastAbility)job.verbToUse).ability;
            Psycast psycast = new Psycast(pawn, ability.def);
            LocalTargetInfo selfTarget = new LocalTargetInfo(ability.pawn);

            Toil toil2 = ToilMaker.MakeToil("CastVerb");
            toil2.initAction = delegate
            {
                ability.CanApplyOn(selfTarget);
                toil2.actor.jobs.curJob.verbToUse.TryStartCastOn(selfTarget, selfTarget.Cell, false, false, toil2.actor.jobs.curJob.preventFriendlyFire, true);
            };
            toil2.defaultCompleteMode = ToilCompleteMode.Instant;
            if (ability.def.IsPsycast && psycast.CanApplyPsycastTo(selfTarget) && psycast.CanCast)
            {
                psycast.Activate(selfTarget, selfTarget.Cell);
            }
            yield return toil2;
        }
    }
}
