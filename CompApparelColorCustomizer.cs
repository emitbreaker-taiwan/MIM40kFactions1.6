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
    public class CompApparelColorCustomizer : ThingComp
    {
        public CompProperties_ApparelColorCustomizer Props => (CompProperties_ApparelColorCustomizer)props;

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn pawn)
        {
            if (pawn.apparel == null || !pawn.apparel.WornApparel.Any(IsCustomizableApparel))
            {
                yield return new FloatMenuOption("EMWH_ApparelColorCustomizer_NoApparelToColor".Translate(), null);
                yield break;
            }

            yield return new FloatMenuOption("EMWH_ApparelColorCustomizer_CustomizeApparelColors".Translate(), () =>
            {
                Find.WindowStack.Add(new Dialog_ApparelColorCustomizer(pawn, parent));
            });
        }

        private bool IsCustomizableApparel(Apparel apparel)
        {
            return apparel.def.HasModExtension<MIM40kFactions.PowerArmorCustomizationExtension>();
        }
    }
}
