﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace MIM40kFactions
{
    public class HediffGiver_MaxPsylink : HediffGiver
    {
        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            if (!ModsConfig.RoyaltyActive)
            {
                return;
            }

            if (Utility_DependencyManager.IsVPEActive())
            {
                return;
            }

            if (!Utility_PawnValidationManager.IsPawnDeadValidator(pawn))
            {
                return;
            }

            if (hediff == null)
                hediff = HediffDefOf.PsychicAmplifier;
            if (hediff != HediffDefOf.PsychicAmplifier)
                return;
            Hediff def = pawn.health.hediffSet.GetFirstHediffOfDef(hediff);
            if (def != null && (double)def.Severity == 6)
            {
                def.ageTicks = 0;
            }
            bool sendLetter = true;
            Hediff_Psylink mainPsylinkSource = pawn.GetMainPsylinkSource();
            if (mainPsylinkSource == null)
            {
                Hediff_Psylink hediffPsylink = (Hediff_Psylink)HediffMaker.MakeHediff(hediff, pawn);
                try
                {
                    hediffPsylink.suppressPostAddLetter = !sendLetter;
                    pawn.health.AddHediff((Hediff)hediffPsylink, pawn.health.hediffSet.GetBrain());
                    mainPsylinkSource = pawn.GetMainPsylinkSource();
                    mainPsylinkSource.ChangeLevel(6, sendLetter);
                }
                finally
                {
                    hediffPsylink.suppressPostAddLetter = false;
                }
            }
            else
                mainPsylinkSource.ChangeLevel(6, sendLetter);
        }
    }
}