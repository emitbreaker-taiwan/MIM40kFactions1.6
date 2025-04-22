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
    public class Hediff_BoostbyHediff : HediffWithComps
    {
        public override void Tick()
        {
            base.Tick();
            BoostbyHediffExtension modExtension = def.GetModExtension<BoostbyHediffExtension>();
            if (modExtension == null || modExtension.targetHediffDef == null)
            {
                return;
            }

            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(modExtension.targetHediffDef);
            if (hediff == null)
            {
                return;
            }

            if (hediff != null)
            {
                SeverityforhediffifTargetExisting(hediff);
            }
        }
        private void SeverityforhediffifTargetExisting(Hediff targetHediff)
        {
            if (targetHediff == null)
            {
                return;
            }
            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(def);
            float targetSeverity = targetHediff.Severity;
            if (hediff.Severity != targetSeverity)
            {
                hediff.Severity = targetSeverity;
            }
        }
    }
}