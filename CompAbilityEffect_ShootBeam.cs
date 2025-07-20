using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.Sound;
using static UnityEngine.Random;
using static UnityEngine.GraphicsBuffer;
using static RimWorld.EffecterMaintainer;
using LudeonTK;

namespace MIM40kFactions
{
    public class CompAbilityEffect_ShootBeam : CompAbilityEffect
    {
        private List<Vector3> path = new List<Vector3>();

        private List<Vector3> tmpPath = new List<Vector3>();

        private int ticksToNextPathStep;

        private Vector3 initialTargetPosition;

        private MoteDualAttached mote;

        private Effecter endEffecter;

        private Sustainer sustainer;

        private HashSet<IntVec3> pathCells = new HashSet<IntVec3>();

        private HashSet<IntVec3> tmpPathCells = new HashSet<IntVec3>();

        private HashSet<IntVec3> tmpHighlightCells = new HashSet<IntVec3>();

        private HashSet<IntVec3> tmpSecondaryHighlightCells = new HashSet<IntVec3>();

        private float lineWidthEnd;

        private bool canHitFilledCells = true;

        protected int lastShotTick = -999999;

        private readonly List<IntVec3> targetCells = new List<IntVec3>();

        private readonly List<IntVec3> tmpTargetCells = new List<IntVec3>();

        private int burstShotsLeft;

        protected int ticksToNextBurstShot;

        private Vector3 vectorBurstingTick;
        private Vector3 vectorBurstingTick2;

        private Verb BeamVerb => parent.verb;

        protected virtual int ShotsPerBurst => Math.Max(2, Props.burstShotCount);

        public float ShotProgress => (float)burstShotsLeft / (float)ShotsPerBurst;
        public Vector3 InterpolatedPosition
        {
            get
            {
                Vector3 vector = BeamVerb.CurrentTarget.CenterVector3 - initialTargetPosition;
                int indexA = Mathf.Clamp(burstShotsLeft, 0, path.Count - 1);
                int indexB = Mathf.Clamp(burstShotsLeft + 1, 0, path.Count - 1);
                return Vector3.Lerp(path[indexA], path[indexB], ShotProgress) + vector;
            }
        }

        [TweakValue("Incinerator", 0f, 10f)]
        public static float DistanceToLifetimeScalar = 5f;

        [TweakValue("Incinerator", -2f, 7f)]
        public static float BarrelOffset = 5f;

        private IncineratorSpray sprayer;

        private ThingDef incineratorSprayThingDef = ThingDefOf.IncineratorSpray;

        private ThingDef incineratorMoteDef = ThingDefOf.Mote_IncineratorBurst;

        private new CompProperties_AbilityShootBeam Props => (CompProperties_AbilityShootBeam)props;

        private Pawn Pawn => parent.pawn;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (Props.isHazardous && Utility_WeaponStatChanger.IsHazardousCalculator())
            {
                Utility_WeaponStatChanger.ApplySurgicalDamage(Pawn, Props.beamDamageDef ?? DamageDefOf.Burn, Props.beamDamageDef?.defaultDamage / 2f ?? 10f, null, Pawn);
                // Cease fire: log, maybe visual feedback, then exit
                MoteMaker.ThrowText(Pawn.DrawPos, Pawn.Map, "EMWH_WeaponAbilities_Hazardous".Translate(), Color.red);
                return; // Cancel the attack
            }
            if (Props.isArcSprayAbility)
            {
                if (Props.incineratorSprayThingDef != null)
                {
                    incineratorSprayThingDef = Props.incineratorSprayThingDef;
                }
                sprayer = GenSpawn.Spawn(incineratorSprayThingDef, Pawn.Position, Pawn.Map) as IncineratorSpray;
            }

            burstShotsLeft = ShotsPerBurst;
            BeamVerb.state = VerbState.Bursting;
            ticksToNextBurstShot = Props.ticksBetweenBurstShots;
            initialTargetPosition = target.CenterVector3;
            CalculatePath(initialTargetPosition, path, pathCells);
            targetCells.Clear();
            if (Props.beamWidth > 1)
            {
                for (int i = 0; i < ShotsPerBurst; i++)
                {
                    AffectedCells(path[i].ToIntVec3(), targetCells);
                    if (Props.isPenetration)
                    {
                        targetCells.Add(path[i].ToIntVec3());
                    }
                }
            }
            else
            {
                AffectedCells(initialTargetPosition.ToIntVec3(), targetCells);
            }

