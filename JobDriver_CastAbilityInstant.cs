using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;

namespace MIM40kFactions
{
    public class JobDriver_CastAbilityInstant : JobDriver_CastAbility
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOn(() => !job.ability.CanCast && !job.ability.Casting);

            Toil toil = ToilMaker.MakeToil("MakeNewToils");
            toil.initAction = delegate
            {
                pawn.pather.StopDead();
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return toil;

            Toil toil2 = ToilMaker.MakeToil("CastVerb");
            toil2.initAction = delegate
            {
                LocalTargetInfo target = toil2.actor.jobs.curJob.GetTarget(TargetIndex.A);
                LocalTargetInfo destTarg = ((TargetIndex.B != 0) ? toil2.actor.jobs.curJob.GetTarget(TargetIndex.B) : LocalTargetInfo.Invalid);

                if (CanDeepStrikeSelectedTargetAt(destTarg, toil2.actor))
                    toil2.actor.jobs.curJob.verbToUse.TryStartCastOn(target, destTarg, surpriseAttack: false, false, toil2.actor.jobs.curJob.preventFriendlyFire);
            };
            toil2.defaultCompleteMode = ToilCompleteMode.Instant;
            if (job.ability != null && job.ability.def.showCastingProgressBar && job.verbToUse != null)
            {
                toil2.WithProgressBar(TargetIndex.A, () => job.verbToUse.WarmupProgress);
            }
            yield return toil2;
            yield return Toils_Combat.GotoCastPosition(TargetIndex.A, TargetIndex.B);
            yield return Toils_Combat.CastVerb(TargetIndex.A);
            this.FailOn(() => !CanDeepStrikeSelectedTargetAt(toil2.actor.jobs.curJob.GetTarget(TargetIndex.B), toil2.actor));
        }

        private bool CanDeepStrikeSelectedTargetAt(LocalTargetInfo destTarg, Pawn pawn)
        {
            IReadOnlyList<Pawn> readOnlyList = pawn.Map.mapPawns.AllPawnsSpawned;
            List<Thing> hostiles = new List<Thing>();

            foreach (Pawn hostilePawns in readOnlyList)
            {
                if (hostilePawns.HomeFaction.HostileTo(pawn.Faction))
                    hostiles.Add(hostilePawns);
            }

            Pawn selectedTarget = destTarg.Pawn;
            if (selectedTarget != null)
            {
                if (selectedTarget.Spawned && !destTarg.Cell.Impassable(pawn.Map))
                {
                    foreach (Thing thing in hostiles)
                    {
                        if ((double)DeepStrikeCalculator(thing.Position, destTarg.Cell) <= 13.1)
                        {
                            return false;
                        }
                    }
                    return destTarg.Cell.WalkableBy(pawn.Map, pawn);
                }
                return false;
            }

            return CanTeleportThingTo(destTarg, pawn.Map);
        }

        private float DeepStrikeCalculator(IntVec3 a, IntVec3 b)
        {
            float dx = Mathf.Abs(b.x - a.x);
            float dy = Mathf.Abs(b.y - a.y);
            float dz = Mathf.Abs(b.z - a.z);
            return dx + dy + dz;
        }

        private static bool CanTeleportThingTo(LocalTargetInfo target, Map map)
        {
            Building edifice = target.Cell.GetEdifice(map);
            Building_Door building_Door;
            if (edifice != null && edifice.def.surfaceType != SurfaceType.Item && edifice.def.surfaceType != SurfaceType.Eat && ((building_Door = edifice as Building_Door) == null || !building_Door.Open))
            {
                return false;
            }

            List<Thing> thingList = target.Cell.GetThingList(map);
            for (int i = 0; i < thingList.Count; i++)
            {
                if (thingList[i].def.category == ThingCategory.Item)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
