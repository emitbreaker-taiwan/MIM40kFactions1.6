using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions.Servitors
{
    public class Recipe_InstallImplant_Servitor : Recipe_InstallImplant
    {
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            base.ApplyOnPawn(pawn, part, billDoer, ingredients, bill);

            if (pawn.workSettings == null)
            {
                return;
            }

            pawn.workSettings.EnableAndInitializeIfNotAlreadyInitialized();

            foreach (var workType in DefDatabase<WorkTypeDef>.AllDefsListForReading)
            {
                if (pawn.WorkTypeIsDisabled(workType)) continue;
                pawn.workSettings.SetPriority(workType, 0);
            }
        }
    }
}