            if (!targetCells.Contains(initialTargetPosition.ToIntVec3()) && targetCells.Count > 1 && !Props.isPenetration)
            {
                target = targetCells[targetCells.Count - 1];
            }

            if (Props.beamMoteDef != null)
            {
                //mote = MoteMaker.MakeInteractionOverlay(verbProps.beamMoteDef, caster, new TargetInfo(targetCells[0], caster.Map));
                //mote = MoteMaker.MakeInteractionOverlay(Props.beamMoteDef, Pawn, new TargetInfo(path[0].ToIntVec3(), Pawn.Map));
            }
            if (!Utility_TargetValidator.IsValidTargetForAbility(target.Thing, Pawn, Props.targetHostilesOnly, Props.targetNeutralBuildings, BeamVerb.verbProps.targetParams.canTargetLocations))
            {
                Log.Message($"[MIM Debug] IsValidTargetForAbility = False");
                return;
            }
            TryCastNextBurstShot(target);

            endEffecter?.Cleanup();
            int j = 0;
            if (Props.soundCastBeam != null)
            {
                sustainer = Props.soundCastBeam.TrySpawnSustainer(SoundInfo.InMap(Pawn, MaintenanceType.PerTick));
            }
            if (BeamVerb.state == VerbState.Bursting)
            {
                if (!BeamVerb.caster.Spawned || (BeamVerb.caster is Pawn pawn && pawn.stances.stunner.Stunned))
                {
                    BeamVerb.Reset();
                }
                else
                {
                    if (Props.beamWidth > 1)
                    {
                        vectorBurstingTick = InterpolatedPosition;
                        vectorBurstingTick2 = InterpolatedPosition - Pawn.Position.ToVector3Shifted();
                    }
                    else
                    {
                        vectorBurstingTick = BeamVerb.CurrentTarget.CenterVector3;
                        vectorBurstingTick2 = BeamVerb.CurrentTarget.CenterVector3 - Pawn.Position.ToVector3Shifted();
                    }
                    for (int i = 0; i < burstShotsLeft; burstShotsLeft--)
                    {
                        if (Props.beamMoteDef != null)
                        {
                            //mote = MoteMaker.MakeInteractionOverlay(verbProps.beamMoteDef, caster, new TargetInfo(targetCells[0], caster.Map));
                            mote = MoteMaker.MakeInteractionOverlay(Props.beamMoteDef, Pawn, new TargetInfo(path[j].ToIntVec3(), Pawn.Map));
                        }
                        BurstingTick(BeamVerb.CurrentTarget, vectorBurstingTick, vectorBurstingTick2);
                        if (Props.beamWidth > 1)
                        {
                            vectorBurstingTick = InterpolatedPosition;
                            vectorBurstingTick2 = InterpolatedPosition - Pawn.Position.ToVector3Shifted();
                        }
                        if (Props.isArcSprayAbility)
                        {
                            TryCastShotArcSpray(target);
                        }
                        else
                        {
                            TryCastShot(target);
                        }
                        j++;
                    }
                }
            }

            if (Pawn != null)
            {
                Pawn.records.Increment(RecordDefOf.ShotsFired);
            }

            base.Apply(target, dest);

            if (Props.isArcSprayAbility)
            {
                Find.BattleLog.Add(new BattleLogEntry_RangedFire(Pawn, target.HasThing ? target.Thing : null, BeamVerb.EquipmentSource?.def, null, burst: false));
            }
        }

        private bool TryCastShotArcSpray(LocalTargetInfo target)
        {
            if (!Props.isArcSprayAbility)
            {
                return false;
            }

            if (Props.incineratorMoteDef != null)
            {
                incineratorMoteDef = Props.incineratorMoteDef;
            }
            bool result = TryCastShot(target);
            Vector3 vector = InterpolatedPosition.Yto0();
            IntVec3 intVec = vector.ToIntVec3();
            Vector3 drawPos = Pawn.DrawPos;
            Vector3 normalized = (vector - drawPos).normalized;
            drawPos += normalized * BarrelOffset;
            MoteDualAttached moteDualAttached = MoteMaker.MakeInteractionOverlay(A: new TargetInfo(Pawn.Position, Pawn.Map), moteDef: incineratorMoteDef, B: new TargetInfo(intVec, Pawn.Map));
            float num = Vector3.Distance(vector, drawPos);
            float num2 = ((num < BarrelOffset) ? 0.5f : 1f);
            IncineratorSpray incineratorSpray = sprayer;
            if (incineratorSpray != null)
            {
                incineratorSpray.Add(new IncineratorProjectileMotion
                {
                    mote = moteDualAttached,
                    targetDest = intVec,
                    worldSource = drawPos,
                    worldTarget = vector,
                    moveVector = (vector - drawPos).normalized,
                    startScale = 1f * num2,
                    endScale = (1f + Rand.Range(0.1f, 0.4f)) * num2,
                    lifespanTicks = Mathf.FloorToInt(num * DistanceToLifetimeScalar)
                });
                return result;
            }

            return result;
        }

