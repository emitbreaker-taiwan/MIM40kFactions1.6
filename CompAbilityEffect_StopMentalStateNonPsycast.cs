using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class CompAbilityEffect_StopMentalStateNonPsycast : CompAbilityEffect
    {
        public CompProperties_AbilityStopMentalStateNonPsycast Attributes
        {
            get => (CompProperties_AbilityStopMentalStateNonPsycast)this.props;
        }

        public override bool HideTargetPawnTooltip => true;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn pawn = target.Pawn;
            Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.CatatonicBreakdown);

            if (!this.Attributes.canApplyToHostile && pawn.Faction != Faction.OfPlayer)
                return;

            if (!this.Attributes.applyToSelf && !this.Attributes.onlyApplyToSelf)
                return;

            else
            {
                if (firstHediffOfDef != null)
                    pawn.health.RemoveHediff(firstHediffOfDef);
                pawn?.MentalState?.RecoverFromState();
            }
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return this.Valid(target, false);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            Pawn pawn1 = target.Pawn;
            if (pawn1 != null)
            {
                if (!AbilityUtility.ValidateHasMentalState(pawn1, throwMessages, this.parent))
                    return false;
                if (pawn1.MentalStateDef != null && this.Attributes.exceptions.Contains(pawn1.MentalStateDef))
                {
                    if (throwMessages)
                        Messages.Message((string)"AbilityDoesntWorkOnMentalState".Translate((NamedArgument)this.parent.def.label, (NamedArgument)pawn1.MentalStateDef.label), (LookTargets)(Thing)pawn1, MessageTypeDefOf.RejectInput, false);
                    return false;
                }
            }
            return true;
        }

        protected virtual bool TryResist(Pawn pawn) => false;

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            if (parent.pawn.Faction == Faction.OfPlayer)
            {
                return false;
            }

            if (target.Pawn == null || target.Pawn.RaceProps.IsMechanoid || target.Pawn.IsMutant || target.Pawn.IsNonMutantAnimal)
                return false;

            if (target.Pawn.Faction != parent.pawn.Faction)
                return false;

            return target.Pawn != null;
        }
    }
}