using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace MIM40kFactions
{
    public class CompAbilityEffect_GiveMentalStateAICanTarget : CompAbilityEffect
    {
        public new CompProperties_AbilityGiveMentalStateAICanTarget Props => (CompProperties_AbilityGiveMentalStateAICanTarget)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            List<Pawn> list = new List<Pawn>();
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(target.Cell, Props.range, true))
            {
                if (!cell.InBounds(parent.pawn.Map)) continue;
                Pawn pawn = cell.GetFirstPawn(parent.pawn.Map);
                if (pawn != null) list.Add(pawn);
            }

            foreach (Pawn pawn in list)
            {
                if (!Props.applyToSelf && pawn == parent.pawn)
                    return;
                if (pawn != null && !pawn.InMentalState)
                {
                    TryGiveMentalState(pawn.RaceProps.IsMechanoid ? (Props.stateDefForMechs ?? Props.stateDef) : Props.stateDef, pawn, parent.def, Props.durationMultiplier, parent.pawn, Props.forced);
                    RestUtility.WakeUp(pawn);
                    if (Props.casterEffect != null)
                    {
                        Effecter effecter = Props.casterEffect.SpawnAttached(parent.pawn, parent.pawn.MapHeld);
                        effecter.Trigger(parent.pawn, null);
                        //effecter.Cleanup();
                    }

                    if (Props.targetEffect != null)
                    {
                        Effecter effecter2 = Props.targetEffect.SpawnAttached(parent.pawn, parent.pawn.MapHeld);
                        effecter2.Trigger(pawn, null);
                        //effecter2.Cleanup();
                    }
                }
            }
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            Pawn pawn = target.Pawn;
            if (pawn != null)
            {
                if (!AbilityUtility.ValidateNoMentalState(pawn, throwMessages, parent))
                {
                    return false;
                }

                if (Props.excludeNPCFactions && pawn.Faction != null && !pawn.Faction.IsPlayer)
                {
                    if (throwMessages)
                    {
                        Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "TargetBelongsToNPCFaction".Translate(), pawn, MessageTypeDefOf.RejectInput, historical: false);
                    }

                    return false;
                }

                if (parent.pawn.Position.DistanceToSquared(target.Cell) > Props.range * Props.range)
                {
                    return false;
                }
            }

            return true;
        }

        public static void TryGiveMentalState(MentalStateDef def, Pawn p, AbilityDef ability, StatDef multiplierStat, Pawn caster, bool forced = false)
        {
            if (p.mindState.mentalStateHandler.TryStartMentalState(def, null, forced, forceWake: true, causedByMood: false, null, transitionSilently: false, causedByDamage: false, ability.IsPsycast))
            {
                float num = ability.GetStatValueAbstract(StatDefOf.Ability_Duration, caster);
                if (multiplierStat != null)
                {
                    num *= p.GetStatValue(multiplierStat);
                }

                if (num > 0f)
                {
                    p.mindState.mentalStateHandler.CurState.forceRecoverAfterTicks = num.SecondsToTicks();
                }

                p.mindState.mentalStateHandler.CurState.sourceFaction = caster.Faction;
            }
        }
        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            if (parent.pawn.Faction == Faction.OfPlayer)
            {
                return false;
            }

            return target.Pawn != null;
        }
    }
}