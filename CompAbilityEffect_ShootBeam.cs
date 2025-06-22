using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MIM40kFactions
{
    public class CompAbilityEffect_ShootBeam : CompAbilityEffect
    {
        private List<Vector3> path = new List<Vector3>();
        private List<Vector3> tmpPath = new List<Vector3>();
        private HashSet<IntVec3> pathCells = new HashSet<IntVec3>();
        private HashSet<IntVec3> tmpPathCells = new HashSet<IntVec3>();
        private MoteDualAttached mote;
        private Effecter endEffecter;
        private Sustainer sustainer;

        private Vector3 initialTargetPosition;
        private int burstShotsLeft;
        private int ticksToNextBurstShot;

        private IncineratorSpray sprayer;
        private ThingDef incineratorMoteDef;
        private ThingDef incineratorSprayThingDef;

        private CompProperties_AbilityShootBeam p => (CompProperties_AbilityShootBeam)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            // Initialize arc spray if enabled
            if (p.isArcSprayAbility)
            {
                if (p.incineratorSprayThingDef != null)
                {
                    incineratorSprayThingDef = p.incineratorSprayThingDef;
                }
                sprayer = GenSpawn.Spawn(incineratorSprayThingDef, parent.pawn.Position, parent.pawn.Map) as IncineratorSpray;
            }

            // Initialize burst shots
            burstShotsLeft = p.burstShotCount;
            ticksToNextBurstShot = p.ticksBetweenBurstShots;
            initialTargetPosition = target.CenterVector3;

            // Calculate the beam path
            CalculatePath(initialTargetPosition, path, pathCells);

            // Create the beam mote
            if (p.beamMoteDef != null)
            {
                mote = MoteMaker.MakeInteractionOverlay(p.beamMoteDef, parent.pawn, new TargetInfo(path[0].ToIntVec3(), parent.pawn.Map));
            }

            // Play sound
            if (p.soundCastBeam != null)
            {
                sustainer = p.soundCastBeam.TrySpawnSustainer(SoundInfo.InMap(parent.pawn, MaintenanceType.PerTick));
            }

            // Process each burst shot
            for (int i = 0; i < p.burstShotCount; i++)
            {
                if (p.isArcSprayAbility)
                {
                    TryCastShotArcSpray(target);
                }
                else
                {
                    TryCastShot(target);
                }
            }

            // Cleanup effects
            endEffecter?.Cleanup();
            if (sustainer != null && !sustainer.Ended)
            {
                sustainer.End();
            }
        }

        private void TryCastShotArcSpray(LocalTargetInfo target)
        {
            if (p.incineratorMoteDef != null)
            {
                incineratorMoteDef = p.incineratorMoteDef;
            }

            TryCastShot(target);
            Vector3 vector = target.CenterVector3; // Assuming target.CenterVector3 is the intended position
            vector.y = 0; // Flatten the y-coordinate to zero

            IntVec3 intVec = vector.ToIntVec3();
            Vector3 drawPos = parent.pawn.DrawPos;
            Vector3 normalized = (vector - drawPos).normalized;
            drawPos += normalized * p.beamStartOffset;

            MoteDualAttached moteDualAttached = MoteMaker.MakeInteractionOverlay(
                A: new TargetInfo(parent.pawn.Position, parent.pawn.Map),
                moteDef: incineratorMoteDef,
                B: new TargetInfo(intVec, parent.pawn.Map)
            );

            float distance = Vector3.Distance(vector, drawPos);
            float scaleFactor = (distance < p.beamStartOffset) ? 0.5f : 1f;

            if (sprayer != null)
            {
                sprayer.Add(new IncineratorProjectileMotion
                {
                    mote = moteDualAttached,
                    targetDest = intVec,
                    worldSource = drawPos,
                    worldTarget = vector,
                    moveVector = (vector - drawPos).normalized,
                    startScale = 1f * scaleFactor,
                    endScale = (1f + Rand.Range(0.1f, 0.4f)) * scaleFactor,
                    lifespanTicks = Mathf.FloorToInt(distance * p.beamFleckChancePerTick)
                });
            }
        }

        private void TryCastShot(LocalTargetInfo target)
        {
            // Determine the hit cell
            List<IntVec3> affectedCells = GetAffectedCells(target.CenterVector3.ToIntVec3());

            // Apply damage to all affected cells
            foreach (IntVec3 cell in affectedCells)
            {
                ApplyDamage(target, cell);
            }

            // Update visual effects
            if (mote != null)
            {
                mote.Maintain();
            }
        }

        private List<IntVec3> GetAffectedCells(IntVec3 targetCell)
        {
            List<IntVec3> affectedCells = new List<IntVec3>();
            Vector3 direction = (targetCell.ToVector3() - parent.pawn.Position.ToVector3Shifted()).normalized;

            for (int i = 0; i < p.burstShotCount; i++)
            {
                Vector3 point = parent.pawn.Position.ToVector3Shifted() + direction * (i * p.beamWidth);
                IntVec3 cell = point.ToIntVec3();

                if (cell.InBounds(parent.pawn.Map) && (!cell.Filled(parent.pawn.Map) || p.isPenetration))
                {
                    affectedCells.Add(cell);
                }
            }

            return affectedCells;
        }

        private void ApplyDamage(LocalTargetInfo target, IntVec3 hitCell)
        {
            Thing thing = hitCell.GetFirstThing<Thing>(parent.pawn.Map);
            if (thing != null && p.beamDamageDef != null)
            {
                DamageInfo dinfo = new DamageInfo(p.beamDamageDef, p.damAmount, p.beamDamageDef.defaultArmorPenetration, -1, parent.pawn);
                thing.TakeDamage(dinfo);
            }
        }

        private void CalculatePath(Vector3 target, List<Vector3> pathList, HashSet<IntVec3> pathCellsList)
        {
            pathList.Clear();
            pathCellsList.Clear();

            Vector3 direction = (target - parent.pawn.Position.ToVector3Shifted()).normalized;
            float distance = (target - parent.pawn.Position.ToVector3Shifted()).magnitude;

            for (int i = 0; i < p.burstShotCount; i++)
            {
                Vector3 point = parent.pawn.Position.ToVector3Shifted() + direction * (distance * i / p.burstShotCount);
                pathList.Add(point);
                pathCellsList.Add(point.ToIntVec3());
            }
        }

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            // Highlight the beam path
            tmpPath.Clear();
            tmpPathCells.Clear();
            CalculatePath(target.CenterVector3, tmpPath, tmpPathCells);

            GenDraw.DrawFieldEdges(tmpPathCells.ToList<IntVec3>(), p.highlightColor);
        }
    }
}