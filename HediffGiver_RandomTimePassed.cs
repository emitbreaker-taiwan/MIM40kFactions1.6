using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions
{
    public class HediffGiver_RandomTimePassed : HediffGiver
    {
        public int tickToWait = 0;
        public HediffDef hediffNullifier;

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            if (!Utility_PawnValidator.IsPawnDeadValidator(pawn))
            {
                return;
            }

            if (hediffNullifier == null || pawn.health.hediffSet.GetFirstHediffOfDef(hediffNullifier) == null)
            {
                if (pawn.IsHashIntervalTick(Rand.Range(60000, 60000 + tickToWait)))
                {
                    Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(hediff);
                    if (firstHediffOfDef != null)
                    {
                        firstHediffOfDef.ageTicks = 0;
                    }
                    else if (TryApply(pawn))
                    {
                        SendLetter(pawn, cause);
                    }
                }
            }
            if (pawn.health.hediffSet.GetFirstHediffOfDef(hediffNullifier) != null)
            {
                pawn.IsHashIntervalTick(Rand.Range(60000, 60000 + tickToWait));
                return;
            }
        }
    }
}