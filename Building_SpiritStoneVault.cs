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

        private Graphic cachedGraphic;

        public override Graphic Graphic
        {
            get
            {
                if (cachedGraphic == null)
                {
                    if (innerContainer.Any(t => t.TryGetComp<CompSpiritStone>() != null))
                    {
                        cachedGraphic = def.building.fullGraveGraphicData.GraphicColoredFor(this);
                    }
                    else
                    {
                        cachedGraphic = base.Graphic;
                    }
                }

                return cachedGraphic;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref everNonEmpty, "everNonEmpty", defaultValue: false);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                cachedGraphic = null;
            }
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
                cachedGraphic = null;
                return true;
            }

            return false;
        }

        public override void Notify_HauledTo(Pawn hauler, Thing thing, int count)
        {
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
    }
}
