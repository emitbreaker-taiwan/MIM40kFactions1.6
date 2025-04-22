using System.Collections.Generic;
using System.Linq;
using System;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class EMWH_BlightGas : Gas
    {
        public MIM40kFactionsGasProperties Props => def.gas as MIM40kFactionsGasProperties;
        
        public override void Tick()
        {
            try
            {
                if (destroyTick <= Find.TickManager.TicksGame)
                {
                    Destroy();
                }

                graphicRotation += graphicRotationSpeed;

                if (!Destroyed && Props.hediffDef != null)
                    AddHediff();
                else
                    return;
            }
            catch (NullReferenceException)
            {
                return;
            }
        }
        private void AddHediff()
        {
            foreach (Thing p in GenRadial.RadialDistinctThingsAround(Position, Map, 1f, useCenter: true))
            {
                Pawn pawn;
                if ((pawn = p as Pawn) == null)
                    return;
                if ((pawn.Position != Position) || pawn == null || !pawn.Spawned || pawn.health.immunity.AnyGeneMakesFullyImmuneTo(Props.hediffDef) || !pawn.IsHashIntervalTick(Props.mtbCheckDuration) || pawn.health.immunity.ImmunityRecordExists(Props.hediffDef) == true)
                    return;
                IngestHediffToPawn(pawn, Props.severityAdjustment);
            }
        }
        private void OldAddHediff()
        {
            List<Thing> pawnList = Position.GetThingList(Map);
            if (pawnList != null)
                foreach (Pawn pawn in this.MapHeld.mapPawns.AllPawnsSpawned)
                {
                    if (pawn != null)
                    {
                        if (!pawn.Spawned || pawn.health.immunity.AnyGeneMakesFullyImmuneTo(Props.hediffDef) || !pawn.IsHashIntervalTick(Props.mtbCheckDuration) || pawn.health.immunity.ImmunityRecordExists(Props.hediffDef) == true)
                            return;
                        IngestHediffToPawn(pawn, Props.severityAdjustment);
                    }
                }
        }
        private void IngestHediffToPawn(Pawn pawn, float severityAdjustment)
        {
            Hediff hediffNurglesRot = HediffMaker.MakeHediff(Props.hediffDef, pawn);

            if (Props.exposureStatFactor != null)
            {
                severityAdjustment *= 1f - pawn.GetStatValue(Props.exposureStatFactor);
            }
            if ((double)severityAdjustment != 0.0 && !Destroyed)
                HealthUtility.AdjustSeverity(pawn, Props.hediffDef, severityAdjustment);
        }
    }
}