        private bool TryCastShot(LocalTargetInfo target)
        {
            ShootLine resultingLine;
            bool flag = BeamVerb.TryFindShootLineFromTo(Pawn.Position, target, out resultingLine);

            if (BeamVerb.verbProps.stopBurstWithoutLos && !flag)
            {
                return false;
            }

            if (BeamVerb.EquipmentSource != null)
            {
                BeamVerb.EquipmentSource.GetComp<CompChangeableProjectile>()?.Notify_ProjectileLaunched();
                BeamVerb.EquipmentSource.GetComp<CompApparelReloadable>()?.UsedOnce();
            }

            lastShotTick = Find.TickManager.TicksGame;
            ticksToNextPathStep = Props.ticksBetweenBurstShots;

            if (!targetCells.Contains(target.Cell) && targetCells.Count > 1)
            {
                target = targetCells[targetCells.Count - 1];
            }

            IntVec3 targetCell = InterpolatedPosition.Yto0().ToIntVec3();

            //IntVec3 targetCell = target.CenterVector3.Yto0().ToIntVec3(); 
            bool hitOk = TryGetHitCell(resultingLine.Source, targetCell, out var hitCell);
            if (!hitOk)
                return true;

            if (targetCells.Contains(Pawn.Position))
            {
                targetCells.Remove(Pawn.Position);
            }

            foreach (IntVec3 cell in targetCells)
            {
                HitCell(target, cell, resultingLine.Source);
            }
            if (Props.beamHitsNeighborCells)
            {
                targetCells.Add(hitCell);
                foreach (IntVec3 beamHitNeighbourCell in GetBeamHitNeighbourCells(resultingLine.Source, hitCell))
                {
                    if (!targetCells.Contains(beamHitNeighbourCell))
                    {
                        float damageFactor = (pathCells.Contains(beamHitNeighbourCell) ? 1f : 0.5f);
                        HitCell(target, beamHitNeighbourCell, resultingLine.Source, damageFactor);
                        targetCells.Add(beamHitNeighbourCell);
                    }
                }
                if (targetCells.Contains(Pawn.Position))
                {
                    targetCells.Remove(Pawn.Position);
                }
            }
            return true;
        }

        protected void TryCastNextBurstShot(LocalTargetInfo target)
        {
            LocalTargetInfo localTargetInfo = target;
            if (Available() && TryCastShot(target))
            {
                if (BeamVerb.verbProps.muzzleFlashScale > 0.01f)
                {
                    FleckMaker.Static(Pawn.Position, Pawn.Map, FleckDefOf.ShotFlash, BeamVerb.verbProps.muzzleFlashScale);
                }

                if (BeamVerb.verbProps.soundCast != null)
                {
                    BeamVerb.verbProps.soundCast.PlayOneShot(new TargetInfo(Pawn.Position, Pawn.MapHeld));
                }

                if (BeamVerb.verbProps.soundCastTail != null)
                {
                    BeamVerb.verbProps.soundCastTail.PlayOneShotOnCamera(Pawn.Map);
                }

                if (BeamVerb.CasterPawn != null)
                {
                    BeamVerb.CasterPawn.Notify_UsedVerb(BeamVerb.CasterPawn, BeamVerb);

                    if (BeamVerb.TerrainDefSource != null)
                    {
                        BeamVerb.CasterPawn.meleeVerbs.Notify_UsedTerrainBasedVerb();
                    }

                    if (BeamVerb.CasterPawn.health != null)
                    {
                        BeamVerb.CasterPawn.health.Notify_UsedVerb(BeamVerb, localTargetInfo);
                    }

                    if (BeamVerb.EquipmentSource != null)
                    {
                        BeamVerb.EquipmentSource.Notify_UsedWeapon(Pawn);
                    }

                    if (!Pawn.Spawned)
                    {
                        BeamVerb.Reset();
                        return;
                    }
                }

                burstShotsLeft--;
            }
            else
            {
                burstShotsLeft = 0;
            }

            if (burstShotsLeft > 0)
            {
                if (BeamVerb.CasterIsPawn && !BeamVerb.NonInterruptingSelfCast)
                {
                    BeamVerb.CasterPawn.stances.SetStance(new Stance_Cooldown(Props.ticksBetweenBurstShots + 1, target, BeamVerb));
                }

                return;
            }

            BeamVerb.state = VerbState.Idle;
            if (BeamVerb.CasterIsPawn && !BeamVerb.NonInterruptingSelfCast)
            {
                BeamVerb.CasterPawn.stances.SetStance(new Stance_Cooldown(BeamVerb.verbProps.AdjustedCooldownTicks(BeamVerb, BeamVerb.CasterPawn), target, BeamVerb));
            }

            if (BeamVerb.castCompleteCallback != null)
            {
                BeamVerb.castCompleteCallback();
            }
        }

