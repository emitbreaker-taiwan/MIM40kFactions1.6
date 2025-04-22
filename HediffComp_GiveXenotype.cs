using Verse;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MIM40kFactions
{
    public class HediffComp_GiveXenotype : HediffComp
    {
        public HediffCompProperties_GiveXenotype Props => (HediffCompProperties_GiveXenotype)props;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            if (Props.targetRaceDef != null && parent.pawn.kindDef.race == ThingDefOf.Human)
            {
                if (ModsConfig.BiotechActive && parent.pawn.genes.Xenotype == XenotypeDefOf.Baseliner)
                {
                    parent.pawn.kindDef.race = Props.targetRaceDef;
                }
            }
            if (ModsConfig.BiotechActive)
            {
                if (parent.pawn.genes.Xenotype == null)
                {
                    return;
                }
                if (Props.targetxenotypeDef == null && Props.targetxenotypeDefs == null)
                {
                    return;
                }
                if (Props.severityAmount == null || parent.pawn.health.hediffSet.GetFirstHediffOfDef(parent.def).Severity >= Props.severityAmount)
                {
                    if (parent.pawn.genes.Xenotype != XenotypeDefOf.Baseliner)
                    {
                        XenotypeDef targetXenotype;
                        if (Props.targetxenotypeDefs != null)
                        {
                            targetXenotype = XenotypeSelector(Props.targetxenotypeDefs);
                        }
                        else
                        {
                            targetXenotype = Props.targetxenotypeDef;
                        }
                        for (int i = 0; i < targetXenotype.genes.Count; i++)
                        {
                            parent.pawn.genes.AddGene(targetXenotype.genes[i], true);
                        }
                        parent.pawn.genes.SetXenotype(targetXenotype);
                    }
                    else
                    {
                        if (Props.targetxenotypeDefs != null)
                        {
                            parent.pawn.genes.SetXenotype(XenotypeSelector(Props.targetxenotypeDefs));
                        }
                        else
                        {
                            parent.pawn.genes.SetXenotype(Props.targetxenotypeDef);
                        }
                    }
                }
                if (Props.removeafterSetXenotype)
                {
                    parent.pawn.health.RemoveHediff(parent);
                }
            }
            return;
        }
        public XenotypeDef XenotypeSelector(List<XenotypeDef> targetxenotypeDefs)
        {
            int diceGod = Rand.Range(0, targetxenotypeDefs.Count);
            XenotypeDef selectedXenotypeDef = targetxenotypeDefs[diceGod];
            return selectedXenotypeDef;
        }
    }
}