using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions.Aeldari
{
    public class Building_SpiritStoneVault : Building_Grave
    {
        private bool everNonEmpty;

        private Graphic cachedGraphicFull;

        private bool artInitialized = false;

        public override bool StorageTabVisible => true;

        public override Graphic Graphic
        {
            get
            {
                if (HasAnyContents)
                {
                    if (innerContainer.Any(t => t.TryGetComp<CompSpiritStone>() == null))
                    {
                        return base.Graphic;
                    }

                    if (cachedGraphicFull == null)
                    {
                        cachedGraphicFull = def.building.fullGraveGraphicData.GraphicColoredFor(this);
                    }

                    return cachedGraphicFull;
                }

                return base.Graphic;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref everNonEmpty, "everNonEmpty", defaultValue: false);
            Scribe_Values.Look(ref artInitialized, "artInitialized", false);
        }

        public override bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
        {
            if (thing.TryGetComp<CompSpiritStone>() == null)
            {
                Messages.Message("EMAE_OnlySpiritStoneAllowed".Translate(), MessageTypeDefOf.RejectInput, false);
                return false;
            }

            int maxAllowed = def.building?.maxItemsInCell ?? 1;
            if (innerContainer.Count >= maxAllowed)
            {
                Messages.Message("EMAE_TooManySouls".Translate(), MessageTypeDefOf.RejectInput, false);
                return false;
            }

            if (base.TryAcceptThing(thing, allowSpecialEffects))
            {
                everNonEmpty = true;

                return true;
            }

            return false;
        }

        public override void Notify_HauledTo(Pawn hauler, Thing thing, int count)
        {
            if (!artInitialized)
            {
                artInitialized = true;
                CompArt comp = GetComp<CompArt>();
                if (comp != null && !comp.Active && hauler.RaceProps.Humanlike)
                {
                    comp.JustCreatedBy(hauler);
                    comp.InitializeArt(thing);
                }
            }

            var compSpiritStone = thing.TryGetComp<CompSpiritStone>();
            if (compSpiritStone == null || compSpiritStone.alreadyHonored) return;

            compSpiritStone.alreadyHonored = true;

            ThoughtDef thought = DefDatabase<ThoughtDef>.GetNamedSilentFail("EMAE_KnowSoulPreserved");
            if (thought == null) return;

            foreach (Pawn colonist in Map.mapPawns.FreeColonists)
            {
                if (colonist.needs?.mood != null)
                {
                    colonist.needs.mood.thoughts.memories.TryGainMemory(thought);
                }
            }

            base.Map.mapDrawer.MapMeshDirty(base.Position, (ulong)MapMeshFlagDefOf.Buildings | (ulong)MapMeshFlagDefOf.Things);
            hauler.records.Increment(RecordDefOf.CorpsesBuried);
            TaleRecorder.RecordTale(TaleDefOf.BuriedCorpse, hauler, compSpiritStone?.pawnNameFull);
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.GetInspectString());

            var storedStones = innerContainer
                .Where(t => t.TryGetComp<CompSpiritStone>() != null)
                .Select(t => t.TryGetComp<CompSpiritStone>())
                .ToList();

            if (storedStones.Any())
            {
                sb.AppendLine();
                sb.AppendLine("EMAE_StoredSouls".Translate());

                foreach (var stone in storedStones)
                {
                    sb.AppendLine(" - " + (stone.pawnNameFull ?? "Unknown"));
                }
            }

            return sb.ToString().TrimEnd();
        }

        public override bool Accepts(Thing thing)
        {
            if (thing?.TryGetComp<CompSpiritStone>() == null) return false;

            int maxAllowed = def.building?.maxItemsInCell ?? 1;

            return innerContainer.Count < maxAllowed;
        }

        public ThingOwner GetInnerContainer()
        {
            return innerContainer;
        }
    }
}
