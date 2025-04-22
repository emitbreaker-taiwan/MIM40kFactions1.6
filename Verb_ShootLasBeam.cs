using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse.Sound;
using Verse;
using Verse.Noise;
using static UnityEngine.GraphicsBuffer;
using System.Reflection;
using System.IO;
using static HarmonyLib.Code;

namespace MIM40kFactions
{
    public class Verb_ShootLasBeam : Verb
    {
        private List<Vector3> path = new List<Vector3>();

        private List<Vector3> tmpPath = new List<Vector3>();

        private int ticksToNextPathStep;

        private Vector3 initialTargetPosition;

        private MoteDualAttached mote;

        private Effecter endEffecter;

        private Sustainer sustainer;

        protected HashSet<IntVec3> pathCells = new HashSet<IntVec3>();

        private HashSet<IntVec3> tmpPathCells = new HashSet<IntVec3>();

        private HashSet<IntVec3> tmpHighlightCells = new HashSet<IntVec3>();

        private HashSet<IntVec3> tmpSecondaryHighlightCells = new HashSet<IntVec3>();

        private List<IntVec3> hitCells = new List<IntVec3>();

        private List<IntVec3> tmpHitCells = new List<IntVec3>();

        private const int NumSubdivisionsPerUnitLength = 1;

        public float? piercingDamageFalloff;

        public bool? radialArcBlast;
        public float? radialArcRange;
        public float? radialArcFalloff;

        public int? chainMaxBounces;
        public float? chainBounceRange;
        public float? chainDamageFalloff; // e.g. 0.8f = 80% damage after each bounce

        protected override int ShotsPerBurst => Math.Max(2, verbProps.burstShotCount);

        public float ShotProgress => (float)ticksToNextPathStep / (float)verbProps.ticksBetweenBurstShots;

        public Vector3 InterpolatedPosition
        {
            get
            {
                Vector3 vector = currentTarget.CenterVector3 - initialTargetPosition;
                int indexA = Mathf.Clamp(burstShotsLeft, 0, path.Count - 1);
                int indexB = Mathf.Clamp(burstShotsLeft + 1, 0, path.Count - 1);
                return Vector3.Lerp(path[indexA], path[indexB], ShotProgress) + vector;
            }
        }

        protected float lineWidthEnd => verbProps.beamWidth;

        protected bool canHitFilledCells = true;

        private CompWeaponAbilities compWeaponAbilities => EquipmentSource.GetComp<CompWeaponAbilities>();

        public override void DrawHighlight(LocalTargetInfo target)
        {
            base.DrawHighlight(target);
            AffectedCells(target.Cell, tmpHitCells);
            foreach (IntVec3 tmpCell in tmpHitCells)
            {
                ShootLine resultingLine;
                bool flag = TryFindShootLineFromTo(caster.Position, target, out resultingLine);
                if ((verbProps.stopBurstWithoutLos && !flag) || !TryGetHitCell(resultingLine.Source, tmpCell, out var hitCell))
                {
                    continue;
                }

                tmpHighlightCells.Add(hitCell);

                if (!verbProps.beamHitsNeighborCells)
                {
                    continue;
                }

                foreach (IntVec3 beamHitNeighbourCell in GetBeamHitNeighbourCells(resultingLine.Source, hitCell))
                {
                    if (!tmpHighlightCells.Contains(beamHitNeighbourCell))
                    {
                        tmpSecondaryHighlightCells.Add(beamHitNeighbourCell);
                    }
                }
            }
            CalculatePath(target.CenterVector3, tmpPath, tmpPathCells, addRandomOffset: false);
            foreach (IntVec3 tmpPathCell in tmpPathCells)
            {
                ShootLine resultingLine;
                bool flag = TryFindShootLineFromTo(caster.Position, target, out resultingLine);
                if ((verbProps.stopBurstWithoutLos && !flag) || !TryGetHitCell(resultingLine.Source, tmpPathCell, out var hitCell))
                {
                    continue;
                }

                tmpHighlightCells.Add(hitCell);

                if (!verbProps.beamHitsNeighborCells)
                {
                    continue;
                }

                foreach (IntVec3 beamHitNeighbourCell in GetBeamHitNeighbourCells(resultingLine.Source, hitCell))
                {
                    if (!tmpHighlightCells.Contains(beamHitNeighbourCell))
                    {
                        tmpSecondaryHighlightCells.Add(beamHitNeighbourCell);
                    }
                }
            }

            tmpSecondaryHighlightCells.RemoveWhere((IntVec3 x) => tmpHighlightCells.Contains(x));
            if (tmpHighlightCells.Any())
            {
                GenDraw.DrawFieldEdges(tmpHighlightCells.ToList(), verbProps.highlightColor ?? Color.white);
            }

            if (tmpSecondaryHighlightCells.Any())
            {
                GenDraw.DrawFieldEdges(tmpSecondaryHighlightCells.ToList(), verbProps.secondaryHighlightColor ?? Color.white);
            }

            tmpHighlightCells.Clear();
            tmpSecondaryHighlightCells.Clear();
        }

