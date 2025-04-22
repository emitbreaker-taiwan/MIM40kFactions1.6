using System;
using Verse;
using RimWorld;
using System.Reflection;
using static UnityEngine.GraphicsBuffer;

namespace MIM40kFactions
{
    public class CompCauseHediff_AppareltoBodyPartGroup : ThingComp
    {
        private CompProperties_CauseHediff_AppareltoBodyPartGroup Props => (CompProperties_CauseHediff_AppareltoBodyPartGroup)props;

        private int i = 0;

        public override void Notify_Equipped(Pawn pawn)
        {
            i = 0;
            if (Props.parts == null)
            {
                Props.parts.Add(pawn.health.hediffSet.GetBrain().def);
            }

            foreach (BodyPartDef part in Props.parts)
            {
                if (!pawn.RaceProps.body.GetPartsWithDef(part).EnumerableNullOrEmpty<BodyPartRecord>() && i <= pawn.RaceProps.body.GetPartsWithDef(part).Count)
                {
                    if (Props.hediff != null)
                    {
                        if (pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediff) == null)
                        {
                            HediffComp_RemoveIfApparelDropped hediffComp_RemoveIfApparelDropped = pawn.health.AddHediff(Props.hediff, pawn.RaceProps.body.GetPartsWithDef(part).ToArray()[i]).TryGetComp<HediffComp_RemoveIfApparelDropped>();
                            if (hediffComp_RemoveIfApparelDropped != null)
                            {
                                hediffComp_RemoveIfApparelDropped.wornApparel = (Apparel)parent;
                            }
                        }
                    }
                    if (Props.hediffDefs != null)
                    {
                        foreach (HediffDef hediffdef in Props.hediffDefs)
                        {
                            HediffComp_RemoveIfApparelDropped hediffComp_RemoveIfApparelDropped = pawn.health.AddHediff(hediffdef, pawn.RaceProps.body.GetPartsWithDef(part).ToArray()[i]).TryGetComp<HediffComp_RemoveIfApparelDropped>();
                            if (hediffComp_RemoveIfApparelDropped != null)
                            {
                                hediffComp_RemoveIfApparelDropped.wornApparel = (Apparel)parent;
                            }
                        }
                    }
                    ++i;
                }
            }
        }
    }
}