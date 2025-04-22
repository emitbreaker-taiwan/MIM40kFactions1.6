using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using RimWorld.Planet;
using Verse.AI;
using Verse.Sound;
using System.Diagnostics;
using UnityEngine;

namespace MIM40kFactions
{
    public class JobDriver_CastAbilityPsycast : JobDriver_CastAbility
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
            LocalTargetInfo target = new LocalTargetInfo(ability.verb.CurrentTarget.Thing);
            LocalTargetInfo dest = new LocalTargetInfo(ability.verb.CurrentDestination.Thing);
            Pawn actor = pawn;

            Toil toilCastVerb = Toils_Combat.CastVerb(TargetIndex.A, TargetIndex.B, canHitNonTargetPawns: false);
            if (ability != null && ability.def.showCastingProgressBar)
            {
                if (ability.def.IsPsycast && psycast.CanApplyPsycastTo(target) && psycast.CanCast)
                {
                    psycast.Activate(target, dest);
                }
                toilCastVerb.WithProgressBar(TargetIndex.A, () => job.verbToUse.WarmupProgress);
            }
            yield return toilCastVerb;
        }
        private float DistanceCalculator(IntVec3 a, IntVec3 b)
        {
            float dx = Mathf.Abs(b.x - a.x);
            float dy = Mathf.Abs(b.y - a.y);
            float dz = Mathf.Abs(b.z - a.z);
            return dx + dy + dz;
        }
    }
}
