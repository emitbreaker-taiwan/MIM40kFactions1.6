using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class CompAbilityEffect_RemoveHediffs : CompAbilityEffect
    {
        public CompProperties_AbilityRemoveHediffs p => (CompProperties_AbilityRemoveHediffs)props;
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            if (p.applyToSelf)
                RemoveHediffs(parent.pawn);
            if (target.Pawn == null || !p.applyToTarget || target.Pawn == parent.pawn)
                return;
            KeywordExtension modExtension = target.Pawn.kindDef.GetModExtension<KeywordExtension>();
            if (p.useKeyword && (modExtension == null || !!Utility_PawnValidationManager.KeywordValidator(target.Pawn, p.keywords, p.isVehicle, p.isMonster, p.isPsychic, p.isPsyker, p.isCharacter, p.isAstartes, p.isInfantry, p.isWalker, p.isLeader, p.isFly, p.isAircraft, p.isChaos, p.isDaemon, p.isDestroyerCult, p.isHereticAstartes)))
                return;
            else RemoveHediffs(target.Pawn);
        }

        private void RemoveHediffsTemp(Pawn pawn)
        {
            List<HediffDef> injuryDefList = new List<HediffDef>();
            if (p.specificHediffs != null)
                injuryDefList = p.specificHediffs;
            else
            {
                foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                {
                    if (hediff.def.isBad)
                        injuryDefList.Add(hediff.def);
                    else if (hediff.def.isInfection)
                        injuryDefList.Add(hediff.def);
                    else if (hediff.def.tendable)
                        injuryDefList.Add(hediff.def);

                    if (hediff.def == HediffDef.Named("EMDG_NurglesRot"))
                        injuryDefList.Remove(hediff.def);
                    else if (hediff.def == HediffDef.Named("EMTS_Mutation_Tzaangor"))
                        injuryDefList.Remove(hediff.def);
                    else if (hediff.def == HediffDef.Named("EMDG_Mutation_WalkingPox"))
                        injuryDefList.Remove(hediff.def);
                }
            }
            Hediff hediffToBeRemoved;
            for (int num = injuryDefList.Count - 1; num >= 0; num--)
            {
                hediffToBeRemoved = pawn.health.hediffSet.GetFirstHediffOfDef(injuryDefList[num]);
                pawn.health.RemoveHediff(hediffToBeRemoved);
            }
        }
        private void RemoveHediffs(Pawn pawn)
        {
            List<Hediff> injuryList = GetInjuries(pawn);
            Hediff hediffToBeRemoved;

            for (int num = injuryList.Count - 1; num >= 0; num--)
            {
                hediffToBeRemoved = pawn.health.hediffSet.GetFirstHediffOfDef(injuryList[num].def);
                pawn.health.RemoveHediff(hediffToBeRemoved);
            }
        }
        private List<Hediff> GetInjuries(Pawn pawn)
        {
            List<Hediff> injuryList = new List<Hediff>();
            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                if (p.specificHediffs != null)
                {
                    if (p.specificHediffs.Contains(hediff.def))
                    {
                        injuryList.Add(hediff);
                    }
                }
                else
                {
                    Hediff_Injury hediff_Injury;
                    if ((hediff_Injury = hediff as Hediff_Injury) != null && hediff_Injury.def.tendable)
                    {
                        injuryList.Add(hediff_Injury);
                    }

                    if (hediff.def == HediffDefOf.BloodLoss || hediff.def == HediffDefOf.ToxicBuildup || hediff.def == HediffDefOf.PollutionStimulus || hediff.def == HediffDefOf.ToxGasExposure || hediff.def == HediffDefOf.FoodPoisoning || hediff.def == HediffDefOf.LungRot || hediff.def == HediffDefOf.LungRotExposure || hediff.def == HediffDefOf.Plague || hediff.def == HediffDefOf.WoundInfection || hediff.def == HediffDefOf.Carcinoma || hediff.def == HediffDefOf.Dementia)
                    {
                        injuryList.Add(hediff);
                    }

                    if (hediff.def.tendable && hediff.Visible && !injuryList.Contains(hediff))
                    {
                        injuryList.Add(hediff);
                    }
                }
            }
            return injuryList;
        }
        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            Pawn Pawn = target.Pawn;
            if (Pawn == null || Pawn.IsDessicated() || Pawn.Dead)
            {
                return false;
            }
            if (Pawn != parent.pawn && (Pawn.health.InPainShock || Pawn.health.Downed || Pawn.Crawling))
            {
                return true;
            }
            if (p.specificHediffs == null && GetInjuries(Pawn).Count > 5)
            {
                return true;
            }
            if (p.specificHediffs != null && GetInjuries(Pawn).Count > 0)
            {
                return true;
            }
            return false;
        }
    }
}