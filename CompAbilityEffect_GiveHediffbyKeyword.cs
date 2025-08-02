using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using MIM40kFactions.GenestealerCult;

namespace MIM40kFactions
{
    public class CompAbilityEffect_GiveHediffbyKeyword : CompAbilityEffect_WithDuration
    {
        public new CompProperties_AbilityGiveHediffbyKeyword Props => (CompProperties_AbilityGiveHediffbyKeyword)props;

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

            if (!Props.ignoreSelf || target.Pawn != parent.pawn)
            {
                if (!Props.onlyApplyToSelf && Props.applyToTarget)
                {
                    ApplyInner(target.Pawn, parent.pawn);
                }

                if (Props.applyToSelf || Props.onlyApplyToSelf)
                {
                    ApplyInner(parent.pawn, target.Pawn);
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
            }

            return true;
        }
        protected void ApplyInner(Pawn target, Pawn other)
        {
            if (target == null)
            {
                return;
            }

            if (TryResist(target))
            {
                MoteMaker.ThrowText(target.DrawPos, target.Map, "Resisted".Translate());
                return;
            }

            if (Props.replaceExisting)
            {
                Hediff firstHediffOfDef = target.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef);
                if (firstHediffOfDef != null)
                {
                    target.health.RemoveHediff(firstHediffOfDef);
                }
            }
            if (Props.criticalEffect != null)
            {
                ApplyHediff(target, other, RandomEffect(target));
                GenestealersKissFactionAdjustment(target);
            }
            else
            {
                ApplyHediff(target, other, FixedEffect(target));
                GenestealersKissFactionAdjustment(target);
            }
        }
        protected virtual bool TryResist(Pawn pawn)
        {
            return false;
        }
        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            if (parent.pawn.Faction == Faction.OfPlayer)
            {
                return false;
            }

            return target.Pawn != null;
        }
        private bool PawnValidator(Pawn target)
        {
            // Faction Validation
            if (!Utility_PawnValidationManager.FactionValidator(target, parent.pawn, Props.onlyTargetNotinSameFactions, Props.onlyTargetHostileFactions, Props.onlyPawnsInSameFaction, Props.onlyTargetNonPlayerFactions))
            {
                return false;
            }

            // Xenotype Validation if Biotech is active
            if (!Utility_PawnValidationManager.XenotypeValidator(target, Props.excludeXenotype, Props.excludeXenotypes, Props.targetXenotype, Props.targetXenotypes))
            {
                return false;
            }

            // Hediff Validation
            if (!Utility_PawnValidationManager.HediffValidator(target, Props.requiredHediff, Props.requiredHediffs))
            {
                return false;
            }

            // Keyword Validation
            if (Props.useKeyword && !Utility_PawnValidationManager.KeywordValidator(target, Props.keywords, Props.isVehicle, Props.isMonster, Props.isPsychic, Props.isPsyker, Props.isCharacter, Props.isAstartes, Props.isInfantry, Props.isWalker, Props.isLeader, Props.isFly, Props.isAircraft, Props.isChaos, Props.isDaemon, Props.isDestroyerCult, Props.isHereticAstartes))
            {
                return false;
            }
            return true;
        }
        private bool CalculateHazardous()
        {
            int isHazardous;
            isHazardous = Rand.RangeInclusive(1, 6);
            if (Props.isHazardous && isHazardous == 1)
            {
                return true;
            }
            return false;
        }
        private Hediff RandomEffect(Pawn target)
        {
            int rand = 0;
            HediffDef hediffDef = new HediffDef();

            if (Props.isHazardous)
                Rand.RangeInclusive(2, 6);
            else
                Rand.RangeInclusive(1, 6);

            if (rand == 6)
            {
                hediffDef = Props.criticalEffect;
            }
            if (rand == 5)
            {
                hediffDef = Props.hediffDef;
            }
            if (rand == 4)
            {
                hediffDef = Props.hediffDef;
            }
            if (rand == 3)
            {
                hediffDef = Props.hediffDef;
            }
            if (rand == 2)
            {
                hediffDef = Props.hediffDef;
            }
            if (rand == 1)
            {
                if (Props.fumbleEffect != null)
                    hediffDef = Props.fumbleEffect;
                else
                    hediffDef = Props.hediffDef;
            }
            Hediff hediff = HediffMaker.MakeHediff(hediffDef, target, Props.onlyBrain ? target.health.hediffSet.GetBrain() : null);
            return hediff;
        }
        private Hediff FixedEffect(Pawn target)
        {
            Hediff hediff = HediffMaker.MakeHediff(Props.hediffDef, target, Props.onlyBrain ? target.health.hediffSet.GetBrain() : null);
            return hediff;
        }
        private void ApplyHediff(Pawn target, Pawn other, Hediff hediff)
        {
            HediffComp_Disappears hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();
            if (hediffComp_Disappears != null)
            {
                hediffComp_Disappears.ticksToDisappear = GetDurationSeconds(target).SecondsToTicks();
            }

            if (Props.severity >= 0f)
            {
                hediff.Severity = Props.severity;
            }

            HediffComp_Link hediffComp_Link = hediff.TryGetComp<HediffComp_Link>();
            if (hediffComp_Link != null)
            {
                hediffComp_Link.other = other;
                hediffComp_Link.drawConnection = target == parent.pawn;
            }

            target.health.AddHediff(hediff);
        }
        private void GenestealersKissFactionAdjustment(Pawn target)
        {
            if (!Utility_DependencyManager.IsGCCoreActive())
            {
                return;
            }
            if (target.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("EMGC_GSKiss_FirstGen")) != null)
            {
                GenestealersKissFactionExtension modExtension = target.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("EMGC_GSKiss_FirstGen")).def.GetModExtension<GenestealersKissFactionExtension>();
                if (modExtension != null)
                {
                    modExtension.faction = parent.pawn.Faction;
                }
            }
            else if (target.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("EMGC_GSKiss_SecondGen")) != null)
            {
                GenestealersKissFactionExtension modExtension = target.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("EMGC_GSKiss_SecondGen")).def.GetModExtension<GenestealersKissFactionExtension>();
                if (modExtension != null)
                {
                    modExtension.faction = parent.pawn.Faction;
                }
            }
            else if (target.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("EMGC_GSKiss_ThirdGen")) != null)
            {
                GenestealersKissFactionExtension modExtension = target.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("EMGC_GSKiss_ThirdGen")).def.GetModExtension<GenestealersKissFactionExtension>();
                if (modExtension != null)
                {
                    modExtension.faction = parent.pawn.Faction;
                }
            }
            else if (target.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("EMGC_GSKiss_ForthGen")) != null)
            {
                GenestealersKissFactionExtension modExtension = target.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("EMGC_GSKiss_ForthGen")).def.GetModExtension<GenestealersKissFactionExtension>();
                if (modExtension != null)
                {
                    modExtension.faction = parent.pawn.Faction;
                }
            }
        }
    }
}