        protected override bool TryCastShot()
        {
            if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map)
            {
                return false;
            }

            ShootLine resultingLine;
            bool flag = TryFindShootLineFromTo(caster.Position, currentTarget, out resultingLine);
            if (verbProps.stopBurstWithoutLos && !flag)
            {
                return false;
            }

            if (base.EquipmentSource != null)
            {
                base.EquipmentSource.GetComp<CompChangeableProjectile>()?.Notify_ProjectileLaunched();
                base.EquipmentSource.GetComp<CompApparelReloadable>()?.UsedOnce();
            }

            lastShotTick = Find.TickManager.TicksGame;
            ticksToNextPathStep = verbProps.ticksBetweenBurstShots;
            IntVec3 targetCell = InterpolatedPosition.Yto0().ToIntVec3();
            if (!TryGetHitCell(resultingLine.Source, targetCell, out var hitCell))
            {
                return true;
            }

            TargetCellCalatWarmupComplete();

            foreach (IntVec3 cell in hitCells)
            {
                HitCell(cell, resultingLine.Source);
            }
            if (verbProps.beamHitsNeighborCells)
            {
                hitCells.Add(hitCell);
                foreach (IntVec3 beamHitNeighbourCell in GetBeamHitNeighbourCells(resultingLine.Source, hitCell))
                {
                    if (!hitCells.Contains(beamHitNeighbourCell))
                    {
                        float damageFactor = (pathCells.Contains(beamHitNeighbourCell) ? 1f : 0.5f);
                        HitCell(beamHitNeighbourCell, resultingLine.Source, damageFactor);
                        hitCells.Add(beamHitNeighbourCell);
                    }
                }
                if (hitCells.Contains(caster.Position))
                {
                    hitCells.Remove(caster.Position);
                }
            }

            //if (CasterPawn != null)
            //{
            //    CasterPawn.records.Increment(RecordDefOf.ShotsFired);
            //}

            return true;
        }

        private void TargetCellCalatWarmupComplete()
        {
            if (verbProps.beamWidth > 1)
            {
                for (int i = 0; i < ShotsPerBurst; i++)
                {
                    AffectedCells(path[i].ToIntVec3(), hitCells);
                    //if (compWeaponAbilities != null && compWeaponAbilities.Props.isPenetration)
                    //{
                    //    hitCells.Add(path[i].ToIntVec3());
                    //}
                }
            }
            else
            {
                AffectedCells(initialTargetPosition.ToIntVec3(), hitCells);
            }
            if (hitCells.Contains(caster.Position))
            {
                hitCells.Remove(caster.Position);
            }
            if (!hitCells.Contains(currentTarget.Cell) && hitCells.Count > 1)
            {
                currentTarget = hitCells[hitCells.Count - 1];
            }
        }

        protected bool TryGetHitCell(IntVec3 source, IntVec3 targetCell, out IntVec3 hitCell)
        {
            IntVec3 intVec = GenSight.LastPointOnLineOfSight(source, targetCell, (IntVec3 c) => c.InBounds(caster.Map) && c.CanBeSeenOverFast(caster.Map), skipFirstCell: true);
            if (verbProps.beamCantHitWithinMinRange && intVec.DistanceTo(source) < verbProps.minRange)
            {
                hitCell = default(IntVec3);
                return false;
            }

            hitCell = (intVec.IsValid ? intVec : targetCell);
            return intVec.IsValid;
        }

