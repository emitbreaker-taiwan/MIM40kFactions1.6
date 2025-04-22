using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class CompAbilityEffect_SpawnPawn : CompAbilityEffect
    {
        public CompProperties_AbilitySpawnPawn p => (CompProperties_AbilitySpawnPawn)props;
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (p.amount > 0)
            {
                for (int i = 0; i < p.amount; i++)
                {
                    SpawnPawn(p.pawnKind, target, parent.pawn.Map);
                    if (p.sendSkipSignal)
                    {
                        return;
                    }
                    CompAbilityEffect_Teleport.SendSkipUsedSignal(target, parent.pawn);
                }
            }
        }
        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (!target.Cell.Filled(parent.pawn.Map) || target.Cell.GetEdifice(parent.pawn.Map) == null)
            {
                return true;
            }

            if (p.pawnKind.RaceProps.IsMechanoid && !MechanitorUtility.IsMechanitor(parent.pawn))
            {
                if (throwMessages)
                {
                    Messages.Message((string)("EMWH_MustBeMechanitor".Translate((NamedArgument)parent.def.label, parent.pawn)), (LookTargets)target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, false);
                }
                return false;
            }

            if (throwMessages)
            {
                Messages.Message((string)("CannotUseAbility".Translate((NamedArgument)parent.def.label) + ": " + "AbilityOccupiedCells".Translate()), (LookTargets)target.ToTargetInfo(parent.pawn.Map), MessageTypeDefOf.RejectInput, false);
            }
            return false;
        }
        public virtual void SpawnPawn(PawnKindDef pawnKind, LocalTargetInfo target, Map map)
        {
            Faction targetFaction = parent.pawn.Faction;

            if (p.forcedFaction != null)
            {
                targetFaction.def = p.forcedFaction;
            }

            PawnGenerationRequest request = new PawnGenerationRequest
                (
                    (PawnKindDef)pawnKind,
                    (Faction)targetFaction,
                    (PawnGenerationContext)PawnGenerationContext.NonPlayer,
                    -1,
                    false,
                    false,
                    false,
                    false,
                    true,
                    1f,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    0.0f,
                    0.0f,
                    null,
                    1f,
                    null,
                    null,
                    null,
                    null,
                    new float?(),
                    new float?(),
                    new float?(),
                    new Gender?(),
                    null,
                    null,
                    null,
                    (Ideo)null,
                    false,
                    false,
                    false,
                    false,
                    null,
                    null,
                    null,
                    null,
                    null,
                    -1f,
                    DevelopmentalStage.Adult,
                    null,
                    null,
                    null,
                    false,
                    false,
                    false,
                    -1,
                    0,
                    true
                );
            Pawn pawn = PawnGenerator.GeneratePawn(request);

            if (pawn == null)
            {
                return;
            }

            if (ModsConfig.BiotechActive && pawn.RaceProps.IsMechanoid)
            {
                if (parent.pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.MechlinkImplant) != null)
                {
                    if (pawn.IsColonyMech)
                    {
                        pawn.GetOverseer()?.relations.RemoveDirectRelation(PawnRelationDefOf.Overseer, pawn);
                        parent.pawn.relations.AddDirectRelation(PawnRelationDefOf.Overseer, pawn);
                    }
                }
                else if (parent.pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.MechlinkImplant) == null)
                {
                    pawn.Destroy();
                }
            }

            if (ModsConfig.IdeologyActive && pawn.RaceProps.Humanlike)
            {
                pawn.ideo.SetIdeo(targetFaction.ideos.PrimaryIdeo);
            }

            if (!pawn.RaceProps.Humanlike && pawn.RaceProps.trainability == TrainabilityDefOf.Intermediate)
            {
                pawn.playerSettings.Master = parent.pawn;
                pawn.training.HasLearned(TrainableDefOf.Tameness);
                pawn.training.HasLearned(TrainableDefOf.Obedience);
                pawn.training.HasLearned(TrainableDefOf.Release);
            }

            if (!pawn.RaceProps.Humanlike && pawn.RaceProps.trainability == TrainabilityDefOf.Advanced)
            {
                pawn.training.HasLearned(DefDatabase<TrainableDef>.GetNamed("Rescue"));
                pawn.training.HasLearned(DefDatabase<TrainableDef>.GetNamed("Haul"));
            }

            if (pawn != null)
            {
                GenPlace.TryPlaceThing(pawn, target.Cell, map, ThingPlaceMode.Near);
                if (p.setKillSwitch && !p.destroywhenKills)
                {
                    SetKillSwitch(pawn);
                }
                else if (p.setKillSwitch && p.destroywhenKills)
                {
                    SetDestroySwitch(pawn);
                }
            }
            if (p.stateDef != null || p.stateDefForMechs != null)
            {
                CompAbilityEffect_GiveMentalState.TryGiveMentalState(pawn.RaceProps.IsMechanoid ? (p.stateDefForMechs ?? p.stateDef) : p.stateDef, pawn, parent.def, p.durationMultiplier, parent.pawn, p.forced);
                RestUtility.WakeUp(pawn);
                if (p.casterEffect != null)
                {
                    Effecter effecter = p.casterEffect.SpawnAttached(parent.pawn, parent.pawn.MapHeld);
                    effecter.Trigger(parent.pawn, null);
                    effecter.Cleanup();
                }

                if (p.targetEffect != null)
                {
                    Effecter effecter2 = p.targetEffect.SpawnAttached(parent.pawn, parent.pawn.MapHeld);
                    effecter2.Trigger(pawn, null);
                    effecter2.Cleanup();
                }
            }
        }

        private void SetKillSwitch(Pawn summoned)
        {
            Hediff hediff = HediffMaker.MakeHediff(HediffDef.Named("EMWH_Summoned"), summoned);
            HediffComp_DisappearsAndKills comp = hediff.TryGetComp<HediffComp_DisappearsAndKills>();
            if (comp == null)
            {
                return;
            }
            if (p.secondsToDisappear > 0)
            {
                comp.SetDuration(p.secondsToDisappear * 60);
            }
            else if (p.ticksToDisappear > 0)
            {
                comp.SetDuration(p.ticksToDisappear);
            }
            else
            {
                comp.SetDuration(1250);
            }
            summoned.health.AddHediff(hediff, summoned.health.hediffSet.GetBrain());
        }

        private void SetDestroySwitch(Pawn summoned)
        {
            Hediff hediff = HediffMaker.MakeHediff(HediffDef.Named("EMWH_Summoned_Destroy"), summoned);
            HediffComp_DisappearsAndDestroys comp = hediff.TryGetComp<HediffComp_DisappearsAndDestroys>();
            if (comp == null)
            {
                return;
            }
            if (p.secondsToDisappear > 0)
            {
                comp.SetDuration(p.secondsToDisappear * 60);
            }
            else if (p.ticksToDisappear > 0)
            {
                comp.SetDuration(p.ticksToDisappear);
            }
            else
            {
                comp.SetDuration(1250);
            }
            summoned.health.AddHediff(hediff, summoned.health.hediffSet.GetBrain());
        }

        protected virtual bool TryResist(Pawn pawn) => false;

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            if (parent.pawn.Faction == Faction.OfPlayer)
            {
                return false;
            }
            if (p.pawnKind.RaceProps.IsMechanoid && parent.pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.MechlinkImplant) == null)
            {
                return false;
            }
            if (!target.Cell.Filled(parent.pawn.Map) || target.Cell.GetEdifice(parent.pawn.Map) == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}