        private bool Available()
        {
            CompApparelVerbOwner compApparelVerbOwner = BeamVerb.EquipmentSource?.GetComp<CompApparelVerbOwner>();
            if (compApparelVerbOwner != null && !compApparelVerbOwner.CanBeUsed(out var reason))
            {
                return false;
            }

            if (Pawn != null && BeamVerb.EquipmentSource != null && EquipmentUtility.RolePreventsFromUsing(Pawn, BeamVerb.EquipmentSource, out reason))
            {
                return false;
            }

            return true;
        }

        private void BurstingTick(LocalTargetInfo target, Vector3 vector, Vector3 vector2)
        {
            IntVec3 intVec = vector.ToIntVec3();
            float num = vector2.MagnitudeHorizontal();
            Vector3 normalized = vector2.Yto0().normalized;
            IntVec3 intVec2 = GenSight.LastPointOnLineOfSight(Pawn.Position, intVec, (IntVec3 c) => c.CanBeSeenOverFast(Pawn.Map), skipFirstCell: true);
            if (intVec2.IsValid)
            {
                num -= (intVec - intVec2).LengthHorizontal;
                vector = Pawn.Position.ToVector3Shifted() + normalized * num;
                intVec = vector.ToIntVec3();
            }

            Vector3 offsetA = normalized * Props.beamStartOffset;
            Vector3 vector3 = vector - intVec.ToVector3Shifted();
            if (mote != null)
            {
                mote.UpdateTargets(new TargetInfo(Pawn.Position, Pawn.Map), new TargetInfo(intVec, Pawn.Map), offsetA, vector3);
                mote.Maintain();
            }

            if (Props.beamGroundFleckDef != null && Rand.Chance(Props.beamFleckChancePerTick))
            {
                FleckMaker.Static(vector, Pawn.Map, Props.beamGroundFleckDef);
            }

            if (endEffecter == null && Props.beamEndEffecterDef != null)
            {
                endEffecter = Props.beamEndEffecterDef.Spawn(intVec, Pawn.Map, vector3);
            }

            if (endEffecter != null)
            {
                endEffecter.offset = vector3;
                endEffecter.EffectTick(new TargetInfo(intVec, Pawn.Map), TargetInfo.Invalid);
                endEffecter.ticksLeft--;
            }

            if (Props.beamLineFleckDef != null)
            {
                float num2 = 1f * num;
                for (int i = 0; (float)i < num2; i++)
                {
                    if (Rand.Chance(Props.beamLineFleckChanceCurve.Evaluate((float)i / num2)))
                    {
                        Vector3 vector4 = i * normalized - normalized * Rand.Value + normalized / 2f;
                        FleckMaker.Static(Pawn.Position.ToVector3Shifted() + vector4, Pawn.Map, Props.beamLineFleckDef);
                    }
                }
            }

            if (!sustainer.Ended)
            {
                sustainer?.Maintain();
            }
        }

