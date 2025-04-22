using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using Verse;
using static System.Collections.Specialized.BitVector32;

namespace MIM40kFactions
{
    public class Hediff_GenestealersKiss : HediffWithComps
    {
        public override void Tick()
        {
            if (!ModsConfig.IsActive("emitbreaker.MIM.WH40k.GC.Core"))
            {
                return;
            }

            if (AnyGeneMakesFullyImmuneTo(def))
            {
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(def);
                pawn.health.RemoveHediff(hediff);
                return;
            }

            ++ageTicks;

            if ((double)Severity < 1.0)
            {
                return;
            }

            DoMutation(pawn);
        }
        
        private void DoMutation(Pawn EMGC_victim)
        {
            Faction faction = null;

            GenestealersKissFactionExtension modExtension = def.GetModExtension<GenestealersKissFactionExtension>();

            if (modExtension != null)
            {
                faction = modExtension.faction;
            }

            if (!EMGC_victim.RaceProps.Humanlike)
            {
                Utility_GenestealerMutation.RemoveTrigger(EMGC_victim);
                return;
            }

            if (ModsConfig.BiotechActive == true)
            {
                Utility_GenestealerMutation.DoMutationConsideration(EMGC_victim, faction);
            }

            if (ModsConfig.IdeologyActive == true)
            {
                Utility_GenestealerMutation.DoIdeoConsideration(EMGC_victim, faction);
            }

            Utility_GenestealerMutation.DoPostAction(EMGC_victim, faction);

        }

        private bool AnyGeneMakesFullyImmuneTo(HediffDef def)
        {
            if (!ModsConfig.BiotechActive || pawn.genes == null)
            {
                return false;
            }

            for (int i = 0; i < pawn.genes.GenesListForReading.Count; i++)
            {
                Gene gene = pawn.genes.GenesListForReading[i];
                if (gene.def.makeImmuneTo == null)
                {
                    continue;
                }

                for (int j = 0; j < gene.def.makeImmuneTo.Count; j++)
                {
                    if (gene.def.makeImmuneTo[j] == def)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
