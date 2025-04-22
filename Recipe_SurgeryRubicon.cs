using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using static UnityEngine.GraphicsBuffer;

namespace MIM40kFactions
{
    public class Recipe_SurgeryRubicon : Recipe_InstallImplant
    {
        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            base.AvailableOnNow(thing, part);

            if ((thing is Pawn pawn))
            {
                RecipeExtension modExtension = recipe.GetModExtension<RecipeExtension>();
                if (modExtension == null)
                {
                    return true;
                }
                
                if (modExtension != null)
                {
                    if (modExtension.requiredHediffDefs != null)
                    {
                        bool flag = false;

                        for (int i = 0; i < modExtension.requiredHediffDefs.Count; i++)
                        {
                            if (pawn.health.hediffSet.GetFirstHediffOfDef(modExtension.requiredHediffDefs[i]) != null)
                            {
                                flag = true;
                            }
                        }

                        if (!flag)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}
