using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Verse.AI;
using Verse.Sound;
using MIM40kFactions.Compatibility;

namespace MIM40kFactions
{    
    public class CompAbilityEffect_DeepStrike : CompAbilityEffect_WithDest
    {
        
        public static string SkipUsedSignalTag = "CompAbilityEffect.SkipUsed";

        public static string CanOnlyCastToSelf = "EMWH_Ability_DeepStrike_CanOnlyDeepStrikeSelf"; 

        public static string TooClosetoEnemy = "EMWH_Ability_DeepStrike_TooCloseToEnemy";

        public static string CannotDeepstriketoTarget = "EMWH_Ability_DeepStrike_CannotDeepStrikeTarget";

        public new CompProperties_AbilityDeepStrike Props => (CompProperties_AbilityDeepStrike)props;

        public override IEnumerable<PreCastAction> GetPreCastActions()
        {
            yield return new PreCastAction
            {
                action = delegate (LocalTargetInfo t, LocalTargetInfo d)
                {
                    if (!parent.def.HasAreaOfEffect && ModsConfig.RoyaltyActive)
                    {
                        Pawn pawn = t.Pawn;
                        if (pawn != null)
                        {
                            FleckCreationData dataAttachedOverlay = FleckMaker.GetDataAttachedOverlay(pawn, FleckDefOf.PsycastSkipFlashEntry, new Vector3(-0.5f, 0f, -0.5f));
                            dataAttachedOverlay.link.detachAfterTicks = 5;
                            pawn.Map.flecks.CreateFleck(dataAttachedOverlay);
                        }
                        else
                        {
                            FleckMaker.Static(t.CenterVector3, parent.pawn.Map, FleckDefOf.PsycastSkipFlashEntry);
                        }

                        FleckMaker.Static(d.Cell, parent.pawn.Map, FleckDefOf.PsycastSkipInnerExit);
                    }

                    if (Props.destination != AbilityEffectDestination.RandomInRange && ModsConfig.RoyaltyActive)
                    {
                        FleckMaker.Static(d.Cell, parent.pawn.Map, FleckDefOf.PsycastSkipOuterRingExit);
                    }

                    if (!parent.def.HasAreaOfEffect && ModsConfig.RoyaltyActive)
                    {
                        SoundDefOf.Psycast_Skip_Entry.PlayOneShot(new TargetInfo(t.Cell, parent.pawn.Map));
                        SoundDefOf.Psycast_Skip_Exit.PlayOneShot(new TargetInfo(d.Cell, parent.pawn.Map));
                    }
                },
                ticksAwayFromCast = 5
            };
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (!target.HasThing)
                return;

            base.Apply(target, dest);

            //Failsafe original code;
            //LocalTargetInfo destinationDeepstrike = GetDestination(dest.IsValid ? dest : target);

            //if (!destinationDeepstrike.IsValid)
            //    return;

            LocalTargetInfo destinationDeepstrike = dest.IsValid ? dest : FindDeepStrikeDestinationFor(parent.pawn, target.Cell);
            if (!dest.IsValid)
            {
                dest = GetAutoDestinationForAI();
            }
            if (!destinationDeepstrike.IsValid)
            {
                return;
            }

            Pawn pawn = parent.pawn;
            if (!parent.def.HasAreaOfEffect)
                parent.AddEffecterToMaintain(EffecterDefOf.Skip_Entry.Spawn(target.Thing, pawn.Map), target.Thing.Position, 60);
            else
                parent.AddEffecterToMaintain(EffecterDefOf.Skip_EntryNoDelay.Spawn(target.Thing, pawn.Map), target.Thing.Position, 60);

            if (Props.destination == AbilityEffectDestination.Selected)
                parent.AddEffecterToMaintain(EffecterDefOf.Skip_Exit.Spawn(destinationDeepstrike.Cell, pawn.Map), destinationDeepstrike.Cell, 60);
            else
                parent.AddEffecterToMaintain(EffecterDefOf.Skip_ExitNoDelay.Spawn(destinationDeepstrike.Cell, pawn.Map), destinationDeepstrike.Cell, 60);

            Pawn pawn2 = target.Thing as Pawn;
            pawn2.TryGetComp<CompCanBeDormant>()?.WakeUp();
            pawn2.Position = destinationDeepstrike.Cell;
            pawn2.stances.stunner.StunFor(Props.stunTicks.RandomInRange, parent.pawn, addBattleLog: false, showMote: false);
            pawn2.Notify_Teleported();
            SendDeepStrikeUsedSignal(pawn2.Position, pawn2);

            if (Props.destClamorType != null)
            {
                GenClamor.DoClamor(pawn, target.Cell, (float)Props.destClamorRadius, Props.destClamorType);
            }
        }

