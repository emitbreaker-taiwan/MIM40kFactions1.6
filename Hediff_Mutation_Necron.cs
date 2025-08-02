using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using Verse;

namespace MIM40kFactions.Necron
{
    public class Hediff_Mutation_Necron : HediffWithComps
    {
        public override void Tick()
        {
            if ((double)Severity == 1.0)
                return;

            ageTicks++;

            if (ModsConfig.BiotechActive && Utility_DependencyManager.IsNCCoreActive() && pawn.genes.Xenotype != Utility_XenotypeManager.XenotypeDefNamed("EMNC_Necrons"))
                pawn.genes.SetXenotype(Utility_XenotypeManager.XenotypeDefNamed("EMNC_Necrons"));
            Severity = 1;
        }
    }
}
