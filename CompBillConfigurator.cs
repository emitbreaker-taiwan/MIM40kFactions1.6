using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace MIM40kFactions
{
    public class CompBillConfigurator : ThingComp
    {
        public CompProperties_BillConfigurator Props => (CompProperties_BillConfigurator)props;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var g in base.CompGetGizmosExtra())
                yield return g;

            yield return new Command_Action
            {
                defaultLabel = Props.titleLabel.Translate(),
                defaultDesc = Props.defaultDescription.Translate(),
                icon = ContentFinder<Texture2D>.Get(Props.UIIconPath),
                action = () => Find.WindowStack.Add(new Dialog_BillConfigurator(
                    (Building_WorkTable)parent,
                    onAccept: () => Messages.Message(Props.configureApplied.Translate(), MessageTypeDefOf.PositiveEvent),
                    onCancel: () => Messages.Message(Props.configureCancelled.Translate(), MessageTypeDefOf.NeutralEvent)))
            };
        }
    }
}