        protected IntVec3 GetHitCell(IntVec3 source, IntVec3 targetCell)
        {
            TryGetHitCell(source, targetCell, out var hitCell);
            return hitCell;
        }

        protected IEnumerable<IntVec3> GetBeamHitNeighbourCells(IntVec3 source, IntVec3 pos)
        {
            if (!verbProps.beamHitsNeighborCells)
            {
                yield break;
            }

            for (int i = 0; i < 4; i++)
            {
                IntVec3 intVec = pos + GenAdj.CardinalDirections[i];
                if (intVec.InBounds(Caster.Map) && (!verbProps.beamHitsNeighborCellsRequiresLOS || GenSight.LineOfSight(source, intVec, caster.Map)))
                {
                    yield return intVec;
                }
            }
        }

        public override bool TryStartCastOn(LocalTargetInfo castTarg, LocalTargetInfo destTarg, bool surpriseAttack = false, bool canHitNonTargetPawns = true, bool preventFriendlyFire = false, bool nonInterruptingSelfCast = false)
        {
            return base.TryStartCastOn(verbProps.beamTargetsGround ? ((LocalTargetInfo)castTarg.Cell) : castTarg, destTarg, surpriseAttack, canHitNonTargetPawns, preventFriendlyFire, nonInterruptingSelfCast);
        }

        public override void BurstingTick()
        {
            Vector3 vector;
            Vector3 vector2;
            if (verbProps.beamWidth > 1)
            {
                ticksToNextPathStep--;
                vector = InterpolatedPosition;
                vector2 = InterpolatedPosition - caster.Position.ToVector3Shifted();
            }
            else
            {
                vector = currentTarget.CenterVector3;
                vector2 = currentTarget.CenterVector3 - caster.Position.ToVector3Shifted();
            }
            IntVec3 intVec = vector.ToIntVec3();
            float num = vector2.MagnitudeHorizontal();
            Vector3 normalized = vector2.Yto0().normalized;
            IntVec3 intVec2 = GenSight.LastPointOnLineOfSight(caster.Position, intVec, (IntVec3 c) => c.CanBeSeenOverFast(caster.Map), skipFirstCell: true);
            if (intVec2.IsValid)
            {
                num -= (intVec - intVec2).LengthHorizontal;
                vector = caster.Position.ToVector3Shifted() + normalized * num;
                intVec = vector.ToIntVec3();
            }

            Vector3 offsetA = normalized * verbProps.beamStartOffset;
            Vector3 vector3 = vector - intVec.ToVector3Shifted();
            if (mote != null)
            {
                mote.UpdateTargets(new TargetInfo(caster.Position, caster.Map), new TargetInfo(intVec, caster.Map), offsetA, vector3);
                mote.Maintain();
            }

            if (verbProps.beamGroundFleckDef != null && Rand.Chance(verbProps.beamFleckChancePerTick))
            {
                FleckMaker.Static(vector, caster.Map, verbProps.beamGroundFleckDef);
            }

            if (endEffecter == null && verbProps.beamEndEffecterDef != null)
            {
                endEffecter = verbProps.beamEndEffecterDef.Spawn(intVec, caster.Map, vector3);
            }

            if (endEffecter != null)
            {
                endEffecter.offset = vector3;
                endEffecter.EffectTick(new TargetInfo(intVec, caster.Map), TargetInfo.Invalid);
                endEffecter.ticksLeft--;
            }

            if (verbProps.beamLineFleckDef != null)
            {
                float num2 = 1f * num;
                for (int i = 0; (float)i < num2; i++)
                {
                    if (Rand.Chance(verbProps.beamLineFleckChanceCurve.Evaluate((float)i / num2)))
                    {
                        Vector3 vector4 = i * normalized - normalized * Rand.Value + normalized / 2f;
                        FleckMaker.Static(caster.Position.ToVector3Shifted() + vector4, caster.Map, verbProps.beamLineFleckDef);
                    }
                }
            }

            sustainer?.Maintain();
        }

