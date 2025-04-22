using Verse;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;
using System.Collections;

namespace MIM40kFactions
{
    public class HediffComp_GiveSecondaryTrait : HediffComp
    {
        public HediffCompProperties_GiveSecondaryTrait Props => (HediffCompProperties_GiveSecondaryTrait)props;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
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