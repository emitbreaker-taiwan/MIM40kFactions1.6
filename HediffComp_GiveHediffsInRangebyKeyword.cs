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
    public class HediffComp_GiveHediffsInRangebyKeyword : HediffComp
    {
        public HediffCompProperties_GiveHediffsInRangebyKeyword Props => (HediffCompProperties_GiveHediffsInRangebyKeyword)props;

        private Mote mote;

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

            PropsConfigurationValidator();

            if (Props.isHazardous && CalculateHazardous())
            {
                IntVec3 currentTargetCell = parent.pawn.Position;
                GenExplosion.DoExplosion(currentTargetCell, parent.pawn.Map, 1f, DamageDefOf.Bomb, null, damAmount: 30);
                return;
            }
            if (!Props.hideMoteWhenNotDrafted || parent.pawn.Drafted)
            {
                if (Props.mote != null && (mote == null || mote.Destroyed))
                {
                    mote = MoteMaker.MakeAttachedOverlay(parent.pawn, Props.mote, Vector3.zero);
                }
                if (mote != null)
                {
                    mote.Maintain();
                }
            }

            IReadOnlyList<Pawn> readOnlyList = ((!Props.onlyPawnsInSameFaction || parent.pawn.Faction == null) ? parent.pawn.Map.mapPawns.AllPawnsSpawned : parent.pawn.Map.mapPawns.SpawnedPawnsInFaction(parent.pawn.Faction));
            foreach (Pawn target in readOnlyList)
            {
                if (target.Dead || target.health == null || !(target.Position.DistanceTo(parent.pawn.Position) <= Props.range) || !Props.targetingParameters.CanTarget(target))
                {
                    continue;
                }
                if (!Props.targetingParameters.canTargetSelf && target == parent.pawn)
                {
                    continue;
                }
                if (!PawnValidator(target))
                {
                    continue;
                }

                ApplyHediff(target, ChooseHediff());
            }
        }

        private bool PropsConfigurationValidator()
        {
            if (Props.onlyTargetHostileFactions && Props.onlyPawnsInSameFaction)
            {
                Log.Error("onlyTargetHostileFactions and onlyPawnsInSameFaction cannot be set together.");
                return false;
            }
            if (Props.onlyPawnsInSameFaction && Props.onlyTargetNonPlayerFactions)
            {
                Log.Error("onlyPawnsInSameFaction and onlyTargetNonPlayerFactions cannot be set together.");
                return false;
            }
            if (Props.onlyPawnsInSameFaction && Props.onlyTargetNotinSameFactions)
            {
                Log.Error("onlyPawnsInSameFaction and onlyTargetNotinSameFactions cannot be set together.");
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool PawnValidator(Pawn target)
        {
            // Faction Validation
            if (!Utility_PawnValidationManager.FactionValidator(target, parent.pawn, Props.onlyTargetNotinSameFactions, Props.onlyTargetHostileFactions, Props.onlyPawnsInSameFaction, Props.onlyTargetNonPlayerFactions))
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

        private bool CalculateHazardous()
        {
            int isHazardous = Rand.Range(1, 6);
            if (Props.isHazardous && isHazardous == 1)
            {
                return true;
            }
            return false;
        }

        private void ApplyHediff(Pawn target, HediffDef hediffDef)
        {
            Hediff hediff = target.health.hediffSet.GetFirstHediffOfDef(hediffDef);
            if (hediff == null)
            {
                hediff = target.health.AddHediff(hediffDef, target.health.hediffSet.GetBrain());
                hediff.Severity = Props.initialSeverity;
                HediffComp_Link hediffComp_Link = hediff.TryGetComp<HediffComp_Link>();
                if (hediffComp_Link != null)
                {
                    hediffComp_Link.drawConnection = true;
                    hediffComp_Link.other = parent.pawn;
                }
            }

            HediffComp_Disappears hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();
            if (hediffComp_Disappears == null)
            {
                Log.Error("HediffComp_GiveHediffsInRangebyKeyword has a hediff in props which does not have a HediffComp_Disappears");
            }
            else
            {
                hediffComp_Disappears.ticksToDisappear = 5;
            }
        }

        private HediffDef ChooseHediff(HediffDef hediffDef = null)
        {
            if (Props.criticalEffect != null)
            {
                hediffDef = RandomEffect();
            }
            else
            {
                hediffDef = FixedEffect();
            }
            return hediffDef;
        }

        private HediffDef RandomEffect(HediffDef hediffDef = null)
        {
            int rand = 0;

            if (Props.isHazardous)
            {
                rand = Rand.Range(2, 6);
            }
            else
            {
                rand = Rand.Range(1, 6);
            }

            if (rand == 6)
            {
                hediffDef = Props.criticalEffect;
            }
            if (rand == 5)
            {
                hediffDef = Props.hediff;
            }
            if (rand == 4)
            {
                hediffDef = Props.hediff;
            }
            if (rand == 3)
            {
                hediffDef = Props.hediff;
            }
            if (rand == 2)
            {
                hediffDef = Props.hediff;
            }
            if (rand == 1)
            {
                if (Props.fumbleEffect != null)
                {
                    hediffDef = Props.fumbleEffect;
                }
                else
                {
                    hediffDef = Props.hediff;
                }
            }
            return hediffDef;
        }

        private HediffDef FixedEffect(HediffDef hediffDef = null)
        {
            hediffDef = Props.hediff;
            return hediffDef;
        }
    }
}