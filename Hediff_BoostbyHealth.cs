using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class Hediff_BoostbyHealth : HediffWithComps
    {
        public override void Tick()
        {
            base.Tick();
            HediffEffectExtension modExtension = def.GetModExtension<HediffEffectExtension>();
            Hediff hediffPositive = HediffMaker.MakeHediff(modExtension.positiveHediffDef, pawn, pawn.health.hediffSet.GetBrain());
            Hediff hediffNeative = HediffMaker.MakeHediff(modExtension.negativeHediffDef, pawn, pawn.health.hediffSet.GetBrain());
            if (pawn.health.summaryHealth.SummaryHealthPercent > 0.49f)
            {
                if (pawn.health.hediffSet.HasHediff(modExtension.negativeHediffDef))
                {
                    pawn.health.RemoveHediff(hediffNeative);
                }
                pawn.health.AddHediff(hediffPositive);
            }
            else if (pawn.health.summaryHealth.SummaryHealthPercent < 0.5f)
            {
                if (pawn.health.hediffSet.HasHediff(modExtension.positiveHediffDef))
                {
                    pawn.health.RemoveHediff(hediffPositive);
                }
                pawn.health.AddHediff(hediffNeative);
            }
        }
    }
}