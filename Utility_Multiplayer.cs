using MIM40kFactions.Compatibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using Verse;
using static HarmonyLib.Code;

namespace MIM40kFactions
{
    public static class Utility_Multiplayer
    {
        // Synchronize effecters for multiplayer
        [Multiplayer.SyncMethod]
        public static void SyncEffecter(EffecterDef effecterDef, Pawn caster, Pawn target)
        {
            if (effecterDef != null)
            {
                Effecter effecter = effecterDef.SpawnAttached(caster, caster.MapHeld);
                effecter.Trigger(caster, null);
                Effecter effecter2 = effecterDef.SpawnAttached(caster, caster.MapHeld);
                effecter2.Trigger(target, null);
            }
        }
    }
}