        public override IEnumerable<PreCastAction> GetPreCastActions()
        {
            if (Props.effecterDef != null)
            {
                yield return new PreCastAction
                {
                    action = delegate (LocalTargetInfo a, LocalTargetInfo b)
                    {
                        parent.AddEffecterToMaintain(Props.effecterDef.Spawn(parent.pawn.Position, a.Cell, parent.pawn.Map), Pawn.Position, a.Cell, Props.ticksAwayFromCast, Pawn.MapHeld);
                    },
                    ticksAwayFromCast = Props.ticksAwayFromCast
                };
            }
        }

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            tmpTargetCells.Clear();
            IntVec3 targetCell = target.Cell;
            AffectedCells(targetCell, tmpTargetCells);
            lineWidthEnd = Props.lineWidthEnd;
            foreach (IntVec3 tmpCell in tmpTargetCells)
            {
                ShootLine resultingLine;
                bool flag = BeamVerb.TryFindShootLineFromTo(Pawn.Position, target, out resultingLine);
                if ((BeamVerb.verbProps.stopBurstWithoutLos && !flag) || !TryGetHitCell(resultingLine.Source, tmpCell, out var hitCell))
                {
                    continue;
                }

                tmpHighlightCells.Add(hitCell);
                if (!Props.beamHitsNeighborCells)
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
                bool flag = BeamVerb.TryFindShootLineFromTo(Pawn.Position, target, out resultingLine);
                if ((BeamVerb.verbProps.stopBurstWithoutLos && !flag) || !TryGetHitCell(resultingLine.Source, tmpPathCell, out var hitCell))
                {
                    continue;
                }

                tmpSecondaryHighlightCells.Add(hitCell);
            }

            tmpSecondaryHighlightCells.RemoveWhere((IntVec3 x) => tmpHighlightCells.Contains(x));
            if (tmpHighlightCells.Any())
            {
                GenDraw.DrawFieldEdges(tmpHighlightCells.ToList(), Props.highlightColor);
            }
            if (tmpSecondaryHighlightCells.Any())
            {
                GenDraw.DrawFieldEdges(tmpSecondaryHighlightCells.ToList(), Props.secondaryHighlightColor);
            }

            tmpHighlightCells.Clear();
            tmpSecondaryHighlightCells.Clear();
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            if (!target.IsValid || !target.HasThing)
                return false;

