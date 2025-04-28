using Verse;

namespace MIM40kFactions
{
    public class HediffGiver_RaceBase : HediffGiver
    {
        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            if (!Utility_PawnValidationManager.IsPawnDeadValidator(pawn))
            {
                return;
            }

            if (TryApply(pawn))
            {
                //SendLetter(pawn, cause);
            }
        }
    }
}