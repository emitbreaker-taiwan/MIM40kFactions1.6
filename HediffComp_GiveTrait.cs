using Verse;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using MIM40kFactions.Compatibility;

namespace MIM40kFactions
{
    
    public class HediffComp_GiveTrait : HediffComp
    {
        public HediffCompProperties_GiveTrait Props => (HediffCompProperties_GiveTrait)props;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            if (!Utility_PawnValidationManager.IsPawnDeadValidator(parent.pawn))
            {
                return;
            }

            if (Props.traitDef == null || parent.pawn.story.traits.HasTrait(Props.traitDef))
            {
                return;
            }

            if (Props.severityAmount != null && parent.pawn.health.hediffSet.GetFirstHediffOfDef(parent.def).Severity > Props.severityAmount)
            {
                int i = Props.degree.HasValue ? Props.degree.Value : Props.traitDef.degreeDatas.FirstOrDefault<TraitDegreeData>().degree;
                Trait t = new Trait(Props.traitDef, i);
                // Trait t = new Trait(Props.traitDef, Props.degreeData == null ? this.Props.traitDef.degreeDatas.FirstOrDefault<TraitDegreeData>().degree : this.Props.degreeData.degree);
                if (t != null)
                {
                    parent.pawn.story.traits.GainTrait(t, true);
                }
            }
        }

        public override void CompPostPostRemoved()
        {
            if (Props.traitDef != null)
            {
                Trait t = parent.pawn.story.traits.GetTrait(Props.traitDef);
                if (t != null)
                {
                    parent.pawn.story.traits.RemoveTrait(t);
                }
            }
        }
    }
}
