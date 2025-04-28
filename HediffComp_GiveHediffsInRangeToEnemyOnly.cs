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
    public class HediffComp_GiveHediffsInRangeToEnemyOnly : HediffComp
    {
        private Mote mote;
        public HediffCompProperties_GiveHediffsInRangeToEnemyOnly Props => (HediffCompProperties_GiveHediffsInRangeToEnemyOnly)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (!Utility_PawnValidationManager.IsPawnDeadValidator(parent.pawn))
            {
                if (parent != null)
                {
                    parent.pawn.health.RemoveHediff(parent);
                }
                return;
            }

            if (!Props.hideMoteWhenNotDrafted || parent.pawn.Drafted)
            {
                if (Props.mote != null && (mote == null || mote.Destroyed))
                    mote = MoteMaker.MakeAttachedOverlay(parent.pawn, Props.mote, Vector3.zero);
                if (mote != null)
                    mote.Maintain();
            }
            foreach (Pawn targ in parent.pawn.Map.mapPawns.AllPawnsSpawned)
            {
                if (targ.Faction != parent.pawn.Faction && !targ.Dead && targ.health != null && targ != parent.pawn && (double)targ.Position.DistanceTo(parent.pawn.Position) <= (double)Props.range && Props.targetingParameters.CanTarget((TargetInfo)targ))
                {
                    if (targ.Faction == parent.pawn.Faction)
                        return;

                    if (Props.onlyTargetHostileFactions && !targ.HomeFaction.HostileTo(parent.pawn.Faction))
                        return;

                    Hediff hd = targ.health.hediffSet.GetFirstHediffOfDef(Props.hediff);

                    if (hd == null)
                    {
                        hd = targ.health.AddHediff(Props.hediff, targ.health.hediffSet.GetBrain());
                        hd.Severity = Props.initialSeverity;
                        HediffComp_Link comp = hd.TryGetComp<HediffComp_Link>();
                        if (comp != null)
                        {
                            comp.drawConnection = true;
                            comp.other = parent.pawn;
                        }
                    }
                    HediffComp_Disappears comp1 = hd.TryGetComp<HediffComp_Disappears>();
                    if (comp1 == null)
                        Log.Error("HediffComp_GiveHediffsInRange has a hediff in props which does not have a HediffComp_Disappears");
                    else
                        comp1.ticksToDisappear = 5;
                }
            }
        }
    }
}