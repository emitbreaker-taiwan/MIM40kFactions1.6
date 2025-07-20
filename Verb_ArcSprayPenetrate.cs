using LudeonTK;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MIM40kFactions
{
    public class Verb_ArcSprayPenetrate : Verb_ShootLasBeam
    {
        [TweakValue("Incinerator", 0f, 10f)]
        public static float DistanceToLifetimeScalar = 5f;

        [TweakValue("Incinerator", -2f, 7f)]
        public static float BarrelOffset = 5f;

        private IncineratorSpray sprayer;

        private ThingDef incineratorSprayThingDef = ThingDefOf.IncineratorSpray;

        private ThingDef incineratorMoteDef = ThingDefOf.Mote_IncineratorBurst;

        public override void WarmupComplete()
        {
            LasBeamPropertiesExtension modExtension = EquipmentSource.def.GetModExtension<LasBeamPropertiesExtension>();
            if (modExtension != null && modExtension.incineratorSprayThingDef != null)
            {
                incineratorSprayThingDef = modExtension.incineratorSprayThingDef;
            }
            sprayer = GenSpawn.Spawn(incineratorSprayThingDef, caster.Position, caster.Map) as IncineratorSpray;
            base.WarmupComplete();
            Find.BattleLog.Add(new BattleLogEntry_RangedFire(caster, currentTarget.HasThing ? currentTarget.Thing : null, base.EquipmentSource?.def, null, burst: false));
        }

        protected override bool TryCastShot()
        {
            LasBeamPropertiesExtension modExtension = EquipmentSource.def.GetModExtension<LasBeamPropertiesExtension>();
            if (modExtension != null && modExtension.incineratorMoteDef != null)
            {
                incineratorMoteDef = modExtension.incineratorMoteDef;
            }
            bool result = base.TryCastShot();

            return result;
        }

        public override void BurstingTick()
        {
            base.BurstingTick();

            if (sprayer == null || burstShotsLeft <= 0) return;

            Vector3 vector = InterpolatedPosition;
            IntVec3 intVec = vector.ToIntVec3();
            Vector3 drawPos = caster.DrawPos;
            Vector3 normalized = (vector - drawPos).normalized;
            drawPos += normalized * BarrelOffset;
            MoteDualAttached moteDualAttached = MoteMaker.MakeInteractionOverlay(A: new TargetInfo(caster.Position, caster.Map), moteDef: incineratorMoteDef, B: new TargetInfo(intVec, caster.Map));
            float num = Vector3.Distance(vector, drawPos);
            float num2 = ((num < BarrelOffset) ? 0.5f : 1f);

            sprayer.Add(new IncineratorProjectileMotion
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
        }

        protected override List<IntVec3> AffectedCells(IntVec3 target, List<IntVec3> tmpCells)
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

                // Exclude all cells occupied by the caster only if the caster is a building
                if (Caster.def.category == ThingCategory.Building && Caster.OccupiedRect().Contains(c))
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
    }
}