        public override void WarmupComplete()
        {
            burstShotsLeft = ShotsPerBurst;
            state = VerbState.Bursting;
            initialTargetPosition = currentTarget.CenterVector3;
            CalculatePath(currentTarget.CenterVector3, path, pathCells);
            hitCells.Clear();
            if (verbProps.beamMoteDef != null)
            {
                //mote = MoteMaker.MakeInteractionOverlay(verbProps.beamMoteDef, caster, new TargetInfo(targetCells[0], caster.Map));
                mote = MoteMaker.MakeInteractionOverlay(verbProps.beamMoteDef, caster, new TargetInfo(path[0].ToIntVec3(), caster.Map));
            }

            TryCastNextBurstShot();
            ticksToNextPathStep = verbProps.ticksBetweenBurstShots;
            endEffecter?.Cleanup();
            if (verbProps.soundCastBeam != null)
            {
                sustainer = verbProps.soundCastBeam.TrySpawnSustainer(SoundInfo.InMap(caster, MaintenanceType.PerTick));
            }
        }

        private void CalculatePath(Vector3 target, List<Vector3> pathList, HashSet<IntVec3> pathCellsList, bool addRandomOffset = true)
        {
            pathList.Clear();
            Vector3 vector = (target - caster.Position.ToVector3Shifted()).Yto0();
            float magnitude = vector.magnitude;
            Vector3 normalized = vector.normalized;
            Vector3 vector2 = normalized.RotatedBy(-90f);
            float num = ((verbProps.beamFullWidthRange > 0f) ? Mathf.Min(magnitude / verbProps.beamFullWidthRange, 1f) : 1f);
            float num2 = 1f;
            if (verbProps.beamWidth > 1)
            {
                num2 = (verbProps.beamWidth + 1f) * num / (float)ShotsPerBurst;
            }
            Vector3 vector3 = target.Yto0() - vector2 * verbProps.beamWidth / 2f * num;
            pathList.Add(vector3);
            for (int i = 0; i < Mathf.Max(ShotsPerBurst, 2); i++)
            {
                Vector3 vector4 = normalized * (Rand.Value * verbProps.beamMaxDeviation) - normalized / 2f;
                Vector3 vector5 = Mathf.Sin(((float)i / (float)ShotsPerBurst + 0.5f) * (float)Math.PI * 57.29578f) * verbProps.beamCurvature * -normalized - normalized * verbProps.beamMaxDeviation / 2f;
                if (addRandomOffset)
                {
                    pathList.Add(vector3 + (vector4 + vector5) * num);
                }
                else
                {
                    pathList.Add(vector3 + vector5 * num);
                }

                vector3 += vector2 * num2;
            }

            //AffectedCells(target.ToIntVec3(), pathCellsList);

            pathCellsList.Clear();
            foreach (Vector3 path in pathList)
            {
                pathCellsList.Add(path.ToIntVec3());
            }
        }

        private bool CanHit(Thing thing)
        {
            if (!thing.Spawned)
            {
                return false;
            }

            return !CoverUtility.ThingCovered(thing, caster.Map);
        }

        protected virtual void HitCell(IntVec3 cell, IntVec3 sourceCell, float damageFactor = 1f)
        {
            if (cell.InBounds(caster.Map))
            {
                ApplyDamage(VerbUtility.ThingsToHit(cell, caster.Map, CanHit).RandomElementWithFallback(), sourceCell, damageFactor);
                if (verbProps.beamSetsGroundOnFire && Rand.Chance(verbProps.beamChanceToStartFire))
                {
                    FireUtility.TryStartFireIn(cell, caster.Map, 1f, caster);
                }
            }
        }

        //private HashSet<IntVec3> AffectedCells(IntVec3 target, HashSet<IntVec3> tmpCells)
        //{
        //    tmpCells.Clear();
        //    Vector3 vector = Caster.Position.ToVector3Shifted().Yto0();
        //    IntVec3 intVec = target.ClampInsideMap(Caster.Map);
        //    if (Caster.Position == intVec)
        //    {
        //        return tmpCells;
        //    }

