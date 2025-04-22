using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MIM40kFactions.Compatibility;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class NonEdibleIngredientValueGetter_Nutrition : IngredientValueGetter
    {
        public override float ValuePerUnitOf(ThingDef t)
        {
            NonEdibleIngredientExtension modExtension = t.GetModExtension<NonEdibleIngredientExtension>();
            if (modExtension == null)
            {
                if (!t.IsNutritionGivingIngestible)
                {
                    return 0f;
                }

                return t.GetStatValueAbstract(StatDefOf.Nutrition);
            }

            return modExtension.nutritionValue;
        }

        public override string BillRequirementsDescription(RecipeDef r, IngredientCount ing)
        {
            return ing.GetBaseCount() + "x " + "BillNutrition".Translate() + " (" + ing.filter.Summary + ")";
        }
    }
}
