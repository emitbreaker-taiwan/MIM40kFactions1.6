using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using Verse;
using static HarmonyLib.Code;

namespace MIM40kFactions
{
    public class HediffComp_GiveGenes : HediffComp
    {
        public HediffCompProperties_GiveGenes Props => (HediffCompProperties_GiveGenes)props;

        private bool flag = false;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (!flag)
            {
                if (!Utility_PawnValidationManager.IsPawnDeadValidator(parent.pawn))
                {
                    return;
                }

                CheckandAddGene();
            }
        }

        private void CheckandAddGene()
        {
            if (!ModsConfig.BiotechActive)
            {
                return;
            }

            if (Props.geneDefs == null && Props.givegenesSets == null)
            {
                return;
            }

            if (!flag)
            {
                if (Props.geneDefs != null)
                {
                    foreach (GeneDef geneDef in Props.geneDefs)
                    {
                        if (!parent.pawn.genes.HasActiveGene(geneDef))
                        {
                            parent.pawn.genes.AddGene(geneDef, true);
                            flag = true;
                        }
                    }
                }

                if (Props.givegenesSets != null)
                {
                    foreach (GiveGenesSet geneDefSet in Props.givegenesSets)
                    {
                        if (geneDefSet.severity == null || parent.pawn.health.hediffSet.GetFirstHediffOfDef(parent.def).Severity >= geneDefSet.severity || (parent.pawn.health.hediffSet.GetFirstHediffOfDef(parent.def) is Hediff_Level hediff_Level && hediff_Level.level >= geneDefSet.severity))
                        {
                            if (!parent.pawn.genes.HasActiveGene(geneDefSet.geneDef))
                            {
                                parent.pawn.genes.AddGene(geneDefSet.geneDef, true);
                                flag = true;
                            }
                        }
                    }
                }
            }
        }

    }
}