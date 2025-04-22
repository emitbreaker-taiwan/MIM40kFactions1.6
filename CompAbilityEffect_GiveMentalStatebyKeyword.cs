using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class CompAbilityEffect_GiveMentalStatebyKeyword : CompAbilityEffect
    {
        public new CompProperties_AbilityGiveMentalStatebyKeyword Props => (CompProperties_AbilityGiveMentalStatebyKeyword)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn pawn = (Props.applyToSelf ? parent.pawn : (target.Thing as Pawn));
            if (pawn != null && !pawn.InMentalState && FactionValidator(pawn))
            {
                TryGiveMentalState(pawn.RaceProps.IsMechanoid ? (Props.stateDefForMechs ?? Props.stateDef) : Props.stateDef, pawn, parent.def, Props.durationMultiplier, parent.pawn, Props.forced);
                RestUtility.WakeUp(pawn);
                if (Props.casterEffect != null)
                {
                    Effecter effecter = Props.casterEffect.SpawnAttached(parent.pawn, parent.pawn.MapHeld);
                    effecter.Trigger(parent.pawn, null);
                    effecter.Cleanup();
                }

                if (Props.targetEffect != null)
                {
                    Effecter effecter2 = Props.targetEffect.SpawnAttached(parent.pawn, parent.pawn.MapHeld);
                    effecter2.Trigger(pawn, null);
                    effecter2.Cleanup();
                }
            }
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (!base.Valid(target, throwMessages))
            {
                return false;
            }

            Pawn pawn = target.Pawn;
            if (pawn != null)
            {
                if (!AbilityUtility.ValidateNoMentalState(pawn, throwMessages, parent))
                {
                    return false;
                }

                if (!Props.onlyApplyToSelf)
                {
                    if (Props.onlyTargetNotinSameFactions && parent.pawn.Faction == pawn.Faction)
                    {
                        if (throwMessages)
                        {
                            Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "EMWH_TargetBelongsToSameFaction".Translate(), pawn, MessageTypeDefOf.RejectInput, historical: false);
                        }

                        return false;
                    }

                    if (Props.onlyTargetHostileFactions && !parent.pawn.Faction.HostileTo(pawn.Faction))
                    {
                        if (throwMessages)
                        {
                            Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "EMWH_TargetNotBelongsToHostileFaction".Translate(), pawn, MessageTypeDefOf.RejectInput, historical: false);
                        }

                        return false;
                    }

                    if (Props.onlyPawnsInSameFaction && parent.pawn.Faction != pawn.Faction)
                    {
                        if (throwMessages)
                        {
                            Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "EMWH_TargetNotBelongsToSameFaction".Translate(), pawn, MessageTypeDefOf.RejectInput, historical: false);
                        }

                        return false;
                    }

                    if (Props.onlyTargetNonPlayerFactions && pawn.Faction == Faction.OfPlayer)
                    {
                        if (throwMessages)
                        {
                            Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "EMWH_TargetBelongsToPlayerFaction".Translate(), pawn, MessageTypeDefOf.RejectInput, historical: false);
                        }

                        return false;
                    }
                }

                if (Props.excludeNPCFactions && pawn.Faction != null && !pawn.Faction.IsPlayer)
                {
                    if (throwMessages)
                    {
                        Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "TargetBelongsToNPCFaction".Translate(), pawn, MessageTypeDefOf.RejectInput, historical: false);
                    }

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
        private bool FactionValidator(Pawn targ)
        {
            Faction casterFaction = parent.pawn.Faction;
            Faction targetFaction = targ.Faction;
            if (Props.onlyTargetNotinSameFactions && casterFaction != targetFaction)
                return true;

            if (Props.onlyTargetHostileFactions && targ.HomeFaction.HostileTo(casterFaction))
                return true;

            if (Props.onlyPawnsInSameFaction && casterFaction == targetFaction)
                return true;

            if (Props.onlyTargetNonPlayerFactions && targetFaction != Faction.OfPlayer)
                return true;
            return false;
        }
    }
}