        //    float lengthHorizontal = (intVec - Caster.Position).LengthHorizontal;
        //    float num = (float)(intVec.x - Caster.Position.x) / lengthHorizontal;
        //    float num2 = (float)(intVec.z - Caster.Position.z) / lengthHorizontal;
        //    intVec.x = Mathf.RoundToInt((float)Caster.Position.x + num * verbProps.range);
        //    intVec.z = Mathf.RoundToInt((float)Caster.Position.z + num2 * verbProps.range);
        //    float target2 = Vector3.SignedAngle(intVec.ToVector3Shifted().Yto0() - vector, Vector3.right, Vector3.up);
        //    float num3 = lineWidthEnd / 2f;
        //    float num4 = Mathf.Sqrt(Mathf.Pow((intVec - Caster.Position).LengthHorizontal, 2f) + Mathf.Pow(num3, 2f));
        //    float num5 = 57.29578f * Mathf.Asin(num3 / num4);
        //    int num6 = GenRadial.NumCellsInRadius(verbProps.range < GenRadial.MaxRadialPatternRadius ? verbProps.range : GenRadial.MaxRadialPatternRadius - 0.1f);
        //    for (int i = 0; i < num6; i++)
        //    {
        //        IntVec3 intVec2 = Caster.Position + GenRadial.RadialPattern[i];
        //        if (CanUseCell(intVec2) && Mathf.Abs(Mathf.DeltaAngle(Vector3.SignedAngle(intVec2.ToVector3Shifted().Yto0() - vector, Vector3.right, Vector3.up), target2)) <= num5)
        //        {
        //            tmpCells.Add(intVec2);
        //        }
        //    }

        //    List<IntVec3> list = GenSight.BresenhamCellsBetween(Caster.Position, target);
        //    for (int j = 0; j < list.Count; j++)
        //    {
        //        IntVec3 intVec3 = list[j];
        //        if (!tmpCells.Contains(intVec3) && CanUseCell(intVec3))
        //        {
        //            tmpCells.Add(intVec3);
        //        }
        //    }

        //    if (compWeaponAbilities == null || !compWeaponAbilities.Props.isPenetration)
        //    {
        //        HashSet<IntVec3> removeCells = new HashSet<IntVec3>();
        //        List<Pawn> pawnList = new List<Pawn>();
        //        foreach (IntVec3 cell in tmpCells)
        //        {
        //            if (cell.GetFirstPawn(Caster.Map) != null && cell.GetFirstPawn(Caster.Map) != CasterPawn)
        //            {
        //                pawnList.Add(cell.GetFirstPawn(Caster.Map));
        //            }
        //            if (pawnList.Count > 0)
        //            {
        //                if (cell != pawnList[0].Position && cell.GetFirstPawn(Caster.Map) != null)
        //                {
        //                    removeCells.Add(cell);
        //                }
        //                if (cell.GetFirstPawn(Caster.Map) == null && DistanceCalculator(Caster.Position, pawnList[0].Position) < DistanceCalculator(Caster.Position, cell))
        //                {
        //                    removeCells.Add(cell);
        //                }
        //                if (pawnList[0].Position != target)
        //                {
        //                    removeCells.Add(target);
        //                }
        //            }
        //        }

        //        foreach (IntVec3 cell in removeCells)
        //        {
        //            if (tmpCells.Contains(cell))
        //            {
        //                tmpCells.Remove(cell);
        //            }
        //        }
        //    }

        //    return tmpCells;

        //    bool CanUseCell(IntVec3 c)
        //    {
        //        if (!c.InBounds(Caster.Map))
        //        {
        //            return false;
        //        }

        //        if (c == Caster.Position)
        //        {
        //            return false;
        //        }

        //        if (!canHitFilledCells && c.Filled(Caster.Map))
        //        {
        //            return false;
        //        }

        //        if (!c.InHorDistOf(Caster.Position, DistanceCalculator(Caster.Position, target.ClampInsideMap(Caster.Map))))
        //        {
        //            return false;
        //        }

        //        if (!c.InHorDistOf(Caster.Position, verbProps.range))
        //        {
        //            return false;
        //        }

        //        ShootLine resultingLine;
        //        return TryFindShootLineFromTo(Caster.Position, c, out resultingLine);
        //    }
        //}

