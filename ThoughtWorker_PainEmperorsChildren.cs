using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class ThoughtWorker_PainEmperorsChildren : ThoughtWorker
    {
        public static ThoughtState CurrentThoughtState(Pawn p)
        {
            float painTotal = p.health.hediffSet.PainTotal;
            if (painTotal < 0.05f)
            {
                return ThoughtState.ActiveAtStage(0);
            }

            if (painTotal < 0.15f)
            {
                return ThoughtState.ActiveAtStage(1);
            }

            if (painTotal < 0.4f)
            {
                return ThoughtState.ActiveAtStage(2);
            }

            if (painTotal < 0.8f)
            {
                return ThoughtState.ActiveAtStage(3);
            }

            return ThoughtState.ActiveAtStage(4);
        }

        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (ThoughtUtility.ThoughtNullified(p, def))
            {
                return ThoughtState.Inactive;
            }

            return CurrentThoughtState(p);
        }
    }
}