using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.Sound;

namespace MIM40kFactions
{
    public class CompCauseHediff_AoEbyKeyword : ThingComp
    {
        public float range;

        private Sustainer activeSustainer;

        private bool lastIntervalActive;

        public CompProperties_CauseHediff_AoEbyKeyword Props => (CompProperties_CauseHediff_AoEbyKeyword)props;

        private CompPowerTrader PowerTrader => parent.TryGetComp<CompPowerTrader>();

        private bool IsPawnAffected(Pawn target)
        {
            if (PowerTrader != null && !PowerTrader.PowerOn)
            {
                return false;
            }

            if (target.Dead || target.health == null)
            {
                return false;
            }

            if (target == parent && !Props.canTargetSelf)
            {
                return false;
            }

            if (Props.ignoreMechs && target.RaceProps.IsMechanoid)
            {
                return false;
            }

            if (!PawnValidator(target))
            {
                return false;
            }

            if (!Props.onlyTargetMechs || target.RaceProps.IsMechanoid)
            {
                return target.PositionHeld.DistanceTo(parent.PositionHeld) <= range;
            }

            return false;
        }
        private bool PawnValidator(Pawn target)
        {
            // Faction Validation
            if (!Utility_PawnValidationManager.FactionValidator(target, parent, Props.onlyTargetNotinSameFactions, Props.onlyTargetHostileFactions, Props.onlyPawnsInSameFaction, Props.onlyTargetNonPlayerFactions))
            {
                return false;
            }

            // Xenotype Validation if Biotech is active
            if (!Utility_PawnValidationManager.XenotypeValidator(target, Props.excludeXenotype, Props.excludeXenotypes, Props.targetXenotype, Props.targetXenotypes))
            {
                return false;
            }

            // Hediff Validation
            if (!Utility_PawnValidationManager.HediffValidator(target, Props.requiredHediff, Props.requiredHediffs))
            {
                return false;
            }

            // Keyword Validation
            if (Props.useKeyword && !Utility_PawnValidationManager.KeywordValidator(target, Props.keywords, Props.isVehicle, Props.isMonster, Props.isPsychic, Props.isPsyker, Props.isCharacter, Props.isAstartes, Props.isInfantry, Props.isWalker, Props.isLeader, Props.isFly, Props.isAircraft, Props.isChaos, Props.isDaemon, Props.isDestroyerCult, Props.isHereticAstartes))
            {
                return false;
            }
            return true;
        }
        public override void PostPostMake()
        {
            base.PostPostMake();
            range = Props.range;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref range, "range", 0f);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && range <= 0f)
            {
                range = Props.range;
            }
        }

        public override void CompTick()
        {
            MaintainSustainer();
            if (!parent.IsHashIntervalTick(Props.checkInterval))
            {
                return;
            }

            CompPowerTrader compPowerTrader = parent.TryGetComp<CompPowerTrader>();
            if (compPowerTrader != null && !compPowerTrader.PowerOn)
            {
                return;
            }

            lastIntervalActive = false;
            if (!parent.SpawnedOrAnyParentSpawned)
            {
                return;
            }

            foreach (Pawn item in parent.MapHeld.mapPawns.AllPawnsSpawned)
            {
                if (IsPawnAffected(item))
                {
                    GiveOrUpdateHediff(item);
                }

                Pawn target;
                if ((target = item.carryTracker.CarriedThing as Pawn) != null && IsPawnAffected(target))
                {
                    GiveOrUpdateHediff(target);
                }
            }
        }

        private void GiveOrUpdateHediff(Pawn target)
        {
            if (Props.hediff != null)
            {
                if ((Props.hediffforMech != null || Props.hediffsforMech != null) && target.RaceProps.IsMechanoid)
                    return;
                if (Props.hediffs != null)
                    return;
                Hediff hediff = target.health.hediffSet.GetFirstHediffOfDef(Props.hediff);
                if (hediff == null)
                {
                    hediff = target.health.AddHediff(Props.hediff, target.health.hediffSet.GetBrain());
                    hediff.Severity = 1f;
                    HediffComp_Link hediffComp_Link = hediff.TryGetComp<HediffComp_Link>();
                    if (hediffComp_Link != null)
                    {
                        hediffComp_Link.drawConnection = false;
                        hediffComp_Link.other = parent;
                    }
                }
                HediffValidator(hediff);
            }
            if (Props.hediffs != null)
            {
                if ((Props.hediffforMech != null || Props.hediffsforMech != null) && target.RaceProps.IsMechanoid)
                    return;
                foreach (HediffDef hediffdef in Props.hediffs)
                {
                    Hediff hediff = target.health.hediffSet.GetFirstHediffOfDef(hediffdef);
                    if (hediff == null)
                    {
                        hediff = target.health.AddHediff(hediffdef, target.health.hediffSet.GetBrain());
                        hediff.Severity = 1f;
                        HediffComp_Link hediffComp_Link = hediff.TryGetComp<HediffComp_Link>();
                        if (hediffComp_Link != null)
                        {
                            hediffComp_Link.drawConnection = false;
                            hediffComp_Link.other = parent;
                        }
                    }
                    HediffValidator(hediff);
                }
            }
            if (Props.hediffforMech != null)
            {
                if (!target.RaceProps.IsMechanoid)
                    return;
                if (Props.hediffforMech != null)
                    return;
                Hediff hediff = target.health.hediffSet.GetFirstHediffOfDef(Props.hediffforMech);
                if (hediff == null)
                {
                    hediff = target.health.AddHediff(Props.hediff, target.health.hediffSet.GetBrain());
                    hediff.Severity = 1f;
                    HediffComp_Link hediffComp_Link = hediff.TryGetComp<HediffComp_Link>();
                    if (hediffComp_Link != null)
                    {
                        hediffComp_Link.drawConnection = false;
                        hediffComp_Link.other = parent;
                    }
                }
                HediffValidator(hediff);
            }
            if (Props.hediffsforMech != null)
            {
                if (!target.RaceProps.IsMechanoid)
                    return;
                foreach (HediffDef hediffdef in Props.hediffsforMech)
                {
                    Hediff hediff = target.health.hediffSet.GetFirstHediffOfDef(hediffdef);
                    if (hediff == null)
                    {
                        hediff = target.health.AddHediff(hediffdef, target.health.hediffSet.GetBrain());
                        hediff.Severity = 1f;
                        HediffComp_Link hediffComp_Link = hediff.TryGetComp<HediffComp_Link>();
                        if (hediffComp_Link != null)
                        {
                            hediffComp_Link.drawConnection = false;
                            hediffComp_Link.other = parent;
                        }
                    }
                    HediffValidator(hediff);
                }
            }

            lastIntervalActive = true;
        }

        private void HediffValidator(Hediff hediff)
        {
            HediffComp_Disappears hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();
            if (hediffComp_Disappears == null)
            {
                Log.ErrorOnce("CompCauseHediff_AoE has a hediff in props which does not have a HediffComp_Disappears", 78945945);
            }
            else
            {
                hediffComp_Disappears.ticksToDisappear = Props.checkInterval + 5;
            }
        }

        private void MaintainSustainer()
        {
            if (lastIntervalActive && Props.activeSound != null)
            {
                if (activeSustainer == null || activeSustainer.Ended)
                {
                    activeSustainer = Props.activeSound.TrySpawnSustainer(SoundInfo.InMap(new TargetInfo(parent)));
                }

                activeSustainer.Maintain();
            }
            else if (activeSustainer != null)
            {
                activeSustainer.End();
                activeSustainer = null;
            }
        }

        public override void PostDraw()
        {
            if (!Props.drawLines)
            {
                return;
            }

            int num = Mathf.Max(parent.Map.Size.x, parent.Map.Size.y);
            if (!Find.Selector.SelectedObjectsListForReading.Contains(parent) || !(range < (float)num))
            {
                return;
            }

            foreach (Pawn item in parent.Map.mapPawns.AllPawnsSpawned)
            {
                if (IsPawnAffected(item))
                {
                    GenDraw.DrawLineBetween(item.DrawPos, parent.DrawPos);
                }
            }
        }
    }
}
