using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions.Aeldari
{
    public class FocusStrengthOffset_SpiritStoneRelationship : FocusStrengthOffset
    {
        public override bool DependsOnPawn => true;

        public override string GetExplanationAbstract(ThingDef def = null)
        {
            return "EMAE_StatsReport_SpiritStoneRelatedAbstract".Translate() + ": " + offset.ToStringWithSign("0%");
        }

        public override float GetOffset(Thing parent, Pawn user = null)
        {
            return offset;
        }

        public override bool CanApply(Thing parent, Pawn user = null)
        {
            if (!parent.Spawned || !(parent is Building_Grave grave) || user?.Name == null)
                return false;

            foreach (Thing thing in grave.GetDirectlyHeldThings())
            {
                var comp = thing.TryGetComp<CompSpiritStone>();
                if (comp?.relations != null)
                {
                    foreach (var rel in comp.relations)
                    {
                        if (rel.otherPawnName == user.Name.ToStringFull)
                            return true;
                    }
                }
            }

            return false;
        }
    }
}