        //Added by 3/4/2025
        private LocalTargetInfo FindDeepStrikeDestinationFor(Pawn caster, IntVec3 origin)
        {
            Map map = caster.Map;
            for (int i = 0; i < 20; i++) // Try 20 random positions
            {
                IntVec3 cell = CellFinder.RandomClosewalkCellNear(origin, map, 8);
                if (cell.Standable(map) && !cell.Impassable(map))
                {
                    return cell;
                }
            }

            return LocalTargetInfo.Invalid;
        }

        private LocalTargetInfo GetAutoDestinationForAI()
        {
            IntVec3 origin = parent.pawn.Position;
            Map map = parent.pawn.Map;

            for (int i = 0; i < 30; i++)
            {
                IntVec3 cell = CellFinder.RandomClosewalkCellNear(origin, map, 8);
                if (cell.Standable(map))
                    return cell;
            }

            return LocalTargetInfo.Invalid;
        }

        private IntVec3 CellRandomizer(IntVec3 destination, int list)
        {
            int randx = Rand.RangeInclusive(1, list);
            destination.x = destination.x + randx;
            int randy = Rand.RangeInclusive(1, list);
            destination.y = destination.y + randy;
            int randz = Rand.RangeInclusive(1, list);
            destination.z = destination.z + randz;

            return destination;
        }

        public override bool CanHitTarget(LocalTargetInfo target)
        {
            if (!CanDeepStrikeSelectedTargetAt(target))
            {
                return false;
            }

            return base.CanHitTarget(target);
        }

        private bool CanDeepStrikeSelectedTargetAt(LocalTargetInfo target)
        {            
            IReadOnlyList<Pawn> readOnlyList = parent.pawn.Map.mapPawns.AllPawnsSpawned;
            List<Thing> hostiles = new List<Thing>();

            foreach (Pawn hostilePawns in readOnlyList)
            {
                if (hostilePawns.HomeFaction.HostileTo(parent.pawn.Faction))
                    hostiles.Add(hostilePawns);
            }

            Pawn pawn = selectedTarget.Pawn;
            if (pawn != null)
            {
                if (pawn.Spawned && !target.Cell.Impassable(parent.pawn.Map))
                {
                    foreach (Thing thing in hostiles)
                    {
                        if (DeepStrikeCalculator(thing.Position, target.Cell) <= Props.rangetoHostile * Props.rangetoHostile)
                        {
                            if (parent.pawn.Faction == Faction.OfPlayer)
                            {
                                Messages.Message(TooClosetoEnemy.Translate(parent.def.LabelCap.Named("ABILITY")), MessageTypeDefOf.RejectInput);
                            }
                            return false;
                        }
                    }
                    return target.Cell.WalkableBy(parent.pawn.Map, pawn);
                }
                return false;
            }

            return CanTeleportThingTo(target, parent.pawn.Map);
        }

        private float DeepStrikeCalculator(IntVec3 a, IntVec3 b)
        {
            float dx = b.x - a.x;
            float dz = b.z - a.z;
            return dx * dx + dz * dz;
        }

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            GenDraw.DrawRadiusRing(target.Cell, Props.rangetoHostile);
        }

        public override bool Valid(LocalTargetInfo target, bool showMessages = true)
        {
            AcceptanceReport acceptanceReport = CanDeepStrikeTarget(target);
            if (!acceptanceReport)
            {
                Pawn pawn;
                if (showMessages && !acceptanceReport.Reason.NullOrEmpty() && (pawn = target.Thing as Pawn) != null)
                {
                    Messages.Message(CannotDeepstriketoTarget.Translate(pawn.Named("PAWN"), acceptanceReport.Reason), pawn, MessageTypeDefOf.RejectInput, historical: false);
                }

                return false;
            }

            return base.Valid(target, showMessages);
        }

        private AcceptanceReport CanDeepStrikeTarget(LocalTargetInfo target)
        {
            Pawn pawn;
            if ((pawn = target.Thing as Pawn) != null)
            {
                if (pawn != parent.pawn && !parent.def.HasAreaOfEffect)
                {
                    return CanOnlyCastToSelf.Translate();
                }
            }
            return true;
        }

        public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
        {
            return CanDeepStrikeTarget(target).Reason;
        }

        [Multiplayer.SyncMethod]
        public static void SendDeepStrikeUsedSignal(LocalTargetInfo target, Thing initiator)
        {
            Find.SignalManager.SendSignal(new Signal(SkipUsedSignalTag, target.Named("POSITION"), initiator.Named("SUBJECT")));
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            if (parent.pawn.Faction == Faction.OfPlayer)
            {
                return false;
            }

            //if (parent.pawn.Faction != Faction.OfPlayer)
            //{
            //    return false;
            //}

            //Unreachable
            if (target.HasThing)
            {
                return false;
            }

            if (CanDeepStrikeSelectedTargetAt(target))
            {
                return true;
            }

            return false;
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (target.Pawn == parent.pawn)
            {
                return true;
            }

            return base.CanApplyOn(target, dest);
        }
    }
}
