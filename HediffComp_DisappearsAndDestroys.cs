using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MIM40kFactions
{
    public class HediffComp_DisappearsAndDestroys : HediffComp_DisappearsDisableable
    {
        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            if (!disabled && ticksToDisappear <= 0 && !base.Pawn.Dead)
            {
                Pawn.Destroy();
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref disabled, "disabled", defaultValue: false);
        }
    }
}