        protected virtual List<IntVec3> AffectedCells(IntVec3 target, List<IntVec3> tmpCells)
        {
            tmpCells.Clear();
            Vector3 vector = Caster.Position.ToVector3Shifted().Yto0();
            IntVec3 intVec = target.ClampInsideMap(Caster.Map);
            if (Caster.Position == intVec)
            {
                return tmpCells;
            }

            float lengthHorizontal = (intVec - Caster.Position).LengthHorizontal;
            float num = (float)(intVec.x - Caster.Position.x) / lengthHorizontal;
            float num2 = (float)(intVec.z - Caster.Position.z) / lengthHorizontal;
            intVec.x = Mathf.RoundToInt((float)Caster.Position.x + num * verbProps.range);
            intVec.z = Mathf.RoundToInt((float)Caster.Position.z + num2 * verbProps.range);
            float target2 = Vector3.SignedAngle(intVec.ToVector3Shifted().Yto0() - vector, Vector3.right, Vector3.up);
            float num3 = lineWidthEnd / 2f;
            float num4 = Mathf.Sqrt(Mathf.Pow((intVec - Caster.Position).LengthHorizontal, 2f) + Mathf.Pow(num3, 2f));
            float num5 = 57.29578f * Mathf.Asin(num3 / num4);
            int num6 = GenRadial.NumCellsInRadius(verbProps.range < GenRadial.MaxRadialPatternRadius ? verbProps.range : GenRadial.MaxRadialPatternRadius - 0.1f);
            for (int i = 0; i < num6; i++)
            {
                IntVec3 intVec2 = Caster.Position + GenRadial.RadialPattern[i];
                if (CanUseCell(intVec2) && Mathf.Abs(Mathf.DeltaAngle(Vector3.SignedAngle(intVec2.ToVector3Shifted().Yto0() - vector, Vector3.right, Vector3.up), target2)) <= num5)
                {
                    tmpCells.Add(intVec2);
                }
            }

            List<IntVec3> list = GenSight.BresenhamCellsBetween(Caster.Position, target);
            for (int j = 0; j < list.Count; j++)
            {
                IntVec3 intVec3 = list[j];
                if (!tmpCells.Contains(intVec3) && CanUseCell(intVec3))
                {
                    tmpCells.Add(intVec3);
                }
            }

            if (compWeaponAbilities == null || !compWeaponAbilities.Props.isPenetration)
            {
                HashSet<IntVec3> removeCells = new HashSet<IntVec3>();
                List<Pawn> pawnList = new List<Pawn>();
                foreach (IntVec3 cell in tmpCells)
                {
                    if (cell.GetFirstPawn(Caster.Map) != null && cell.GetFirstPawn(Caster.Map) != CasterPawn)
                    {
                        // Skip cell that any pawn from the same faction within. May needs to be updated after execution function.
                        if (cell.GetFirstPawn(Caster.Map).Faction != CasterPawn.Faction)
                        {
                            pawnList.Add(cell.GetFirstPawn(Caster.Map));
                        }
                    }
                    if (pawnList.Count > 0)
                    {
                        if (cell != pawnList[0].Position && cell.GetFirstPawn(Caster.Map) != null)
                        {
                            removeCells.Add(cell);
                        }
                        if (cell.GetFirstPawn(Caster.Map) == null && DistanceCalculator(Caster.Position, pawnList[0].Position) < DistanceCalculator(Caster.Position, cell))
                        {
                            removeCells.Add(cell);
                        }
                        if (pawnList[0].Position != target)
                        {
                            removeCells.Add(target);
                        }
                    }
                }

                foreach (IntVec3 cell in removeCells)
                {
                    if (tmpCells.Contains(cell))
                    {
                        tmpCells.Remove(cell);
                    }
                }
            }

            return tmpCells;

            bool CanUseCell(IntVec3 c)
            {
                if (!c.InBounds(Caster.Map))
                {
                    return false;
                }

                if (c == Caster.Position)
                {
                    return false;
                }

                if (!canHitFilledCells && c.Filled(Caster.Map))
                {
                    return false;
                }

                if (!c.InHorDistOf(Caster.Position, DistanceCalculator(Caster.Position, target.ClampInsideMap(Caster.Map))))
                {
                    return false;
                }

                if (!c.InHorDistOf(Caster.Position, verbProps.range))
                {
                    return false;
                }

                ShootLine resultingLine;
                return TryFindShootLineFromTo(Caster.Position, c, out resultingLine);
            }
        }

