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
    public class JobDriver_CastAbilityGoToNear : JobDriver_CastVerbOnce
    {        
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);

            Toil toil = ToilMaker.MakeToil("MakeNewToils");
            toil.initAction = delegate
            {
                pawn.pather.StopDead();
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return toil;

            Ability ability = ((Verb_CastAbility)job.verbToUse).ability;
            Pawn Caster = ability.pawn;
            Verb Verb = job.verbToUse;
            LocalTargetInfo target = new LocalTargetInfo(Verb.CurrentTarget.Thing);
            IntVec3 targetCell = target.Cell;
            IntVec3 casterPosition = Caster.Position;
            if (casterPosition.x != 0)
                casterPosition.x = casterPosition.x / 2;
            if (casterPosition.y != 0)
                casterPosition.y = casterPosition.y / 2;
            if (casterPosition.z != 0)
                casterPosition.z = casterPosition.z / 2;
            bool gotoFlag = false;
            List<IntVec3> list = GenRadial.RadialCellsAround(casterPosition, ability.def.verbProperties.range, true).OfType<IntVec3>().ToList();
            for (int i = 0; i < list.Count; i++)
            {
                if (targetCell == list[i])
                    gotoFlag = true;
            }

            if (gotoFlag)
            {
                targetCell.x = targetCell.x - casterPosition.x;
                targetCell.y = targetCell.y - casterPosition.y;
                targetCell.z = targetCell.z - casterPosition.z;
                yield return GotoCell(targetCell, PathEndMode.OnCell);
            }

            Psycast psycast = new Psycast(pawn, ability.def);

            yield return CastVerb(TargetIndex.A, TargetIndex.B, ability, psycast, Verb, canHitNonTargetPawns: false);
        }
        private static Toil GotoCell(IntVec3 cell, PathEndMode peMode)
        {
            Toil toil = ToilMaker.MakeToil("GotoCell");
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                if (actor.Position == cell)
                {
                    actor.jobs.curDriver.ReadyForNextToil();
                }
                else
                {
                    actor.pather.StartPath(cell, peMode);
                }
            };
            toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            return toil;
        }
        private static Toil CastVerb(TargetIndex targetInd, TargetIndex destInd, Ability ability, Psycast psycast, Verb Verb, bool canHitNonTargetPawns = true)
        {
            Toil toil = ToilMaker.MakeToil("CastVerb");
            toil.initAction = delegate
            {
                LocalTargetInfo target = toil.actor.jobs.curJob.GetTarget(targetInd);
                LocalTargetInfo destTarg = ((destInd != 0) ? toil.actor.jobs.curJob.GetTarget(destInd) : LocalTargetInfo.Invalid);
                if (ability != null && ability.def.showCastingProgressBar)
                {
                    if (ability.def.IsPsycast && psycast.CanApplyPsycastTo(target) && psycast.CanCast)
                    {
                        psycast.Activate(target, destTarg);
                    }
                    toil.WithProgressBar(TargetIndex.A, () => Verb.WarmupProgress);
                }
                toil.actor.jobs.curJob.verbToUse.TryStartCastOn(target, destTarg, surpriseAttack: false, canHitNonTargetPawns, toil.actor.jobs.curJob.preventFriendlyFire);
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            toil.activeSkill = () => Toils_Combat.GetActiveSkillForToil(toil);
            return toil;
        }
    }
}
