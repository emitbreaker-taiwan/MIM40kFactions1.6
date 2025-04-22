using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions
{
    public class HediffGiver_WhenTargetHediffExists : HediffGiver
    {
        public HediffDef targethediffDef;

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            if (!Utility_PawnValidator.IsPawnDeadValidator(pawn))
            {
                return;
            }

            Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(hediff);
            Hediff targethediff = pawn.health.hediffSet.GetFirstHediffOfDef(targethediffDef);
            if (targethediff == null)
            {
                if (firstHediffOfDef != null)
                {
                    pawn.health.RemoveHediff(firstHediffOfDef);
                }
            }
            else if (TryApply(pawn))
            {
                //SendLetter(pawn, cause);
            }
        }
    }
}