        private void ApplyDamage(Thing thing, IntVec3 sourceCell, float damageFactor = 1f)
        {
            IntVec3 intVec = currentTarget.CenterVector3.Yto0().ToIntVec3();
            IntVec3 intVec2 = GenSight.LastPointOnLineOfSight(sourceCell, intVec, (IntVec3 c) => c.InBounds(caster.Map) && c.CanBeSeenOverFast(caster.Map), skipFirstCell: true);
            if (intVec2.IsValid)
            {
                intVec = intVec2;
            }

            Map map = caster.Map;
            if (thing == null || verbProps.beamDamageDef == null)
            {
                return;
            }

            // Avoid Friendly fire within same faction. To Do - how to enable excution?
            if (thing is Pawn pawn && (compWeaponAbilities == null || !compWeaponAbilities.Props.isPenetration))
            {
                if (pawn.Faction == caster.Faction)
                {
                    return;
                }
            }

            float angleFlat = (currentTarget.Cell - caster.Position).AngleFlat;
            BattleLogEntry_RangedImpact log = new BattleLogEntry_RangedImpact(caster, thing, currentTarget.Thing, base.EquipmentSource.def, null, null);
            DamageInfo dinfo;
            if (verbProps.beamTotalDamage > 0f)
            {
                float num = verbProps.beamTotalDamage / (float)pathCells.Count;
                num *= damageFactor;
                if (compWeaponAbilities != null && compWeaponAbilities.Props.isMelta)
                {
                    float bonus = compWeaponAbilities.Props.originalDamageAmount * compWeaponAbilities.Props.meltaAmount / 6;
                    DamageInfo bonusDinfo = new DamageInfo(
                        verbProps.beamDamageDef, // Or a custom Melta damageDef
                        bonus,
                        verbProps.beamDamageDef.defaultArmorPenetration,
                        angleFlat,
                        caster,
                        null,
                        EquipmentSource.def,
                        DamageInfo.SourceCategory.ThingOrUnknown,
                        currentTarget.Thing);

                    thing.TakeDamage(bonusDinfo);
                }
                dinfo = new DamageInfo(verbProps.beamDamageDef, num, verbProps.beamDamageDef.defaultArmorPenetration, angleFlat, caster, null, base.EquipmentSource.def, DamageInfo.SourceCategory.ThingOrUnknown, currentTarget.Thing);
            }
            else
            {
                float amount = (float)verbProps.beamDamageDef.defaultDamage * damageFactor;
                dinfo = new DamageInfo(verbProps.beamDamageDef, amount, verbProps.beamDamageDef.defaultArmorPenetration, angleFlat, caster, null, base.EquipmentSource.def, DamageInfo.SourceCategory.ThingOrUnknown, currentTarget.Thing);
            }

            thing.TakeDamage(dinfo).AssociateWithLog(log);
            if (thing.CanEverAttachFire())
            {
                float chance = ((verbProps.flammabilityAttachFireChanceCurve == null) ? verbProps.beamChanceToAttachFire : verbProps.flammabilityAttachFireChanceCurve.Evaluate(thing.GetStatValue(StatDefOf.Flammability)));
                if (Rand.Chance(chance))
                {
                    thing.TryAttachFire(verbProps.beamFireSizeRange.RandomInRange, caster);
                }
            }
            else if (Rand.Chance(verbProps.beamChanceToStartFire))
            {
                FireUtility.TryStartFireIn(intVec, map, verbProps.beamFireSizeRange.RandomInRange, caster, verbProps.flammabilityAttachFireChanceCurve);
            }
        }

        protected float DistanceCalculator(IntVec3 a, IntVec3 b)
        {
            float dx = Mathf.Abs(b.x - a.x);
            float dy = Mathf.Abs(b.y - a.y);
            float dz = Mathf.Abs(b.z - a.z);
            return dx + dy + dz;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref path, "path", LookMode.Value);
            Scribe_Values.Look(ref ticksToNextPathStep, "ticksToNextPathStep", 0);
            Scribe_Values.Look(ref initialTargetPosition, "initialTargetPosition");
            if (Scribe.mode == LoadSaveMode.PostLoadInit && path == null)
            {
                path = new List<Vector3>();
            }
        }
    }
}