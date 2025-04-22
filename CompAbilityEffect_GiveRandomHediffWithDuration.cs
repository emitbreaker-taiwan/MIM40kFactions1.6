using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace MIM40kFactions
{
    public class CompAbilityEffect_GiveRandomHediffWithDuration : CompAbilityEffect_WithDuration
    {
        public new CompProperties_AbilityGiveRandomHediffWithDuration Props => (CompProperties_AbilityGiveRandomHediffWithDuration)props;

        private Hediff selectedHediff;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {

            base.Apply(target, dest);
            if (!PawnValidator(target.Pawn))
            {
                return;
            }
            if (Props.isHazardous && CalculateHazardous())
            {
                IntVec3 currentTargetCell = parent.pawn.Position;
                GenExplosion.DoExplosion(currentTargetCell, parent.pawn.Map, 1f, DamageDefOf.Bomb, null, damAmount: 30);
                return;
            }
            if (!Props.onlyApplyToSelf && Props.applyToTarget)
            {
                selectedHediff = SetHediff(target.Pawn, Props.options);
                ApplyInner(target.Pawn, parent.pawn);
            }
            if (!Props.applyToSelf && !Props.onlyApplyToSelf)
                return;
            else
            {
                selectedHediff = SetHediff(parent.pawn, Props.options);
                ApplyInner(parent.pawn, target.Pawn);
            }
        }
        private Hediff SetHediff(Pawn pawn, List<HediffOption> options)
        {
            HediffOption option = GetApplicableHediffs(pawn, options).RandomElement<HediffOption>();
            BodyPartRecord partRecord = GetAcceptablePartsForHediff(pawn, option, Props.allowDuplicates).RandomElement<BodyPartRecord>();
            Hediff hediff = HediffMaker.MakeHediff(option.hediffDef, pawn, partRecord);
            return hediff;
        }
        private static List<HediffOption> GetApplicableHediffs(Pawn target, List<HediffOption> options, bool allowDuplicates = false)
        {
            List<HediffOption> applicableHediffs = new List<HediffOption>();
            foreach (HediffOption option in options)
            {
                if (!GetAcceptablePartsForHediff(target, option, allowDuplicates).EnumerableNullOrEmpty<BodyPartRecord>())
                    applicableHediffs.Add(option);
            }
            return applicableHediffs;
        }

        private static IEnumerable<BodyPartRecord> GetAcceptablePartsForHediff(Pawn target, HediffOption option, bool allowDuplicates = false)
        {
            return !allowDuplicates && target.health.hediffSet.hediffs.Where<Hediff>((Func<Hediff, bool>)(x => x.def == option.hediffDef)).Any<Hediff>() ? (IEnumerable<BodyPartRecord>)null : target.health.hediffSet.GetNotMissingParts().Where<BodyPartRecord>((Func<BodyPartRecord, bool>)(p => (option.bodyPart == null || p.def == option.bodyPart) && !target.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(p)));
        }

        protected void ApplyInner(Pawn target, Pawn other)
        {
            if (target == null)
                return;

            if (Props.replaceExisting)
            {
                Hediff firstHediffOfDef = target.health.hediffSet.GetFirstHediffOfDef(selectedHediff.def);
                if (firstHediffOfDef != null)
                {
                    target.health.RemoveHediff(firstHediffOfDef);
                }
            }

            HediffComp_Disappears hediffComp_Disappears = selectedHediff.TryGetComp<HediffComp_Disappears>();

            if (hediffComp_Disappears != null)
            {
                hediffComp_Disappears.ticksToDisappear = GetDurationSeconds(target).SecondsToTicks();
            }

            if (Props.severity >= 0f)
            {
                selectedHediff.Severity = Props.severity;
            }

            HediffComp_Link hediffComp_Link = selectedHediff.TryGetComp<HediffComp_Link>();
            if (hediffComp_Link != null)
            {
                hediffComp_Link.other = other;
                hediffComp_Link.drawConnection = target == parent.pawn;
            }

            target.health.AddHediff(selectedHediff);

            if (!SendLetter)
                return;
            Find.LetterStack.ReceiveLetter(Props.customLetterLabel.Formatted((NamedArgument)selectedHediff.def.LabelCap), Props.customLetterText.Formatted((NamedArgument)(Thing)parent.pawn, (NamedArgument)(Thing)target, (NamedArgument)selectedHediff.def.label), LetterDefOf.PositiveEvent, new LookTargets((Thing)target));
        }

        private bool CalculateHazardous()
        {
            int isHazardous;
            isHazardous = Rand.RangeInclusive(1, 6);
            if (Props.isHazardous && isHazardous == 1)
                return true;
            return false;
        }
        private bool PawnValidator(Pawn target)
        {
            // Faction Validation
            if (!Utility_PawnValidator.FactionValidator(target, parent.pawn, Props.onlyTargetNotinSameFactions, Props.onlyTargetHostileFactions, Props.onlyPawnsInSameFaction, Props.onlyTargetNonPlayerFactions))
            {
                return false;
            }

            // Xenotype Validation if Biotech is active
            if (!Utility_PawnValidator.XenotypeValidator(target, Props.excludeXenotype, Props.excludeXenotypes, Props.targetXenotype, Props.targetXenotypes))
            {
                return false;
            }

            // Hediff Validation
            if (!Utility_PawnValidator.HediffValidator(target, Props.requiredHediff, Props.requiredHediffs))
            {
                return false;
            }

            // Keyword Validation
            if (Props.useKeyword && !Utility_PawnValidator.KeywordValidator(target, Props.keywords, Props.isVehicle, Props.isMonster, Props.isPsychic, Props.isPsyker, Props.isCharacter, Props.isAstartes, Props.isInfantry, Props.isWalker, Props.isLeader, Props.isFly, Props.isAircraft, Props.isChaos, Props.isDaemon, Props.isDestroyerCult))
            {
                return false;
            }
            return true;
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