using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions
{
    public class Hediff_Mutation_Wulfen : Hediff_Level
    {
        public override void Tick()
        {
            base.Tick();

            if (pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("EMSM_SpaceWolves_CanisHelix")) != null)
                return;

            ageTicks++;

            if (!ModsConfig.IsActive("Phonicmas.40kGenes") && pawn.health.hediffSet.GetFirstHediffOfDef(def).def == HediffDef.Named("EMSM_SWGeneSeed") && pawn.health.hediffSet.GetFirstHediffOfDef(def).Severity != 19)
                return;
            DoMutation(pawn);
            return;
        }
        private static void DoMutation(Pawn EMSM_SWvictim)
        {
            if (ModsConfig.BiotechActive == true)
            {
                Utility_WolfenMutation.DoMutationConsideration(EMSM_SWvictim);
            }

            if (ModsConfig.IdeologyActive == true)
            {
                Utility_WolfenMutation.DoIdeoConsideration(EMSM_SWvictim);
            }

            Utility_WolfenMutation.DoPostAction(EMSM_SWvictim);
        }
    }
}
