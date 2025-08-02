using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions.Aeldari
{
    public class FocusStrengthOffset_SpiritStoneStored : FocusStrengthOffset
    {
        public override string GetExplanation(Thing parent)
        {
            if (CanApply(parent))
            {
                return "EMAE_StoredSpiritStoneFocusDesc".Translate() + ": " + GetOffset(parent).ToStringWithSign("0%");
            }

            return GetExplanationAbstract();
        }

        public override string GetExplanationAbstract(ThingDef def = null)
        {
            return "EMAE_StoredSpiritStoneFocusAbstract".Translate() + ": " + offset.ToStringWithSign("0%");
        }

        public override float GetOffset(Thing parent, Pawn user = null)
        {
            return offset;
        }

        public override bool CanApply(Thing parent, Pawn user = null)
        {
            if (!parent.Spawned || !(parent is Building_Grave grave))
                return false;

            return grave.GetDirectlyHeldThings()
                .OfType<ThingWithComps>()
                .Any(t => t.GetComp<CompSpiritStone>() != null);
        }
    }
}
