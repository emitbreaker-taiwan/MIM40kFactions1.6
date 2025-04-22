using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using static HarmonyLib.Code;

namespace MIM40kFactions
{
    public class CompGiveTraits : ThingComp
    {
        public CompProperties_GiveTraits Props => (CompProperties_GiveTraits)props;

        private List<TraitDef> originalTraitSet = new List<TraitDef>();

        public override void Notify_Equipped(Pawn pawn)
        {
            if (originalTraitSet.Count > 0)
            {
                originalTraitSet.Clear();
            }
            if (Props.traitDefSets == null)
            {
                return;
            }
            foreach (TraitDegreeSet traitdegree in Props.traitDefSets)
            {
                if (pawn.story.traits.HasTrait(traitdegree.traitDef))
                {
                    originalTraitSet.Add(traitdegree.traitDef);
                    continue;
                }
                int i = traitdegree.degree.HasValue ? traitdegree.degree.Value : traitdegree.traitDef.degreeDatas.FirstOrDefault<TraitDegreeData>().degree;
                Trait t = new Trait(traitdegree.traitDef, i);
                pawn.story.traits.GainTrait(t, true);
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            if (Props.traitDefSets != null)
            {
                foreach (TraitDegreeSet traitdegree in Props.traitDefSets)
                {
                    if (originalTraitSet != null && originalTraitSet.Contains(traitdegree.traitDef))
                    {
                        continue;
                    }
                    Trait t = pawn.story.traits.GetTrait(traitdegree.traitDef);
                    pawn.story.traits.RemoveTrait(t);
                }
            }
            if (originalTraitSet.Count > 0)
            {
                originalTraitSet.Clear();
            }
        }
    }
}