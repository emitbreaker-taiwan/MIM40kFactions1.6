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
    public class HediffComp_GiveHediffs : HediffComp
    {
        public HediffCompProperties_GiveHediffs Props => (HediffCompProperties_GiveHediffs)props;
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            if (!Utility_PawnValidationManager.IsPawnDeadValidator(parent.pawn))
            {
                if (parent != null)
                {
                    parent.pawn.health.RemoveHediff(parent);
                }
                return;
            }

            if (Props.hediffOption == null && Props.hediffOptions == null)
            {
                return;
            }

            if (Props.severityAmount == null || parent.pawn.health.hediffSet.GetFirstHediffOfDef(parent.def).Severity >= Props.severityAmount)
            {
                if (Props.hediffOptions != null)
                {
                    foreach (HediffOption option in Props.hediffOptions)
                    {
                        DoSingleHediffOption(option, parent.pawn);
                        {
                            return;
                        }
                    }
                }
                if (Props.hediffOption != null)
                {
                    DoSingleHediffOption(Props.hediffOption, parent.pawn);
                    {
                        return;
                    }
                }
            }
            else return;
        }
        private void DoSingleHediffOption(HediffOption option, Pawn pawn)
        {
            if (Props.givetoAllbodyparts)
            {
                DoAddHediffs(option, pawn);
            }
            else
                parent.pawn.health.AddHediff(option.hediffDef, pawn.health.hediffSet.GetBodyPartRecord(option.bodyPart));
        }
        private void DoAddHediffs(HediffOption option, Pawn pawn)
        {
            List<BodyPartRecord> list = Utility_HediffManagement.GetBodyPartRecords(option.bodyPart, pawn);
            foreach (BodyPartRecord record in list)
            {
                Hediff hediff = HediffMaker.MakeHediff(option.hediffDef, pawn, record);
                parent.pawn.health.AddHediff(hediff);
            }
        }
    }
}