            return Utility_TargetValidator.IsValidTargetForAbility(target.Thing, parent.pawn, Props.targetHostilesOnly, Props.targetNeutralBuildings, BeamVerb.verbProps.targetParams.canTargetLocations);
        }

        private List<IntVec3> AffectedCells(IntVec3 target, List<IntVec3> tmpCells)
        {
            tmpCells.Clear();
            Vector3 vector = Pawn.Position.ToVector3Shifted().Yto0();
            IntVec3 intVec = target.ClampInsideMap(Pawn.Map);
            if (Pawn.Position == intVec)
            {
                return tmpCells;
            }

            lineWidthEnd = LineWidthEndCalculator();
            canHitFilledCells = Props.canHitFilledCells;

            float lengthHorizontal = (intVec - Pawn.Position).LengthHorizontal;
            float num = (float)(intVec.x - Pawn.Position.x) / lengthHorizontal;
            float num2 = (float)(intVec.z - Pawn.Position.z) / lengthHorizontal;
            intVec.x = Mathf.RoundToInt((float)Pawn.Position.x + num * BeamVerb.verbProps.range);
            intVec.z = Mathf.RoundToInt((float)Pawn.Position.z + num2 * BeamVerb.verbProps.range);
            float target2 = Vector3.SignedAngle(intVec.ToVector3Shifted().Yto0() - vector, Vector3.right, Vector3.up);
            float num3 = lineWidthEnd / 2f;
            float num4 = Mathf.Sqrt(Mathf.Pow((intVec - Pawn.Position).LengthHorizontal, 2f) + Mathf.Pow(num3, 2f));
            float num5 = 57.29578f * Mathf.Asin(num3 / num4);
            int num6 = GenRadial.NumCellsInRadius(BeamVerb.verbProps.range);
            for (int i = 0; i < num6; i++)
            {
                IntVec3 intVec2 = Pawn.Position + GenRadial.RadialPattern[i];
                if (CanUseCell(intVec2) && Mathf.Abs(Mathf.DeltaAngle(Vector3.SignedAngle(intVec2.ToVector3Shifted().Yto0() - vector, Vector3.right, Vector3.up), target2)) <= num5)
                {
                    tmpCells.Add(intVec2);
                }
            }

            List<IntVec3> list = GenSight.BresenhamCellsBetween(Pawn.Position, target);
            for (int j = 0; j < list.Count; j++)
            {
                IntVec3 intVec3 = list[j];
                if (!tmpCells.Contains(intVec3) && CanUseCell(intVec3))
                {
                    tmpCells.Add(intVec3);
                }
            }

            if (!Props.isPenetration)
            {
                HashSet<IntVec3> removeCells = new HashSet<IntVec3>();
                List<Pawn> pawnList = new List<Pawn>();
                foreach (IntVec3 cell in tmpCells)
                {
                    if (cell.GetFirstPawn(Pawn.Map) != null && cell.GetFirstPawn(Pawn.Map) != Pawn)
                    {
                        pawnList.Add(cell.GetFirstPawn(Pawn.Map));
                    }
                    if (pawnList.Count > 0)
                    {
                        if (cell != pawnList[0].Position && cell.GetFirstPawn(Pawn.Map) != null)
                        {
                            removeCells.Add(cell);
                        }
                        if (cell.GetFirstPawn(Pawn.Map) == null && IntVec3Utility.DistanceTo(Pawn.Position, pawnList[0].Position) < IntVec3Utility.DistanceTo(Pawn.Position, cell))
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
                if (!c.InBounds(Pawn.Map))
                {
                    return false;
                }

                if (c == Pawn.Position)
                {
                    return false;
                }

                if (!canHitFilledCells && c.Filled(Pawn.Map))
                {
                    return false;
                }

                if (!c.InHorDistOf(Pawn.Position, IntVec3Utility.DistanceTo(Pawn.Position, target.ClampInsideMap(Pawn.Map))))
                {
                    return false;
                }

                if (!c.InHorDistOf(Pawn.Position, BeamVerb.verbProps.range))
                {
                    return false;
                }

                ShootLine resultingLine;
                return BeamVerb.TryFindShootLineFromTo(Pawn.Position, c, out resultingLine);
            }
        }

        private float LineWidthEndCalculator()
        {
            lineWidthEnd = Props.beamWidth;

            if (Props.lineWidthEnd > 0)
            {
                lineWidthEnd = Props.lineWidthEnd;
            }
            return lineWidthEnd;
        }

        protected bool TryGetHitCell(IntVec3 source, IntVec3 targetCell, out IntVec3 hitCell)
        {
            IntVec3 intVec = GenSight.LastPointOnLineOfSight(source, targetCell, (IntVec3 c) => c.InBounds(Pawn.Map) && c.CanBeSeenOverFast(Pawn.Map), skipFirstCell: true);
            if (Props.beamCantHitWithinMinRange && intVec.DistanceTo(source) < BeamVerb.verbProps.minRange)
            {
                hitCell = default(IntVec3);
                return false;
            }

            hitCell = (intVec.IsValid ? intVec : targetCell);
            return intVec.IsValid;
        }

        protected IEnumerable<IntVec3> GetBeamHitNeighbourCells(IntVec3 source, IntVec3 pos)
        {
            if (!Props.beamHitsNeighborCells)
            {
                yield break;
            }

            for (int i = 0; i < 4; i++)
            {
                IntVec3 intVec = pos + GenAdj.CardinalDirections[i];
                if (intVec.InBounds(Pawn.Map) && (!Props.beamHitsNeighborCellsRequiresLOS || GenSight.LineOfSight(source, intVec, Pawn.Map)))
                {
                    yield return intVec;
                }
            }
        }

        private void CalculatePath(Vector3 target, List<Vector3> pathList, HashSet<IntVec3> pathCellsList, bool addRandomOffset = true)
        {
            pathList.Clear();
            Vector3 vector = (target - Pawn.Position.ToVector3Shifted()).Yto0();
            float magnitude = vector.magnitude;
            Vector3 normalized = vector.normalized;
            Vector3 vector2 = normalized.RotatedBy(-90f);
            float num = ((Props.beamFullWidthRange > 0f) ? Mathf.Min(magnitude / Props.beamFullWidthRange, 1f) : 1f);
            float num2 = 1f;
            if (Props.beamWidth > 1)
            {
                num2 = (Props.beamWidth + 1f) * num / (float)ShotsPerBurst;
            }
            Vector3 vector3 = target.Yto0() - vector2 * Props.beamWidth / 2f * num;
            pathList.Add(vector3);
            for (int i = 0; i < Mathf.Max(ShotsPerBurst, 2); i++)
            {
                Vector3 vector4 = normalized * (Rand.Value * Props.beamMaxDeviation) - normalized / 2f;
                Vector3 vector5 = Mathf.Sin(((float)i / (float)ShotsPerBurst + 0.5f) * (float)Math.PI * 57.29578f) * Props.beamCurvature * -normalized - normalized * Props.beamMaxDeviation / 2f;
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

            return !CoverUtility.ThingCovered(thing, Pawn.Map);
        }

        private void HitCell(LocalTargetInfo target, IntVec3 cell, IntVec3 sourceCell, float damageFactor = 1f)
        {
            if (cell.InBounds(Pawn.Map))
            {
                ApplyDamage(target, VerbUtility.ThingsToHit(cell, Pawn.Map, CanHit).RandomElementWithFallback(), sourceCell, damageFactor);
                if (Props.beamSetsGroundOnFire && Rand.Chance(Props.beamChanceToStartFire))
                {
                    FireUtility.TryStartFireIn(cell, Pawn.Map, 1f, Pawn);
                }
            }
        }

        private void ApplyDamage(LocalTargetInfo target, Thing thing, IntVec3 sourceCell, float damageFactor = 1f)
        {
            IntVec3 intVec = target.CenterVector3.Yto0().ToIntVec3();
            IntVec3 intVec2 = GenSight.LastPointOnLineOfSight(sourceCell, intVec, (IntVec3 c) => c.InBounds(Pawn.Map) && c.CanBeSeenOverFast(Pawn.Map), skipFirstCell: true);
            if (intVec2.IsValid)
            {
                intVec = intVec2;
            }

            Map map = Pawn.Map;
            if (thing == null || Props.beamDamageDef == null)
            {
                return;
            }

            float angleFlat = (target.Cell - Pawn.Position).AngleFlat;

            ThingDef equipmentSource = null;
            if (BeamVerb.EquipmentSource != null)
            {
                equipmentSource = BeamVerb.EquipmentSource.def;
            }
            BattleLogEntry_RangedImpact log = new BattleLogEntry_RangedImpact(Pawn, thing, target.Thing, equipmentSource, null, null);
            DamageInfo dinfo;
            float amount = (float)Props.beamDamageDef.defaultDamage * damageFactor;
            dinfo = new DamageInfo(Props.beamDamageDef, amount, Props.beamDamageDef.defaultArmorPenetration, angleFlat, Pawn, null, equipmentSource, DamageInfo.SourceCategory.ThingOrUnknown, target.Thing);

            thing.TakeDamage(dinfo).AssociateWithLog(log);
            if (thing.CanEverAttachFire())
            {
                float chance = ((Props.flammabilityAttachFireChanceCurve == null) ? Props.beamChanceToAttachFire : Props.flammabilityAttachFireChanceCurve.Evaluate(thing.GetStatValue(StatDefOf.Flammability)));
                if (Rand.Chance(chance))
                {
                    thing.TryAttachFire(Props.beamFireSizeRange.RandomInRange, Pawn);
                }
            }
            else if (Rand.Chance(Props.beamChanceToStartFire))
            {
                FireUtility.TryStartFireIn(intVec, map, Props.beamFireSizeRange.RandomInRange, Pawn, Props.flammabilityAttachFireChanceCurve);
            }
        }

        private int RandomDamageCalculator(int diceResult, IntRange fumbleDamageDice, IntRange normalDamageDice, IntRange criticalDamageDice)
        {
            int damageResult;
            if (normalDamageDice == null)
                normalDamageDice = new IntRange(1, 6);
            if (criticalDamageDice == null)
                criticalDamageDice = new IntRange(4, 6);

            if (diceResult == 0 && fumbleDamageDice != null)
            {
                damageResult = fumbleDamageDice.RandomInRange;
                return damageResult * Props.damAmount;
            }
            if (diceResult == 0 && fumbleDamageDice == null)
            {
                damageResult = normalDamageDice.RandomInRange;
                return damageResult * Props.damAmount;
            }
            if (diceResult == 6)
            {
                damageResult = criticalDamageDice.RandomInRange;
                return damageResult * Props.damAmount;
            }
            else
                damageResult = normalDamageDice.RandomInRange;
            return damageResult * Props.damAmount;
        }
    }
}