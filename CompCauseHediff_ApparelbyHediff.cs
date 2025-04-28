using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class CompCauseHediff_ApparelbyHediff : ThingComp
    {
        private CompProperties_CauseHediff_ApparelbyHediff Props => (CompProperties_CauseHediff_ApparelbyHediff)props;

        public override void Notify_Equipped(Pawn pawn)
        {
            if (!pawn.Spawned)
            {
                return;
            }

            if (pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediff) == null)
            {
                if (Props.hediffRequired != null)
                {
                    foreach (HediffDef hediffDef in Props.hediffRequired)
                    {
                        if (pawn.health.hediffSet.HasHediff(hediffDef))
                        {
                            ApplyHediff(pawn);
                            break; // Apply the Hediff only once, exit the loop
                        }
                    }
                }
                if (Props.hediffIfNotExists != null)
                {
                    foreach (HediffDef hediffDef in Props.hediffIfNotExists)
                    {
                        if (!pawn.health.hediffSet.HasHediff(hediffDef))
                        {
                            if (ModsConfig.IsActive("Phonicmas.40kGenes") && pawn.genes.HasActiveGene(Utility_GeneManager.GeneDefNamed("BEWH_BlackCarapace")) && Props.hediffIfNotExists.Contains(HediffDef.Named("EMWH_BlackCarapace")))
                            {
                                continue;
                            }
                            else
                            {
                                ApplyHediff(pawn);
                                break; // Apply the Hediff only once, exit the loop
                            }
                        }
                    }
                }
            }
        }

        private void ApplyHediff(Pawn pawn)
        {
            HediffComp_RemoveIfApparelDropped hediffComp_RemoveIfApparelDropped = pawn.health.AddHediff(Props.hediff, pawn.health.hediffSet.GetNotMissingParts().FirstOrFallback((BodyPartRecord p) => p.def == Props.part)).TryGetComp<HediffComp_RemoveIfApparelDropped>();
            if (hediffComp_RemoveIfApparelDropped != null)
            {
                hediffComp_RemoveIfApparelDropped.wornApparel = (Apparel)parent;
            }
        }
    }
}