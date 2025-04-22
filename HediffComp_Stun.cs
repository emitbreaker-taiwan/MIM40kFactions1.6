using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using Verse;
using MIM40kFactions.Compatibility;

namespace MIM40kFactions
{
    public class HediffComp_Stun : HediffComp
    {
        public HediffCompProperties_Stun Props => (HediffCompProperties_Stun)props;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);

            parent.pawn.stances.stunner.StunFor(Props.stunSeconds.SecondsToTicks(), parent.pawn, addBattleLog: true);
        }
    }
}
