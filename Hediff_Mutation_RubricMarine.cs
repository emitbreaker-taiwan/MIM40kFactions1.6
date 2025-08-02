using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions
{
    public class Hediff_Mutation_RubricMarine : Hediff_Level
    {
        public override void Tick()
        {
            base.Tick();

            if (pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("EMTS_WarpPotential")) != null || pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("EMTS_RubricofAhriman")) != null)
                return;

            ageTicks++;

            if (!Utility_DependencyManager.IsRimDarkActive() && pawn.health.hediffSet.GetFirstHediffOfDef(def).def == HediffDef.Named("EMCM_TSGeneSeed") && pawn.health.hediffSet.GetFirstHediffOfDef(def).Severity != 19)
                return;

            DoMutation(pawn);
            return;
        }
        private static void DoMutation(Pawn EMCM_TSvictim)
        {
            if (ModsConfig.BiotechActive == true)
            {
                Utility_RubricMarineMutation.DoMutationConsideration(EMCM_TSvictim);
            }

            if (ModsConfig.IdeologyActive == true)
            {
                Utility_RubricMarineMutation.DoIdeoConsideration(EMCM_TSvictim);
            }

            Utility_RubricMarineMutation.DoPostAction(EMCM_TSvictim);
        }
    }
}
