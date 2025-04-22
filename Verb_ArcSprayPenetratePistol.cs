using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MIM40kFactions
{
    public class Verb_ArcSprayPenetratePistol : Verb_ArcSprayPenetrate
    {
        public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
        {
            if (!targ.IsValid || !CanHitTarget(targ))
                return false;

            if (targ.Cell.AdjacentToCardinal(root))
                return true;

            return base.CanHitTargetFrom(root, targ);
        }

        public override bool CanHitTarget(LocalTargetInfo target)
        {
            return base.CanHitTarget(target)
                   && caster is Pawn pawn
                   && !pawn.Downed
                   && target.IsValid
                   && (!target.HasThing || !target.Thing.Destroyed);
        }
    }
}
