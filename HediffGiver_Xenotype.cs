using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions
{
    public class HediffGiver_Xenotype : HediffGiver
    {
        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            if (!ModsConfig.BiotechActive)
            {
                return;
            }

            if (!Utility_PawnValidator.IsPawnDeadValidator(pawn))
            {
                return;
            }

            HediffCompProperties_GiveXenotype hediffComp = hediff.CompProps<HediffCompProperties_GiveXenotype>();
            if (hediffComp == null)
            {
                return;
            }

            if (pawn.genes.Xenotype == hediffComp.targetxenotypeDef)
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
